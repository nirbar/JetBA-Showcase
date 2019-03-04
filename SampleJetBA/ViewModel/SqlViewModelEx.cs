using PanelSW.Installer.JetBA.JetPack.ViewModel;
using PanelSW.Installer.JetBA.ViewModel;
using System;
using System.Data;

namespace SampleJetBA.ViewModel
{
    public class SqlViewModelEx : SqlViewModel
    {
        public SqlViewModelEx(SampleBA ba, VariablesViewModel vars, Lazy<IDbConnection> conn)
            : base(ba, conn)
        {
            vars["SQL_SERVER"].PropertyChanged += SqlViewModelEx_PropertyChanged;
            vars["SQL_DATABASE"].PropertyChanged += SqlViewModelEx_PropertyChanged;
            vars["SQL_USER"].PropertyChanged += SqlViewModelEx_PropertyChanged;
            vars["SQL_PASSWORD"].PropertyChanged += SqlViewModelEx_PropertyChanged;
            vars["SQL_AUTH"].PropertyChanged += SqlViewModelEx_PropertyChanged;
        }

        private void SqlViewModelEx_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("String"))
            {
                ResetTestStatus();
            }
        }
    }
}