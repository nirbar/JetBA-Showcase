using WixToolset.Mba.Core;

[assembly: BootstrapperApplicationFactory(typeof(SampleJetBA.SampleJetBaFactory))]

namespace SampleJetBA
{
    public class SampleJetBaFactory : BaseBootstrapperApplicationFactory
    {
        private SampleBA _ba;
        protected override IBootstrapperApplication Create(IEngine engine, IBootstrapperCommand bootstrapperCommand)
        {
            System.Diagnostics.Debugger.Launch();
            if (_ba == null)
            {
                _ba = new SampleBA(engine, bootstrapperCommand);
            }
            return _ba;
        }
    }
}
