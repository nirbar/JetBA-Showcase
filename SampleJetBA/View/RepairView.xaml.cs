using PanelSW.Installer.JetBA.ViewModel;
using System.Windows.Controls;

namespace SampleJetBA.View
{
    public partial class RepairView : UserControl
    {
        public RepairView(ApplyViewModel apply, VariablesViewModel vars)
        {
            DataContext = this;
            ApplyViewModel = apply;
            VariablesViewModel = vars;
            InitializeComponent();
        }

        public ApplyViewModel ApplyViewModel { get; private set; }
        public VariablesViewModel VariablesViewModel { get; private set; }
    }
}
