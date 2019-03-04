using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Ninject;
using PanelSW.Installer.JetBA;
using SampleJetBA.View;
using System;
using System.Threading;

namespace SampleJetBA.ViewModel
{
    public enum Pages
    {
        Unknown,
        Detecting,
        InstallLocation,
        Summary,
        Progress,
        Finish,
        Repair,
        Help
    }

    public class NavigationViewModelEx : PanelSW.Installer.JetBA.ViewModel.NavigationViewModel, IInitializable
    {
        public NavigationViewModelEx(SampleBA ba
            , Lazy<DetectingView> detectingView
            , Lazy<InstallLocationView> installLocationView
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

            AddPage(Pages.Finish, new Lazy<object>(() => finishView.Value));
            AddPage(Pages.Help, new Lazy<object>(() => helpView.Value));
            AddPage(Pages.Detecting, new Lazy<object>(() => detectingView.Value));
            AddPage(Pages.InstallLocation, new Lazy<object>(() => installLocationView.Value));
            AddPage(Pages.Progress, new Lazy<object>(() => progressView.Value));
            AddPage(Pages.Repair, new Lazy<object>(() => repairView.Value));
            AddPage(Pages.Summary, new Lazy<object>(() => summaryView.Value));

            SetStartPage();
        }

        void IInitializable.Initialize()
        {
            PanelSW.Installer.JetBA.ViewModel.ApplyViewModel apply = BA.Kernel.Get<PanelSW.Installer.JetBA.ViewModel.ApplyViewModel>();
            apply.PropertyChanged += apply_PropertyChanged;
        }

        #region Event-based navigations

        public void SetStartPage()
        {
            if (BA.Command.Action == LaunchAction.Help)
            {
                Page = Pages.Help;
                return;
            }

            PanelSW.Installer.JetBA.ViewModel.ApplyViewModel apply = BA.Kernel.Get<PanelSW.Installer.JetBA.ViewModel.ApplyViewModel>();
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
                    Page = Pages.InstallLocation;
                    break;

                case DetectionState.Present:
                    Page = Pages.Repair;
                    break;

                default:
                    BA.Engine.Log(LogLevel.Error, $"Unhandled detect state '{apply.DetectState}'");
                    break;
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

        protected override object QueryNextPage(object hint)
        {
            Pages nextPage = Pages.Unknown;
            PanelSW.Installer.JetBA.ViewModel.ApplyViewModel apply = BA.Kernel.Get<PanelSW.Installer.JetBA.ViewModel.ApplyViewModel>();
            switch ((Pages)Page)
            {
                case Pages.InstallLocation:
                    apply.PlanCommand.Execute(LaunchAction.Install); // Plan only, not starting install yet
                    nextPage = Pages.Summary;
                    break;

                default:
                    throw new InvalidProgramException("Page");
            }

            return nextPage;
        }
    }
}
