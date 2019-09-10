using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Ninject;
using PanelSW.Installer.JetBA.ViewModel;
using System;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

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

                case Pages.Service:
                    ValidateServiceAccount();
                    break;

                default:
                    break;
            }
        }

        public override void ValidateAll()
        {
            ValidateTargetFolder();
            ValidateDatabase();
            ValidateServiceAccount();
        }

        private void ValidateTargetFolder()
        {
            VariablesViewModel vars = BA.Kernel.Get<VariablesViewModel>();
            string installFolder = vars["InstallFolder"];

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
                SqlConnectionStringBuilder connStr = new SqlConnectionStringBuilder()
                {
                    DataSource = vars["SQL_SERVER"],
                    InitialCatalog = vars["SQL_DATABASE"],
                    IntegratedSecurity = !vars["SQL_AUTH"]
                };

                SqlCredential sqlCredential = null;
                if (!connStr.IntegratedSecurity)
                {
                    SecureString psw = vars["SQL_PASSWORD"];
                    psw.MakeReadOnly();
                    sqlCredential = new SqlCredential(vars["SQL_USER"], psw);
                }

                using (SqlConnection conn = new SqlConnection(connStr.ToString(), sqlCredential))
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

        private void ValidateServiceAccount()
        {
            VariablesViewModel vars = BA.Kernel.Get<VariablesViewModel>();
            if (vars["SERVICE_USER"].IsNullOrEmpty)
            {
                // Default - .\LocalSystem account
                vars["SERVICE_PASSWORD"].SecureString = new SecureString();
                return;
            }

            ValidateCredentials(vars["SERVICE_USER"], vars["SERVICE_PASSWORD"]);
        }

        public static void ValidateCredentials(string username, SecureString password)
        {
            // Parse domain\user and user@domain formats.
            string name = username;
            string domain = ".";
            int i = username.IndexOf('\\');
            if (i >= 0)
            {
                domain = username.Substring(0, i);
                name = username.Substring(i + 1);
            }
            else
            {
                i = username.IndexOf('@');
                if (i >= 0)
                {
                    name = username.Substring(0, i);
                    domain = username.Substring(i + 1);
                }
            }

            ContextType ctx = ContextType.Domain;
            if (string.IsNullOrWhiteSpace(domain) || domain.Equals(".") || domain.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase))
            {
                ctx = ContextType.Machine;
                domain = Environment.MachineName;
            }
            using (PrincipalContext pc = new PrincipalContext(ctx, domain))
            {
                IntPtr pPsw = IntPtr.Zero;
                try
                {
                    pPsw = Marshal.SecureStringToGlobalAllocUnicode(password);

                    // Validate credentials with minimal keep of managed plain password.
                    if (!pc.ValidateCredentials(name, Marshal.PtrToStringUni(pPsw)))
                    {
                        throw new Exception(Properties.Resources.InvalidCredentials);
                    }
                }
                finally
                {
                    if (pPsw != IntPtr.Zero)
                    {
                        Marshal.ZeroFreeGlobalAllocUnicode(pPsw);
                    }
                }
            }
        }
    }
}