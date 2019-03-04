using System.Windows.Controls;

namespace SampleJetBA.View
{
    public partial class HelpView : UserControl
    {
        public HelpView(ViewModel.NavigationViewModelEx nav)
        {
            DataContext = this;
            NavigationViewModel = nav;
            InitializeComponent();
        }

        public ViewModel.NavigationViewModelEx NavigationViewModel { get; private set; }
    }
}
