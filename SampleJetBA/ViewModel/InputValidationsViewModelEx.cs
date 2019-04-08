using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Ninject;
using PanelSW.Installer.JetBA.ViewModel;
using System;
using System.Data.SqlClient;
using System.IO;

namespace SampleJetBA.ViewModel
{
    public class InputValidationsViewModelEx : InputValidationsViewModel
    {
        public InputValidationsViewModelEx(SampleBA ba)
            : base(ba)
        {
        }

        public override void ValidatePage(object pageId)
        {
            Pages page = (Pages)pageId;

            switch (page)
            {
                case Pages.InstallLocation:
                    ValidateTargetFolder();
                    break;

                case Pages.Database:
                    ValidateDatabase();
                    break;

                default:
                    break;
            }
        }

        public override void ValidateAll()
        {
            ValidateTargetFolder();
            ValidateDatabase();
        }

        private void ValidateTargetFolder()
        {
            VariablesViewModel vars = BA.Kernel.Get<VariablesViewModel>();
            string installFolder = vars["InstallFolder"].String;

            if (string.IsNullOrWhiteSpace(installFolder) || (installFolder.IndexOfAny(Path.GetInvalidPathChars()) >= 0))
            {
                AddResult(new Exception(string.Format(Properties.Resources._0IsNotALegalFolderName, installFolder)));
            }
        }

        private void ValidateDatabase()
        {
            try
            {
                VariablesViewModel vars = BA.Kernel.Get<VariablesViewModel>();
                string server = vars["SQL_SERVER"].String;
                string db = vars["SQL_DATABASE"].String;
                string user = vars["SQL_USER"].String;
                string psw = vars["SQL_PASSWORD"].String;
                bool isSqlAuth = vars["SQL_AUTH"].Boolean;

                SqlConnectionStringBuilder connStr = new SqlConnectionStringBuilder()
                {
                    DataSource = server,
                    InitialCatalog = db,
                    UserID = user,
                    Password = psw,
                    IntegratedSecurity = !isSqlAuth
                };

                using (SqlConnection conn = new SqlConnection(connStr.ToString()))
                {
                    conn.Open();
                }
            }
            catch (Exception ex)
            {
                BA.Engine.Log(LogLevel.Error, $"Failed connecting to DB server: {ex.Message}");
                AddResult(new Exception(string.Format(Properties.Resources.FailedConnectingToDbServer0, ex.Message)));
            }
        }
    }
}