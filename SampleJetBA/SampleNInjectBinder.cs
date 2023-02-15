using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using PanelSW.Installer.JetBA;
using System.Windows;

namespace SampleJetBA
{
    class SampleNInjectBinder : NInjectBinder
    {
        public SampleNInjectBinder(SampleBA ba)
            : base(ba)
        {
        }

        public override void Load()
        {
            base.Load();

            Rebind<BootstrapperApplication, JetBootstrapperApplication, SampleBA>().ToConstant(ba_ as SampleBA);

            // ViewModel
            Rebind<PanelSW.Installer.JetBA.ViewModel.NavigationViewModel, ViewModel.NavigationViewModelEx>().To<ViewModel.NavigationViewModelEx>().InSingletonScope();
            Rebind<PanelSW.Installer.JetBA.ViewModel.InputValidationsViewModel, ViewModel.InputValidationsViewModelEx>().To<ViewModel.InputValidationsViewModelEx>().InSingletonScope();
            Rebind<PanelSW.Installer.JetBA.ViewModel.VariablesViewModel, VariablesViewModelEx>().To<VariablesViewModelEx>().InSingletonScope();
            Rebind<PanelSW.Installer.JetBA.ViewModel.FinishViewModel, PanelSW.Installer.JetBA.JetPack.ViewModel.FinishViewModelEx>().To<PanelSW.Installer.JetBA.JetPack.ViewModel.FinishViewModelEx>().InSingletonScope();
            Bind<PanelSW.Installer.JetBA.JetPack.ViewModel.SqlViewModel>().ToSelf().InSingletonScope();
            Bind<PanelSW.Installer.JetBA.JetPack.ViewModel.PackagesViewModel>().ToSelf().InSingletonScope();
            Rebind<PanelSW.Installer.JetBA.Localization.Resources, SampleJetBA.Localization.Resources>().To<SampleJetBA.Localization.Resources>().InSingletonScope();

            // View
            Bind<View.PageSelectionView>().ToSelf();
            Bind<View.InstallLocationView>().ToSelf();
            Bind<View.DatabaseView>().ToSelf();
            Bind<View.ServiceAccountView>().ToSelf();
            Bind<View.RepairView>().ToSelf();
            Bind<View.ProgressView>().ToSelf();
            Bind<View.FinishView>().ToSelf();
            Bind<View.HelpView>().ToSelf();
            Bind<View.SummaryView>().ToSelf();
            Bind<Window>().To<View.RootView>().InSingletonScope();
        }
    }
}
