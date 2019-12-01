using System;
using System.ComponentModel;
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

        public static WindowsImpersonationContext Impersonate(string userName, SecureString password)
        {
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

            IntPtr psw = Marshal.SecureStringToBSTR(password);
            try
            {
                IntPtr token = IntPtr.Zero;
                if (!LogonUserCore(name, domain, Marshal.PtrToStringUni(psw), LOGON32_LOGON.LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER.LOGON32_PROVIDER_WINNT50, ref token) || (token == IntPtr.Zero))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                WindowsIdentity wi = new WindowsIdentity(token);
                return wi.Impersonate();
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
}