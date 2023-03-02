using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using PanelSW.Installer.JetBA.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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

        private readonly Engine eng_;

        public RootView(Engine eng, ProgressViewModel prog, PopupViewModel popup, JetBundleVariables.BundleVariablesViewModel vars, ViewModel.NavigationViewModelEx nav, UtilViewModel util)
        {
            eng_ = eng;
            ProgressViewModel = prog;
            PopupViewModel = popup;
            VariablesViewModel = vars;
            NavigationViewModel = nav;
            UtilViewModel = util;

            DataContext = this;
            Closing += RootView_Closing;
            InitializeComponent();
            LoadCultures();
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
        public ViewModel.NavigationViewModelEx NavigationViewModel { get; private set; }
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

        private void LoadCultures()
        {
            ComboBoxItem bestMatchItem = null;

            string myPath = Assembly.GetExecutingAssembly().Location;
            string myDir = Path.GetDirectoryName(myPath);
            string myName = Path.GetFileNameWithoutExtension(myPath);
            List<string> cultureFiles = new List<string>(Directory.GetFiles(myDir, $"{myName}.resources.dll", SearchOption.AllDirectories));
            foreach (string resFile in cultureFiles)
            {
                string cultureName = Path.GetDirectoryName(resFile);
                cultureName = Path.GetFileName(cultureName);
                CultureInfo culture = new CultureInfo(cultureName);

                ComboBoxItem langItem = new ComboBoxItem();
                langItem.Tag = culture;
                langItem.Content = culture.NativeName;
                langItem.FlowDirection = culture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                cultureCombo_.Items.Add(langItem);

                if (Thread.CurrentThread.CurrentUICulture.Equals(culture))
                {
                    bestMatchItem = langItem;
                }
                else if ((bestMatchItem == null) && ((Thread.CurrentThread.CurrentUICulture.Parent?.Equals(culture) == true) || (Thread.CurrentThread.CurrentUICulture.Parent?.Equals(culture?.Parent) == true)))
                {
                    bestMatchItem = langItem;
                }
            }

            if (bestMatchItem != null)
            {
                cultureCombo_.SelectedItem = bestMatchItem;
            }
        }

        private void culture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((cultureCombo_.SelectedItem is ComboBoxItem clt) && (clt.Tag is CultureInfo newClt))
            {
                eng_.Log(LogLevel.Standard, $"Changing UI locale to '{newClt.Name}': {newClt.NativeName}");
                Thread.CurrentThread.CurrentCulture = newClt;
                Thread.CurrentThread.CurrentUICulture = newClt;
                Properties.Resources.Culture = newClt;
                PanelSW.Installer.JetBA.Properties.Resources.Culture = newClt;
                NavigationViewModel.Refresh();
                PopupViewModel.Refresh();
            }
        }

        private void showLicenses_Click(object sender, RoutedEventArgs e)
        {
            string openSrcLicPath = Assembly.GetExecutingAssembly().Location;
            openSrcLicPath = Path.GetDirectoryName(openSrcLicPath);
            openSrcLicPath = Path.Combine(openSrcLicPath, "LICENSE.md");
            if (!File.Exists(openSrcLicPath))
            {
                return;
            }

            string text = File.ReadAllText(openSrcLicPath);
            PopupViewModel.Show(0, PopupViewModel.UIHintFlags.MB_ICONINFORMATION | PopupViewModel.UIHintFlags.MB_OK | PopupViewModel.UIHintFlags.MB_DEFBUTTON1, text);
        }
    }
}
