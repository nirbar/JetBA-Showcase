using PanelSW.Installer.JetBA;
using PanelSW.Installer.JetBA.JetPack;
using PanelSW.Installer.JetBA.ViewModel;
using SampleJetBA.View;
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using WixToolset.Mba.Core;

namespace SampleJetBA.ViewModel
{
    public enum Pages
    {
        Unknown,
        Detecting,
        PageSelection,
        InstallLocation,
        Service,
        Database,
        Summary,
        Progress,
        Finish,
        Repair,
        Help
    }

    public class NavigationViewModelEx : NavigationViewModel
    {
        private readonly Dispatcher _uiDispatcher;

        public NavigationViewModelEx(SampleBA ba, IBootstrapperCommand command, IEngine engine, JetPackActivator activator)
            : base(ba, command, engine, activator)
        {
            _ba.DetectComplete += BA_DetectComplete;
            _ba.ApplyBegin += BA_ApplyBegin;
            _ba.ApplyComplete += BA_ApplyComplete;

            _uiDispatcher = _activator.GetService<Dispatcher>();
            _uiDispatcher.Invoke(() => ExpectedPages = new ObservableCollection<Pages>());

            ApplyViewModel apply = _activator.GetService<ApplyViewModel>();
            apply.PropertyChanged += apply_PropertyChanged;

            VariablesViewModelEx vars = _activator.GetService<VariablesViewModelEx>();
            vars.CONFIGURE_SQL.PropertyChanged += CONFIGURE_SQL_PropertyChanged;
            vars.CONFIGURE_SERVICE_ACCOUNT.PropertyChanged += CONFIGURE_SERVICE_ACCOUNT_PropertyChanged;

            SetStartPage();
        }

        protected override object GetView(object key)
        {
            switch ((Pages)key)
            {
                case Pages.Detecting:
                    return _activator.GetService<DetectingView>();
                case Pages.PageSelection:
                    return _activator.GetService<PageSelectionView>();
                case Pages.InstallLocation:
                    return _activator.GetService<InstallLocationView>();
                case Pages.Service:
                    return _activator.GetService<ServiceAccountView>();
                case Pages.Database:
                    return _activator.GetService<DatabaseView>();
                case Pages.Summary:
                    return _activator.GetService<SummaryView>();
                case Pages.Progress:
                    return _activator.GetService<ProgressView>();
                case Pages.Finish:
                    return _activator.GetService<FinishView>();
                case Pages.Repair:
                    return _activator.GetService<RepairView>();
                case Pages.Help:
                    return _activator.GetService<HelpView>();
                default:
                    return null;
            }
        }

        #region Event-based navigations

        public void SetStartPage()
        {
            if (_command.Display < Display.Passive)
            {
                return;
            }

            if (_command.Action == LaunchAction.Help)
            {
                Page = Pages.Help;
                return;
            }

            ApplyViewModel apply = _activator.GetService<ApplyViewModel>();
            if (apply.InstallState < InstallationState.Detected)
            {
                Page = Pages.Detecting;
                return;
            }
            if (apply.DetectState.HasFlag(DetectionState.Present))
            {
                _uiDispatcher.Invoke(() =>
                {
                    ExpectedPages.Add(Pages.Repair);
                    ExpectedPages.Add(Pages.Progress);
                    ExpectedPages.Add(Pages.Finish);
                });
                Page = Pages.Repair;
            }
            else
            {
                _uiDispatcher.Invoke(() =>
                {
                    ExpectedPages.Add(Pages.PageSelection);
                    ExpectedPages.Add(Pages.InstallLocation);
                    ExpectedPages.Add(Pages.Service);
                    ExpectedPages.Add(Pages.Database);
                    ExpectedPages.Add(Pages.Summary);
                    ExpectedPages.Add(Pages.Progress);
                    ExpectedPages.Add(Pages.Finish);
                });
                Page = Pages.PageSelection;
            }

            VariablesViewModelEx vars = _activator.GetService<VariablesViewModelEx>();
            if (!vars.ForcePage.IsNullOrEmpty && Enum.TryParse(vars.ForcePage.String, out Pages page))
            {
                Page = page;
            }
        }

