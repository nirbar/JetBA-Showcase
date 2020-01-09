using PanelSW.Installer.JetBA.Util;
using System;
using System.Windows.Forms;
using System.Windows.Markup;

namespace SampleJetBA.Util
{
    public class BrowseFolderExtension : MarkupExtension
    {
        public string VariableName { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Tag = VariableName;

            return fbd;
        }
    }
}
