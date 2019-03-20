using PanelSW.Installer.JetBA.ViewModel;
using System.Windows.Controls;

namespace SampleJetBA.View
{
    public partial class PageSelectionView : UserControl
    {
        public PageSelectionView(NavigationViewModel nav)
        {
            NavigationViewModel = nav;

            DataContext = this;
            InitializeComponent();
        }

        public NavigationViewModel NavigationViewModel { get; private set; }
    }
}
