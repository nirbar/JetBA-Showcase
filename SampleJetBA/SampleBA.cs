using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PanelSW.Installer.JetBA;
using PanelSW.Installer.JetBA.JetPack.Util;
using PanelSW.Installer.JetBA.JetPack.ViewModel;
using PanelSW.Installer.JetBA.Util;
using PanelSW.Installer.JetBA.ViewModel;
using SampleJetBA.View;
using SampleJetBA.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using WixToolset.Mba.Core;

namespace SampleJetBA
{
    public class SampleBA : JetBootstrapperApplication
    {
        public SampleBA(IEngine engine, IBootstrapperCommand command)
            : base(engine, command)
        {
        }

        protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            base.ConfigureServices(context, services);

            services.RemoveAll(typeof(VariablesViewModel));
            services.AddSingleton<VariablesViewModel, ViewModel.VariablesViewModelEx>();

            services.AddSingleton<NavigationViewModel, ViewModel.NavigationViewModelEx>();

            services.RemoveAll(typeof(InputValidationsViewModel));
            services.AddSingleton<InputValidationsViewModel, ViewModel.InputValidationsViewModelEx>();

            services.AddTransient<DatabaseView>();
            services.AddTransient<DetectingView>();
            services.AddTransient<FinishView>();
            services.AddTransient<HelpView>();
            services.AddTransient<InstallLocationView>();
            services.AddTransient<PageSelectionView>();
            services.AddTransient<ProgressView>();
            services.AddTransient<RepairView>();
            services.AddTransient<ServiceAccountView>();
            services.AddTransient<SummaryView>();
            services.AddSingleton<Window, RootView>();
        }

        protected override void OnResolveCulture(ResolveCultureEventArgs args)
        {
            base.OnResolveCulture(args);
            Properties.Resources.Culture = args.CultureInfo;
            PanelSW.Installer.JetBA.JetPack.Properties.Resources.Culture = args.CultureInfo;
            //TODO Keep? FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(args.CultureInfo.IetfLanguageTag)));
        }

        [STAThread]
        protected override void Run()
        {
            VariablesViewModelEx vars = Kernel.Get<VariablesViewModelEx>();
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
                ICommand exit = new RelayCommand(o => InvokeShutdown());
                if (cmd != null)
                {
                    Engine.Log(LogLevel.Standard, $"Prompting to resume with {Command.Action} after an interrupted reboot");

                    PopupViewModel popup = Kernel.Get<PopupViewModel>();
                    popup.Show(nameof(Properties.Resources.Resume), nameof(Properties.Resources.InterruptedRebootPrompt0), PopupViewModel.IconHint.Question
                        , nameof(Properties.Resources.Install), cmd
                        , nameof(Properties.Resources.Cancel), exit);
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

                VariablesViewModelEx vars = Kernel.Get<VariablesViewModelEx>();
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
                VariablesViewModel vars = Kernel.Get<VariablesViewModel>();
                PopupViewModel.Buttons res = popup.ShowSync(Properties.Resources.Restart, nameof(Properties.Resources.WeNeedToRebootNow0), PopupViewModel.IconHint.Information
                    , nameof(Properties.Resources.Yes)
                    , nameof(Properties.Resources.No)
                    , null
                    , PopupViewModel.Buttons.Right
                    , vars["WixBundleName"].String);

                if (res != PopupViewModel.Buttons.Right)
                {
                    Engine.Log(LogLevel.Standard, "User selected to delay reboot");
                    apply.RebootState = ApplyRestart.RestartRequired;
                }
            }
        }

        protected override void OnShutdown(ShutdownEventArgs args)
        {
            FinishViewModelEx finish = Kernel.Get<FinishViewModelEx>();
            finish.ZipLogFiles();

            base.OnShutdown(args);
        }
    }
}
