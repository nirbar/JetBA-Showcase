using PanelSW.Installer.JetBA.ViewModel;
using System.Windows.Controls;

namespace SampleJetBA.View
{
    public partial class FinishView : UserControl
    {
        public FinishView(ApplyViewModel apply, VariablesViewModel vars, FinishViewModel finish, NavigationViewModel nav, UtilViewModel util)
        {
            FinishViewModel = finish;
            VariablesViewModel = vars;
            ApplyViewModel = apply;
            NavigationViewModel = nav;
            UtilViewModel = util;
            DataContext = this;
            InitializeComponent();
        }

        public ApplyViewModel ApplyViewModel { get; private set; }
        public VariablesViewModel VariablesViewModel { get; private set; }
        public FinishViewModel FinishViewModel { get; private set; }
        public NavigationViewModel NavigationViewModel { get; private set; }
        public UtilViewModel UtilViewModel { get; private set; }
    }
}
