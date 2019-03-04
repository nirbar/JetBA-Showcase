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

            // View
            Bind<View.InstallLocationView>().ToSelf().InSingletonScope();
            Bind<View.RepairView>().ToSelf().InSingletonScope();
            Bind<View.ProgressView>().ToSelf().InSingletonScope();
            Bind<View.FinishView>().ToSelf().InSingletonScope();
            Bind<View.HelpView>().ToSelf().InSingletonScope();
            Bind<View.SummaryView>().ToSelf().InSingletonScope();
            Bind<Window>().To<View.RootView>().InSingletonScope();
        }
    }
}
