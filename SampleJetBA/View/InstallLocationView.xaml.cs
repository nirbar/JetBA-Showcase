using System.Windows.Controls;
using PanelSW.Installer.JetBA.ViewModel;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace SampleJetBA.View
{
    public partial class InstallLocationView : UserControl
    {
        public InstallLocationView(JetBundleVariables.BundleVariablesViewModel vars, NavigationViewModel nav, ApplyViewModel apply, PopupViewModel popup)
        {
            NavigationViewModel = nav;
            VariablesViewModel = vars;
	        ApplyViewModel = apply;
            PopupViewModel = popup;

            DataContext = this;
            InitializeComponent();
        }

        public JetBundleVariables.BundleVariablesViewModel VariablesViewModel { get; private set; }
        public NavigationViewModel NavigationViewModel { get; private set; }
        public ApplyViewModel ApplyViewModel { get; private set; }
        public PopupViewModel PopupViewModel { get; private set; }

		private void browseInstallFolder_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            if (!VariablesViewModel.INSTALL_FOLDER.IsNullOrEmpty)
            {
                fbd.SelectedPath = VariablesViewModel.INSTALL_FOLDER.String;
            }

            if (fbd.ShowDialog() == WinForms.DialogResult.OK)
            {
                VariablesViewModel.INSTALL_FOLDER.String = fbd.SelectedPath;
            }
        }
    }
}
