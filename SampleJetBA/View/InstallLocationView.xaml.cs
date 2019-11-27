using System.Windows.Controls;
using PanelSW.Installer.JetBA.ViewModel;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace SampleJetBA.View
{
    public partial class InstallLocationView : UserControl
    {
        public InstallLocationView(VariablesViewModel vars, NavigationViewModel nav, ApplyViewModel apply, PopupViewModel popup)
        {
            NavigationViewModel = nav;
            VariablesViewModel = vars;
	        ApplyViewModel = apply;
            PopupViewModel = popup;

            DataContext = this;
            InitializeComponent();
        }

        public VariablesViewModel VariablesViewModel { get; private set; }
        public NavigationViewModel NavigationViewModel { get; private set; }
        public ApplyViewModel ApplyViewModel { get; private set; }
        public PopupViewModel PopupViewModel { get; private set; }

		private void browseInstallFolder_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog fbd = new WinForms.FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            if (!VariablesViewModel["InstallFolder"].IsNullOrEmpty)
            {
                fbd.SelectedPath = VariablesViewModel["InstallFolder"].String;
            }

            if (fbd.ShowDialog() == WinForms.DialogResult.OK)
            {
                VariablesViewModel["InstallFolder"].String = fbd.SelectedPath;
            }
        }
    }
}
