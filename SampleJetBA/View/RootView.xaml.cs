using PanelSW.Installer.JetBA.Util;
using PanelSW.Installer.JetBA.ViewModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace SampleJetBA.View
{
    public partial class RootView : Window
    {
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

        private void ApplicationCommandsOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is FolderBrowserDialog fbd)
            {
                using (fbd)
                {
                    string varName = fbd.Tag as string;
                    if (!string.IsNullOrEmpty(varName))
                    {
                        fbd.Description = VariablesViewModel.WixBundleName.String;
                        fbd.SelectedPath = VariablesViewModel[varName].String;
                        if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            VariablesViewModel[varName].String = fbd.SelectedPath;
                        }
                    }
                }
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}
