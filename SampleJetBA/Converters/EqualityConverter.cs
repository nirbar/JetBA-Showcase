using PanelSW.Installer.JetBA.Util;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SampleJetBA.Converters
{
    public class EqualityConverter : BaseConverter, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values == null) || (values.Length == 0))
            {
                return null; // Let FallbackValue decide
            }

            object val0 = values[0];
            for (int i = 1; i < values.Length; ++i)
            {
                object val1 = values[i];

                // If both are null, we'll continue; If just one is null, we'll return false; If neither is null, then actual comparison will be made.
                if ((val0?.Equals(val1) ?? val1?.Equals(val0)) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}