        private void BA_DetectComplete(object sender, DetectCompleteEventArgs e)
        {
            SetStartPage();
        }

        private void BA_ApplyBegin(object sender, ApplyBeginEventArgs e)
        {
            ClearHistory();
            Page = Pages.Progress;
        }

        private void BA_ApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            ClearHistory();
            Page = Pages.Finish;
        }

        private void apply_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyViewModel apply = _activator.GetService<ApplyViewModel>();
            if (e.PropertyName.Equals("InstallState") && (apply.InstallState >= InstallationState.Applied))
            {
                ClearHistory();
                Page = Pages.Finish;
            }
        }

        #endregion

        public ObservableCollection<Pages> ExpectedPages { get; private set; }
                
        public override void Refresh()
        {
            base.Refresh();
            _uiDispatcher.Invoke(() => ExpectedPages = new ObservableCollection<Pages>(ExpectedPages));
            OnPropertyChanged(nameof(ExpectedPages));
        }

        private void CONFIGURE_SQL_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals("BooleanString") || (_command.Display < Display.Passive))
            {
                return;
            }

            VariablesViewModelEx vars = _activator.GetService<VariablesViewModelEx>();
            _uiDispatcher.Invoke(() =>
            {
                if (vars.CONFIGURE_SQL.BooleanString && !ExpectedPages.Contains(Pages.Database))
                {
                    ExpectedPages.Insert(ExpectedPages.IndexOf(Pages.Summary), Pages.Database);
                }
                else if (!vars.CONFIGURE_SQL.BooleanString && ExpectedPages.Contains(Pages.Database))
                {
                    ExpectedPages.Remove(Pages.Database);
                }
            });
        }

        private void CONFIGURE_SERVICE_ACCOUNT_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals("BooleanString") || (_command.Display < Display.Passive))
            {
                return;
            }

            VariablesViewModelEx vars = _activator.GetService<VariablesViewModelEx>();
            _uiDispatcher.Invoke(() =>
            {
                if (vars.CONFIGURE_SERVICE_ACCOUNT.BooleanString && !ExpectedPages.Contains(Pages.Service))
                {
                    ExpectedPages.Insert(ExpectedPages.IndexOf(Pages.InstallLocation) + 1, Pages.Service);
                }
                else if (!vars.CONFIGURE_SERVICE_ACCOUNT.BooleanString && ExpectedPages.Contains(Pages.Service))
                {
                    ExpectedPages.Remove(Pages.Service);
                }
            });
        }

        protected override object QueryNextPage(object hint)
        {
            VariablesViewModelEx vars = _activator.GetService<VariablesViewModelEx>();
            Pages nextPage = Pages.Unknown;
            switch ((Pages)Page)
            {
                case Pages.PageSelection:
                    nextPage = Pages.InstallLocation;
                    break;

                case Pages.InstallLocation:
                    if (vars.CONFIGURE_SERVICE_ACCOUNT.BooleanString)
                    {
                        nextPage = Pages.Service;
                    }
                    else if (vars.CONFIGURE_SQL.BooleanString)
                    {
                        nextPage = Pages.Database;
                    }
                    else
                    {
                        ApplyViewModel apply = _activator.GetService<ApplyViewModel>();
                        apply.PlanCommand.Execute(LaunchAction.Install); // Plan only, not starting install yet
                        nextPage = Pages.Summary;
                    }
                    break;

                case Pages.Service:
                    if (vars.CONFIGURE_SQL.BooleanString)
                    {
                        nextPage = Pages.Database;
                    }
                    else
                    {
                        ApplyViewModel apply = _activator.GetService<ApplyViewModel>();
                        apply.PlanCommand.Execute(LaunchAction.Install); // Plan only, not starting install yet
                        nextPage = Pages.Summary;
                    }
                    break;

                case Pages.Database:
                    {
                        ApplyViewModel apply = _activator.GetService<ApplyViewModel>();
                        apply.PlanCommand.Execute(LaunchAction.Install); // Plan only, not starting install yet
                        nextPage = Pages.Summary;
                    }
                    break;

                default:
                    throw new InvalidProgramException("Page");
            }

            return nextPage;
        }
    }
}
