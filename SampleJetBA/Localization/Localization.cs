using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SampleJetBA.Localization
{
    public class Localization : PanelSW.Installer.JetBA.Localization.Resources
    {
        private ImageSource informationIcon_;
        public override ImageSource InformationIcon => informationIcon_ ?? (informationIcon_ = GetIcon("baseline_notification_important_black_36dp.png"));

        private ImageSource warningIcon_;
        public override ImageSource WarningIcon => warningIcon_ ?? (warningIcon_ = GetIcon("baseline_warning_black_36dp.png"));

        private ImageSource questionIcon_;
        public override ImageSource QuestionIcon => questionIcon_ ?? (questionIcon_ = GetIcon("baseline_help_black_36dp.png"));

        private ImageSource errorIcon_;
        public override ImageSource ErrorIcon => errorIcon_ ?? (errorIcon_ = GetIcon("baseline_error_black_36dp.png"));

        public override ImageSource UserIcon => InformationIcon;

        private ImageSource GetIcon(string png)
        {
            string ImagesPath = $"pack://application:,,/SampleJetBA;component/Resources/{png}";
            Uri uri = new Uri(ImagesPath, UriKind.RelativeOrAbsolute);
            BitmapImage bitmap = new BitmapImage(uri);
            return bitmap;
        }
    }
}
