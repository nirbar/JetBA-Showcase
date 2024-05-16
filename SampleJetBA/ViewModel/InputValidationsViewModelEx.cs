using PanelSW.Installer.JetBA.JetPack;
using PanelSW.Installer.JetBA.ViewModel;
using SampleJetBA.Util;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using WixToolset.BootstrapperApplicationApi;

namespace SampleJetBA.ViewModel
{
    public class InputValidationsViewModelEx : InputValidationsViewModel
    {
        public InputValidationsViewModelEx(SampleBA ba, IEngine engine, IBootstrapperCommand command, JetPackActivator activator)
            : base(ba, engine, command, activator)
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

                case Pages.Service:
                    ValidateServiceAccount();
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

            VariablesViewModelEx vars = _activator.GetService<VariablesViewModelEx>();
            if (vars.CONFIGURE_SERVICE_ACCOUNT.BooleanString)
            {
                ValidateServiceAccount();
            }
            if (vars.CONFIGURE_SQL.BooleanString)
            {
                ValidateDatabase();
            }
        }

        private void ValidateTargetFolder()
        {
            VariablesViewModelEx vars = _activator.GetService<VariablesViewModelEx>();
            if (vars.INSTALL_FOLDER.IsNullOrEmpty || (vars.INSTALL_FOLDER.String.IndexOfAny(Path.GetInvalidPathChars()) >= 0) || !Path.IsPathRooted(vars.INSTALL_FOLDER.String))
            {
                AddResult(new Exception(string.Format(Properties.Resources._0IsNotALegalFolderName, vars.INSTALL_FOLDER.String)));
            }
        }

        private void ValidateDatabase()
        {
            try
            {
                VariablesViewModelEx vars = _activator.GetService<VariablesViewModelEx>();
                SqlConnectionStringBuilder connStr = new SqlConnectionStringBuilder()
                {
                    DataSource = vars.SQL_SERVER.String,
                    InitialCatalog = vars.SQL_DATABASE.String,
                    IntegratedSecurity = !vars.SQL_AUTH.Boolean
                };

                _engine.Log(LogLevel.Verbose, $"Testing SQL connection string '{connStr}'");
                if (connStr.IntegratedSecurity)
                {
                    vars.SQL_USER.String = "";
                    vars.SQL_PASSWORD.SecureString = new SecureString();

                    if (vars.CONFIGURE_SERVICE_ACCOUNT.BooleanString && !vars.SERVICE_USER.IsNullOrEmpty)
                    {
                        _engine.Log(LogLevel.Standard, $"Impersonating '{vars.SERVICE_USER.String}' to check Windows authentication to SQL server");
                        using (WindowsImpersonationContextEx ctx = WindowsIndetityEx.Impersonate(vars.SERVICE_USER.String, vars.SERVICE_PASSWORD.SecureString))
                        {
                            WindowsIdentity.RunImpersonated(ctx.Handle, () =>
                            {
                                using (SqlConnection conn = new SqlConnection(connStr.ToString(), null))
                                {
                                    conn.Open();
                                }
                            });
                        }
                    }
                }
                else // SQL Auth
                {
                    if (vars.SQL_USER.IsNullOrEmpty)
                    {
                        AddResult(new Exception(Properties.Resources.PleaseProvideSqlUserName));
                    }

                    SecureString psw = vars.SQL_PASSWORD.SecureString;
                    psw.MakeReadOnly();
                    SqlCredential sqlCredential = new SqlCredential(vars.SQL_USER.String, psw);
                    using (SqlConnection conn = new SqlConnection(connStr.ToString(), sqlCredential))
                    {
                        conn.Open();
                    }
                }
            }
            catch (Exception ex)
            {
                _engine.Log(LogLevel.Error, $"Failed connecting to DB server: {ex.Message}");
                AddResult(new Exception(string.Format(Properties.Resources.FailedConnectingToDbServer0, ex.Message)));
            }
        }

        private void ValidateServiceAccount()
        {
            VariablesViewModelEx vars = _activator.GetService<VariablesViewModelEx>();
            if (vars.SERVICE_USER.IsNullOrEmpty)
            {
                // Default - .\LocalSystem account
                vars.SERVICE_PASSWORD.SecureString = new SecureString();
                return;
            }

            ValidateCredentials(vars.SERVICE_USER.String, vars.SERVICE_PASSWORD.SecureString);
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
                    if (!pc.ValidateCredentials(name, Marshal.PtrToStringUni(pPsw), ContextOptions.Negotiate))
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
