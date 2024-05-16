using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WixToolset.BootstrapperApplicationApi;

namespace SampleJetBA
{
    internal class Program
    {
        public static int Main(string[] args)
        {
#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif

            var application = new SampleBA();
            ManagedBootstrapperApplication.Run(application);
            return 0;
        }
    }
}
