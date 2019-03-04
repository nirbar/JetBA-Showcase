using Ninject;
using PanelSW.Installer.JetBA.ViewModel;
using System;
using System.IO;

namespace SampleJetBA.ViewModel
{
    public class InputValidationsViewModelEx : InputValidationsViewModel
    {
        public InputValidationsViewModelEx(SampleBA ba)
            : base(ba)
        {
        }

        public override void ValidatePage(object pageId)
        {
            Pages page = (Pages)pageId;

            switch (page)
            {
                case Pages.InstallLocation:
                    ValidateTargetFolder();
                    break;

                default:
                    break;
            }
        }

        public override void ValidateAll()
        {
            ValidateTargetFolder();
        }

        private void ValidateTargetFolder()
        {
            VariablesViewModel vars = BA.Kernel.Get<VariablesViewModel>();
            string installFolder = vars["InstallFolder"].String;

            if (string.IsNullOrWhiteSpace(installFolder) || (installFolder.IndexOfAny(Path.GetInvalidPathChars()) >= 0))
            {
                AddResult(new Exception(string.Format(Properties.Resources._0IsNotALegalFolderName, installFolder)));
            }
        }
    }
}
