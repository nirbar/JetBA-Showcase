using PanelSW.Installer.JetBA.JetPack;
using PanelSW.Installer.JetBA.ViewModel;
using WixToolset.Mba.Core;

namespace SampleJetBA.ViewModel
{
    public partial class VariablesViewModelEx : VariablesViewModel
    {
        public VariablesViewModelEx(SampleBA ba, IEngine engine, JetPackActivator activator)
            : base(ba, engine, activator)
        {
        }
    }
}
