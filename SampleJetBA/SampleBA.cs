using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PanelSW.Installer.JetBA;
using PanelSW.Installer.JetBA.JetPack;
using PanelSW.Installer.JetBA.JetPack.Util;
using PanelSW.Installer.JetBA.JetPack.ViewModel;
using PanelSW.Installer.JetBA.ViewModel;
using SampleJetBA.View;
using SampleJetBA.ViewModel;
using System.Windows;
using WixToolset.BootstrapperApplicationApi;

namespace SampleJetBA
{
    public class SampleBA : JetPackBootstrapperApplication
    {
        public enum JetBaShowcaseErrors
        {
            // The process terminated unexpectedly.
            ERROR_PROCESS_ABORTED = 1067,
        }

        public SampleBA()
        {
        }

        protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            base.ConfigureServices(context, services);

            services.RemoveAll(typeof(VariablesViewModel));
            services.AddSingleton<VariablesViewModelEx>();
            services.AddSingleton<VariablesViewModel, VariablesViewModelEx>(s => s.GetService<VariablesViewModelEx>());

            services.AddSingleton<NavigationViewModelEx>();
            services.AddSingleton<NavigationViewModel, NavigationViewModelEx>(s => s.GetService<NavigationViewModelEx>());

            services.RemoveAll(typeof(InputValidationsViewModel));
            services.AddSingleton<InputValidationsViewModelEx>();
            services.AddSingleton<InputValidationsViewModel, InputValidationsViewModelEx>(s => s.GetService<InputValidationsViewModelEx>());

            services.AddSingleton<System.Resources.ResourceManager>(s => Properties.Resources.ResourceManager);

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
            VariablesViewModelEx vars = GetService<VariablesViewModelEx>();
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
                        engine.Log(LogLevel.Error, "Exiting because another instance of this installation is already running");
                        engine.Quit(-1);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    engine.Log(LogLevel.Error, $"Failed checking whether another instance of this installation is already running. {ex.Message}. Ignoring...");
                }
                base.Run();
            }
        }

        protected override void OnDetectBegin(DetectBeginEventArgs args)
        {
            if (HasJetBaExecuted)
            {
                ApplyViewModel apply = GetService<ApplyViewModel>();
                apply.PlanAfterReboot = true;
            }
            base.OnDetectBegin(args);
        }

        protected override void OnDetectComplete(DetectCompleteEventArgs args)
        {
            base.OnDetectComplete(args);

            if (HasJetBaExecuted && (_command.Resume == ResumeType.Interrupted) && (_command.Display == Display.Full))
            {
                if (_command.Action != LaunchAction.Unknown)
                {
                    engine.Log(LogLevel.Standard, $"Prompting to resume with {_command.Action} after an interrupted reboot");

                    PopupViewModel popup = GetService<PopupViewModel>();
                    VariablesViewModelEx vars = GetService<VariablesViewModelEx>();
                    Task<Result> task = popup.Show((int)JetBaShowcaseErrors.ERROR_PROCESS_ABORTED, PopupViewModel.UIHintFlags.MB_ICONQUESTION | PopupViewModel.UIHintFlags.MB_OKCANCEL | PopupViewModel.UIHintFlags.MB_DEFBUTTON1, nameof(Properties.Resources.InterruptedRebootPrompt0), vars.WixBundleName);
                    task.ContinueWith(t =>
                    {
                        if (t.Result == Result.Ok)
                        {
                            ApplyViewModel apply = GetService<ApplyViewModel>();
                            apply.Plan(_command.Action);
                        }
                        else
                        {
                            InvokeShutdown();
                        }
                    });
                }
            }
        }

        protected override void OnDetectRelatedBundle(DetectRelatedBundleEventArgs args)
        {
            base.OnDetectRelatedBundle(args);
            if ((args.RelationType == RelationType.Upgrade) || (args.RelationType == RelationType.Update))
            {
                BundleSearch bs = GetService<BundleSearch>();
                PanelSW.Installer.JetBA.JetPack.Util.BundleInfo bi = bs.LoadByBundleId(args.ProductCode, args.PerMachine);
                engine.Log(LogLevel.Standard, $"Copying variables from {bi.Name} v{bi.Version}");

                VariablesViewModelEx vars = GetService<VariablesViewModelEx>();
                foreach (string s in vars.VariableNames)
                {
                    if (bi.PersistedVariables.ContainsKey(s) && !VariablesViewModelEx.BuiltinVariableNames.Contains(s) && !string.IsNullOrEmpty(bi.PersistedVariables[s]) && !vars[s].IsOnCommandLine)
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
            if ((args.Restart == ApplyRestart.RestartInitiated) && (_command.Display == Display.Full))
            {
                PopupViewModel popup = GetService<PopupViewModel>();
                ApplyViewModel apply = GetService<ApplyViewModel>();
                VariablesViewModelEx vars = GetService<VariablesViewModelEx>();
                Result res = popup.ShowSync((int)ErrorCodes.ERROR_SUCCESS_REBOOT_REQUIRED, PopupViewModel.UIHintFlags.MB_ICONINFORMATION | PopupViewModel.UIHintFlags.MB_YESNO | PopupViewModel.UIHintFlags.MB_DEFBUTTON1, nameof(Properties.Resources.WeNeedToRebootNow0), vars.WixBundleName.String);
                if (res != Result.Yes)
                {
                    engine.Log(LogLevel.Standard, "User selected to delay reboot");
                    apply.RebootState = ApplyRestart.RestartRequired;
                }
            }
        }

        protected override void OnShutdown(ShutdownEventArgs args)
        {
            FinishViewModelEx finish = GetService<FinishViewModelEx>();
            finish.ZipLogFiles();

            base.OnShutdown(args);
        }
    }
}
