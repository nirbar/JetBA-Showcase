using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Ninject;
using PanelSW.Installer.JetBA;
using PanelSW.Installer.JetBA.JetPack.Util;
using PanelSW.Installer.JetBA.JetPack.ViewModel;
using PanelSW.Installer.JetBA.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace SampleJetBA
{
    public class SampleBA : JetBootstrapperApplication
    {
        private SampleNInjectBinder binder_ = null;
        protected override NInjectBinder Binder => binder_ ?? (binder_ = new SampleNInjectBinder(this));

        protected override void OnResolveCulture(ResolveCultureEventArgs args)
        {
            base.OnResolveCulture(args);
            Properties.Resources.Culture = args.CultureInfo;
            PanelSW.Installer.JetBA.JetPack.Properties.Resources.Culture = args.CultureInfo;
        }

        protected override void Run()
        {
            JetBundleVariables.BundleVariablesViewModel vars = Kernel.Get<JetBundleVariables.BundleVariablesViewModel>();
            if (vars.BaLaunchDebugger.Exists && vars.BaLaunchDebugger.Boolean)
            {
                System.Diagnostics.Debugger.Launch();
            }

            // Allow a single concurrent instance of this bootstrapper system-wide.
            string mutexName = $"Global\\{vars.WixBundleProviderKey.String}.{vars.WixBundleVersion.String}";
            using (Mutex mutex = new Mutex(false, mutexName))
            {
                try
                {
                    if (!mutex.WaitOne(0))
                    {
                        Engine.Log(LogLevel.Error, "Exiting because another instance of this installation is already running");
                        Engine.Quit(-1);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Engine.Log(LogLevel.Error, $"Failed checking whether another instance of this installation is already running. {ex.Message}. Ignoring...");
                }
                base.Run();
            }
        }

        protected override void OnDetectBegin(DetectBeginEventArgs args)
        {
            if (HasJetBaExecuted)
            {
                ApplyViewModel apply = Kernel.Get<ApplyViewModel>();
                apply.PlanAfterReboot = true;
            }
            base.OnDetectBegin(args);
        }

        protected override void OnDetectComplete(DetectCompleteEventArgs args)
        {
            base.OnDetectComplete(args);

            if (HasJetBaExecuted && (Command.Resume == ResumeType.Interrupted) && (Kernel.Get<Display>() == Display.Full))
            {
                ApplyViewModel apply = Kernel.Get<ApplyViewModel>();
                ICommand cmd = apply.GetCommand(Command.Action);
                if (cmd != null)
                {
                    Engine.Log(LogLevel.Standard, $"Prompting to resume with {Command.Action} after an interrupted reboot");

                    PanelSW.Installer.JetBA.Localization.Resources local = Kernel.Get<PanelSW.Installer.JetBA.Localization.Resources>();
                    PopupViewModel popup = Kernel.Get<PopupViewModel>();
                    popup.Show(Properties.Resources.Resume, Properties.Resources.InterruptedRebootPrompt, PopupViewModel.IconHint.Question
                        , local.Yes, cmd
                        , local.No);
                }
            }
        }

        protected override void OnDetectRelatedBundle(DetectRelatedBundleEventArgs args)
        {
            base.OnDetectRelatedBundle(args);
            if ((args.RelationType == RelationType.Upgrade) || (args.RelationType == RelationType.Update))
            {
                BundleSearch bs = Kernel.Get<BundleSearch>();
                BundleInfo bi = bs.LoadByBundleId(args.ProductCode, args.PerMachine);
                Engine.Log(LogLevel.Standard, $"Copying variables from {bi.Name} v{bi.Version}");

                JetBundleVariables.BundleVariablesViewModel vars = Kernel.Get<JetBundleVariables.BundleVariablesViewModel>();
                foreach (string s in vars.VariableNames)
                {
                    if (bi.PersistedVariables.ContainsKey(s) && !vars.BuiltinVariableNames.Contains(s) && !string.IsNullOrEmpty(bi.PersistedVariables[s]) && !vars[s].IsOnCommandLine)
                    {
                        vars[s].String = bi.PersistedVariables[s];
                    }
                }

                if (bi.PersistedVariables.ContainsKey("JetBA_Encrypted_Variables") && !string.IsNullOrEmpty(bi.PersistedVariables["JetBA_Encrypted_Variables"]))
                {
                    Dictionary<string, string> passwords = vars.DecryptAll(bi.PersistedVariables["JetBA_Encrypted_Variables"]);
                    foreach (string s in passwords.Keys)
                    {
                        if (!string.IsNullOrEmpty(passwords[s]) && !vars[s].IsOnCommandLine)
                        {
                            vars[s].String = passwords[s];
                        }
                    }
                }
            }
        }

        // Prompt when need to reboot midway
        protected override void OnApplyComplete(ApplyCompleteEventArgs args)
        {
            base.OnApplyComplete(args);
            if ((args.Restart == ApplyRestart.RestartInitiated) && (Kernel.Get<Display>() == Display.Full))
            {
                PopupViewModel popup = Kernel.Get<PopupViewModel>();
                ApplyViewModel apply = Kernel.Get<ApplyViewModel>();
                PanelSW.Installer.JetBA.Localization.Resources local = Kernel.Get<PanelSW.Installer.JetBA.Localization.Resources>();
                popup.ShowSync(Properties.Resources.Restart, string.Format(Properties.Resources.WeNeedToRebootNow_0WillContinueAfterYouLoginAgain, apply.PlannedAction), PopupViewModel.IconHint.Information, local.OK);
            }
        }

        protected override void OnShutdown(ShutdownEventArgs args)
        {
            FinishViewModelEx finish = Kernel.Get<FinishViewModelEx>();
            finish.ZipLogFilesCommand.Execute(null);

            base.OnShutdown(args);
        }
    }
}