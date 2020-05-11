using PanelSW.Installer.JetBA.Util;
using PanelSW.Installer.JetBA.ViewModel;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace SampleJetBA.View
{
    public partial class RootView : Window
    {
        #region P/Invoke

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public Int32 cbSize;
            public IntPtr hwnd;
            public FlashWindow dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        [Flags]
        public enum FlashWindow : uint
        {
            // Stop flashing. The system restores the window to its original state.
            FLASHW_STOP = 0,

            // Flash the window caption
            FLASHW_CAPTION = 1,

            // Flash the taskbar button.
            FLASHW_TRAY = 2,

            // Flash both the window caption and taskbar button.
            FLASHW_ALL = 3,

            // Flash continuously, until the FLASHW_STOP flag is set.
            FLASHW_TIMER = 4,

            // Flash continuously until the window comes to the foreground.
            FLASHW_TIMERNOFG = 12
        }

        #endregion

        public RootView(ProgressViewModel prog, PopupViewModel popup, JetBundleVariables.BundleVariablesViewModel vars, NavigationViewModel nav, UtilViewModel util)
        {
            ProgressViewModel = prog;
            PopupViewModel = popup;
            VariablesViewModel = vars;
            NavigationViewModel = nav;
            UtilViewModel = util;

            DataContext = this;
            Closing += RootView_Closing;
            InitializeComponent();
        }

        // Support closing from NavigationViewModel only.
        private void RootView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            NavigationViewModel.StopCommand.Execute(this);
        }

        public ProgressViewModel ProgressViewModel { get; private set; }
        public PopupViewModel PopupViewModel { get; private set; }
        public JetBundleVariables.BundleVariablesViewModel VariablesViewModel { get; private set; }
        public NavigationViewModel NavigationViewModel { get; private set; }
        public UtilViewModel UtilViewModel { get; private set; }

        private void Background_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void popup_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FLASHWINFO flash = new FLASHWINFO();
            flash.cbSize = Marshal.SizeOf(flash.GetType());
            flash.hwnd = new WindowInteropHelper(this).EnsureHandle();
            flash.uCount = 0;
            flash.dwTimeout = 0;
            if (e.NewValue.Equals(true))
            {
                flash.dwFlags = FlashWindow.FLASHW_ALL | FlashWindow.FLASHW_TIMER;
                FlashWindowEx(ref flash);
            }
            else
            {
                flash.dwFlags = FlashWindow.FLASHW_STOP;
                FlashWindowEx(ref flash);
            }
        }
    }
}
