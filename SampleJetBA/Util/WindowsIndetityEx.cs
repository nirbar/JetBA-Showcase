using System;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;

namespace SampleJetBA.Util
{
    public static class WindowsIndetityEx
    {
        #region P/Invoke

        private enum LOGON32_LOGON
        {
            LOGON32_LOGON_INTERACTIVE = 2,
            LOGON32_LOGON_NETWORK = 3,
            LOGON32_LOGON_BATCH = 4,
            LOGON32_LOGON_SERVICE = 5,
            LOGON32_LOGON_UNLOCK = 7,
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,
            LOGON32_LOGON_NEW_CREDENTIALS = 9
        }

        private enum LOGON32_PROVIDER
        {
            LOGON32_PROVIDER_DEFAULT = 0,
            LOGON32_PROVIDER_WINNT35 = 1,
            LOGON32_PROVIDER_WINNT40 = 2,
            LOGON32_PROVIDER_WINNT50 = 3,
            LOGON32_PROVIDER_VIRTUAL = 4
        }

        [DllImport("advapi32.dll", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true, EntryPoint = "LogonUser")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool LogonUserCore([MarshalAs(UnmanagedType.LPStr)] string pszUserName, [MarshalAs(UnmanagedType.LPStr)] string pszDomain, [MarshalAs(UnmanagedType.LPStr)] string pszPassword, [MarshalAs(UnmanagedType.U4)] LOGON32_LOGON dwLogonType, [MarshalAs(UnmanagedType.U4)]LOGON32_PROVIDER dwLogonProvider, ref IntPtr phToken);

        #endregion        

        public enum RequestedLevel
        {
            // Impersonate when making network connections.
            // This level is sufficient for connecting to SQL Server and otehr network resources
            NetworkOnly,

            // Changes the local identity to the impersonated user
            LocalAndNetwork
        }

        public static WindowsImpersonationContextEx Impersonate(string userName, SecureString password, RequestedLevel level = RequestedLevel.NetworkOnly)
        {
            LOGON32_LOGON logon = LOGON32_LOGON.LOGON32_LOGON_NEW_CREDENTIALS;
            LOGON32_PROVIDER provider = LOGON32_PROVIDER.LOGON32_PROVIDER_WINNT50;
            if (level == RequestedLevel.LocalAndNetwork)
            {
                logon = LOGON32_LOGON.LOGON32_LOGON_INTERACTIVE;
                provider = LOGON32_PROVIDER.LOGON32_PROVIDER_DEFAULT;
            }

            string name = userName;
            string domain = ".";
            int i = userName.IndexOf('\\');
            if (i >= 0)
            {
                domain = userName.Substring(0, i);
                name = userName.Substring(i + 1);
            }
            else
            {
                i = userName.IndexOf('@');
                if (i >= 0)
                {
                    name = userName.Substring(0, i);
                    domain = userName.Substring(i + 1);
                }
            }

            IntPtr psw = IntPtr.Zero;
            try
            {
                psw = Marshal.SecureStringToBSTR(password);
                IntPtr token = IntPtr.Zero;
                if (!LogonUserCore(name, domain, Marshal.PtrToStringBSTR(psw), logon, provider, ref token) || (token == IntPtr.Zero))
                {
                    //TODO P/Invoke FormatMessage to get localized error text
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                WindowsImpersonationContext context = WindowsIdentity.Impersonate(token);
                return new WindowsImpersonationContextEx(context, token);
            }
            finally
            {
                if (psw != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(psw);
                }
            }
        }
    }

    public class WindowsImpersonationContextEx : IDisposable
    {
        private IntPtr token_ = IntPtr.Zero;
        private WindowsImpersonationContext context_ = null;

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        public WindowsImpersonationContextEx(WindowsImpersonationContext context, IntPtr token)
        {
            token_ = token;
            context_ = context;
        }

        public void Dispose()
        {
            context_?.Dispose();
            if (token_ != IntPtr.Zero)
            {
                if (!CloseHandle(token_))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed releasing impersonation token");
                }
            }
        }
    }
}