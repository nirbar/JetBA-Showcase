using PanelSW.Installer.JetBA.Util;
using SampleJetBA.ViewModel;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SampleJetBA.Converters
{
    public class PageTitleConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!Enum.TryParse<Pages>(value?.ToString(), out Pages page))
            {
                return null;
            }

            return Properties.Resources.ResourceManager.GetString($"PageTitle_{page.ToString()}", Properties.Resources.Culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}