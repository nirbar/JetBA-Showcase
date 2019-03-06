using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using PanelSW.Installer.JetBA;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
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
            Bind<View.DatabaseView>().ToSelf().InSingletonScope();
            Bind<View.RepairView>().ToSelf().InSingletonScope();
            Bind<View.ProgressView>().ToSelf().InSingletonScope();
            Bind<View.FinishView>().ToSelf().InSingletonScope();
            Bind<View.HelpView>().ToSelf().InSingletonScope();
            Bind<View.SummaryView>().ToSelf().InSingletonScope();
            Bind<Window>().To<View.RootView>().InSingletonScope();

            // DB connection test
            Bind<IDbConnection, DbConnection, SqlConnection>().ToMethod(
                          (a) =>
                          {
                              string server = ba_.Engine.StringVariables["SQL_SERVER"];
                              string db = ba_.Engine.StringVariables["SQL_DATABASE"];
                              string user = ba_.Engine.StringVariables["SQL_USER"];
                              string psw = ba_.Engine.StringVariables["SQL_PASSWORD"];
                              bool isSqlAuth = (ba_.Engine.NumericVariables["SQL_AUTH"] == 1L);

                              SqlConnectionStringBuilder connStr = new SqlConnectionStringBuilder()
                              {
                                  DataSource = server,
                                  InitialCatalog = db,
                                  UserID = user,
                                  Password = psw,
                                  IntegratedSecurity = !isSqlAuth
                              };

                              return new SqlConnection(connStr.ToString());
                          });
        }
    }
}
