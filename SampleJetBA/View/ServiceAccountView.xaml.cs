using PanelSW.Installer.JetBA.ViewModel;
using System.Windows.Controls;

namespace SampleJetBA.View
{
    public partial class ServiceAccountView : UserControl
    {
        public ServiceAccountView(VariablesViewModel vars, NavigationViewModel nav)
        {
            NavigationViewModel = nav;
            VariablesViewModel = vars;

            DataContext = this;
            InitializeComponent();
        }

        public VariablesViewModel VariablesViewModel { get; private set; }
        public NavigationViewModel NavigationViewModel { get; private set; }
    }
}
