using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using PanelSW.Installer.JetBA;
using PanelSW.Installer.JetBA.Util;

namespace SampleJetBA
{
    public class SampleBA : JetBootstrapperApplication
    {
        private SampleNInjectBinder binder_ = null;
        protected override NInjectBinder Binder => binder_ ?? (binder_ = new SampleNInjectBinder(this));

        protected override void OnResolveCulture(ResolveCultureEventArgs args)
        {
            base.OnResolveCulture(args);
            Properties.Resources.Culture = args.CultureInfo;
            PanelSW.Installer.JetBA.JetPack.Properties.Resources.Culture = args.CultureInfo;
        }

        protected override void Run()
        {
            if (Engine.StringVariables.Contains("BaLaunchDebugger") && Engine.BooleanVariable("BaLaunchDebugger"))
            {
                System.Diagnostics.Debugger.Launch();
            }
            base.Run();
        }
    }
}