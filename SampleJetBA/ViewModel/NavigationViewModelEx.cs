﻿using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Ninject;
using PanelSW.Installer.JetBA;
using PanelSW.Installer.JetBA.ViewModel;
using SampleJetBA.View;
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;

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

    public class NavigationViewModelEx : NavigationViewModel, IInitializable
    {
        public NavigationViewModelEx(SampleBA ba
            , Lazy<DetectingView> detectingView
            , Lazy<PageSelectionView> pageSelectionView
            , Lazy<InstallLocationView> installLocationView
            , Lazy<ServiceAccountView> svcView
            , Lazy<DatabaseView> dbView
            , Lazy<RepairView> repairView
            , Lazy<ProgressView> progressView
            , Lazy<HelpView> helpView
            , Lazy<FinishView> finishView
            , Lazy<SummaryView> summaryView
            )
            : base(ba)
        {
            BA.DetectComplete += BA_DetectComplete;
            BA.ApplyBegin += BA_ApplyBegin;
            BA.ApplyComplete += BA_ApplyComplete;
            BA.Kernel.Get<Dispatcher>().Invoke(() => ExpectedPages = new ObservableCollection<Pages>());

            AddPage(Pages.Finish, new Lazy<object>(() => finishView.Value));
            AddPage(Pages.Help, new Lazy<object>(() => helpView.Value));
            AddPage(Pages.Detecting, new Lazy<object>(() => detectingView.Value));
            AddPage(Pages.PageSelection, new Lazy<object>(() => pageSelectionView.Value));
            AddPage(Pages.InstallLocation, new Lazy<object>(() => installLocationView.Value));
            AddPage(Pages.Service, new Lazy<object>(() => svcView.Value));
            AddPage(Pages.Database, new Lazy<object>(() => dbView.Value));
            AddPage(Pages.Progress, new Lazy<object>(() => progressView.Value));
            AddPage(Pages.Repair, new Lazy<object>(() => repairView.Value));
            AddPage(Pages.Summary, new Lazy<object>(() => summaryView.Value));

            SetStartPage();
        }

        void IInitializable.Initialize()
        {
            ApplyViewModel apply = BA.Kernel.Get<ApplyViewModel>();
            apply.PropertyChanged += apply_PropertyChanged;

            JetBundleVariables.BundleVariablesViewModel vars = BA.Kernel.Get<JetBundleVariables.BundleVariablesViewModel>();
            vars.CONFIGURE_SQL.PropertyChanged += CONFIGURE_SQL_PropertyChanged;
            vars.CONFIGURE_SERVICE_ACCOUNT.PropertyChanged += CONFIGURE_SERVICE_ACCOUNT_PropertyChanged;
        }

        #region Event-based navigations

        public void SetStartPage()
        {
            if (BA.Kernel.Get<Display>() < Display.Passive)
            {
                return;
            }

            if (BA.Command.Action == LaunchAction.Help)
            {
                Page = Pages.Help;
                return;
            }

            ApplyViewModel apply = BA.Kernel.Get<ApplyViewModel>();
            if (apply.InstallState < InstallationState.Detected)
            {
                Page = Pages.Detecting;
                return;
            }

            switch (apply.DetectState)
            {
                case DetectionState.Absent:
                case DetectionState.Newer:
                case DetectionState.SameVersion:
                case DetectionState.Older:
                    BA.Kernel.Get<Dispatcher>().Invoke(() =>
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
                    break;

                case DetectionState.Present:
                    BA.Kernel.Get<Dispatcher>().Invoke(() =>
                    {
                        ExpectedPages.Add(Pages.Repair);
                        ExpectedPages.Add(Pages.Progress);
                        ExpectedPages.Add(Pages.Finish);
                    });
                    Page = Pages.Repair;
                    break;

                default:
                    BA.Engine.Log(LogLevel.Error, $"Unhandled detect state '{apply.DetectState}'");
                    break;
            }

            JetBundleVariables.BundleVariablesViewModel vars = BA.Kernel.Get<JetBundleVariables.BundleVariablesViewModel>();
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
            PanelSW.Installer.JetBA.ViewModel.ApplyViewModel apply = BA.Kernel.Get<PanelSW.Installer.JetBA.ViewModel.ApplyViewModel>();
            if (e.PropertyName.Equals("InstallState") && (apply.InstallState >= InstallationState.Applied))
            {
                ClearHistory();
                Page = Pages.Finish;
            }
        }

        #endregion

        public ObservableCollection<Pages> ExpectedPages { get; private set; }

        private void CONFIGURE_SQL_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals("BooleanString") || (BA.Kernel.Get<Display>() < Display.Passive))
            {
                return;
            }

            JetBundleVariables.BundleVariablesViewModel vars = BA.Kernel.Get<JetBundleVariables.BundleVariablesViewModel>();
            BA.Kernel.Get<Dispatcher>().Invoke(() =>
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
            if (!e.PropertyName.Equals("BooleanString") || (BA.Kernel.Get<Display>() < Display.Passive))
            {
                return;
            }

            JetBundleVariables.BundleVariablesViewModel vars = BA.Kernel.Get<JetBundleVariables.BundleVariablesViewModel>();
            BA.Kernel.Get<Dispatcher>().Invoke(() =>
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
            JetBundleVariables.BundleVariablesViewModel vars = BA.Kernel.Get<JetBundleVariables.BundleVariablesViewModel>();
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
                        ApplyViewModel apply = BA.Kernel.Get<ApplyViewModel>();
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
                        ApplyViewModel apply = BA.Kernel.Get<ApplyViewModel>();
                        apply.PlanCommand.Execute(LaunchAction.Install); // Plan only, not starting install yet
                        nextPage = Pages.Summary;
                    }
                    break;

                case Pages.Database:
                    {
                        ApplyViewModel apply = BA.Kernel.Get<ApplyViewModel>();
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