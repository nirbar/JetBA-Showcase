using PanelSW.Installer.JetBA.JetPack.ViewModel;
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

            //TODO: Migrate to 'VariablesViewModel["DB_PASSWORD"].BindPassword'' once released.
            passwordBox_.Password = VariablesViewModel["SQL_PASSWORD"].String;
        }

        public VariablesViewModel VariablesViewModel { get; private set; }
        public NavigationViewModel NavigationViewModel { get; private set; }
        public SqlViewModel SqlViewModel { get; private set; }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            VariablesViewModel["SQL_PASSWORD"].String = ((PasswordBox)sender).Password;
        }
    }
}
