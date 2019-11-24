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
            if (values.Length < 2)
            {
                return false;
            }

            object val0 = values[0];
            for (int i = 1; i < values.Length; ++i)
            {
                object val1 = values[i];
                if (val0 == null)
                {
                    if (val1 == null)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (!val0.Equals(val1))
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