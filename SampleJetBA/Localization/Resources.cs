﻿using System.Globalization;
using System.Resources;

namespace SampleJetBA.Localization
{
    public class Resources : PanelSW.Installer.JetBA.Localization.Resources
    {
        public Resources(SampleBA ba)
            : base(ba)
        {
        }

        public override string Resolve(string key, ResourceManager resourceManager = null, CultureInfo culture = null, params object[] args)
        {
            // Prefer using my translations over JetBA's
            string val = base.Resolve(key, resourceManager ?? Properties.Resources.ResourceManager, culture ?? Properties.Resources.Culture, args);
            if ((string.IsNullOrEmpty(val) || val.Equals(key)) && ((resourceManager == null) || (culture == null)))
            {
                val = base.Resolve(key, resourceManager, culture, args);
            }

            return val;
        }
    }
}