using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Ninject;
using PanelSW.Installer.JetBA;
using PanelSW.Installer.JetBA.JetPack.Util;
using PanelSW.Installer.JetBA.JetPack.ViewModel;
using PanelSW.Installer.JetBA.Util;
using PanelSW.Installer.JetBA.ViewModel;
using System.Collections.Generic;

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
            if (Engine.StringVariables.Contains("BaLaunchDebugger") && Engine.BooleanVariable("BaLaunchDebugger"))
            {
                System.Diagnostics.Debugger.Launch();
            }
            base.Run();
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
                    if (bi.PersistedVariables.ContainsKey(s) && !string.IsNullOrEmpty(bi.PersistedVariables[s]) && !vars[s].IsOnCommandLine)
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

        protected override void OnShutdown(ShutdownEventArgs args)
        {
            FinishViewModelEx finish = Kernel.Get<FinishViewModelEx>();
            finish.ZipLogFilesCommand.Execute(null);

            base.OnShutdown(args);
        }
    }
}