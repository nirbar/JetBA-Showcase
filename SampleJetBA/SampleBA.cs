using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PanelSW.Installer.JetBA;
using PanelSW.Installer.JetBA.JetPack;
using PanelSW.Installer.JetBA.JetPack.Util;
using PanelSW.Installer.JetBA.JetPack.ViewModel;
using PanelSW.Installer.JetBA.Util;
using PanelSW.Installer.JetBA.ViewModel;
using SampleJetBA.View;
using SampleJetBA.ViewModel;
using System.Windows;
using System.Windows.Input;
using WixToolset.Mba.Core;

namespace SampleJetBA
{
    public class SampleBA : JetPackBootstrapperApplication
    {
        public SampleBA(IEngine engine, IBootstrapperCommand command)
            : base(engine, command)
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

            services.RemoveAll(typeof(PanelSW.Installer.JetBA.Localization.Resources));
            services.RemoveAll(typeof(PanelSW.Installer.JetBA.JetPack.Localization.Resources));
            services.AddSingleton<Localization.Resources>();
            services.AddSingleton<PanelSW.Installer.JetBA.JetPack.Localization.Resources, Localization.Resources>(s => s.GetService<Localization.Resources>());
            services.AddSingleton<PanelSW.Installer.JetBA.Localization.Resources, Localization.Resources>(s => s.GetService<Localization.Resources>());

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
                ApplyViewModel apply = GetService<ApplyViewModel>();
                ICommand cmd = apply.GetCommand(_command.Action);
                ICommand exit = new RelayCommand(o => InvokeShutdown());
                if (cmd != null)
                {
                    engine.Log(LogLevel.Standard, $"Prompting to resume with {_command.Action} after an interrupted reboot");

                    PopupViewModel popup = GetService<PopupViewModel>();
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
                BundleSearch bs = GetService<BundleSearch>();
                PanelSW.Installer.JetBA.JetPack.Util.BundleInfo bi = bs.LoadByBundleId(args.ProductCode, args.PerMachine);
                engine.Log(LogLevel.Standard, $"Copying variables from {bi.Name} v{bi.Version}");

                VariablesViewModelEx vars = GetService<VariablesViewModelEx>();
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
            if ((args.Restart == ApplyRestart.RestartInitiated) && (_command.Display == Display.Full))
            {
                PopupViewModel popup = GetService<PopupViewModel>();
                ApplyViewModel apply = GetService<ApplyViewModel>();
                VariablesViewModel vars = GetService<VariablesViewModel>();
                PopupViewModel.Buttons res = popup.ShowSync(Properties.Resources.Restart, nameof(Properties.Resources.WeNeedToRebootNow0), PopupViewModel.IconHint.Information
                    , nameof(Properties.Resources.Yes)
                    , nameof(Properties.Resources.No)
                    , null
                    , PopupViewModel.Buttons.Right
                    , vars["WixBundleName"].String);

                if (res != PopupViewModel.Buttons.Right)
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
