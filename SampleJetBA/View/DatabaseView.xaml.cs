using PanelSW.Installer.JetBA.JetPack.ViewModel;
using PanelSW.Installer.JetBA.JetPack.Util;
using PanelSW.Installer.JetBA.ViewModel;
using System.Windows.Controls;

namespace SampleJetBA.View
{
    public partial class DatabaseView : UserControl
    {
        public DatabaseView(VariablesViewModel vars, NavigationViewModel nav, SqlViewModel db)
        {
            NavigationViewModel = nav;
            VariablesViewModel = vars;
            SqlViewModel = db;

            DataContext = this;
            InitializeComponent();
        }

        public VariablesViewModel VariablesViewModel { get; private set; }
        public NavigationViewModel NavigationViewModel { get; private set; }
        public SqlViewModel SqlViewModel { get; private set; }
    }
}
