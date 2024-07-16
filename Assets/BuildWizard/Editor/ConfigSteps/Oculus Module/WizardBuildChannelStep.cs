using BuildWizard.Core;
using BuildWizard.RepositoryKeys;
using System.Text;
using UnityEditor;

namespace BuildWizard.ConfigSteps.OculusModule
{
    internal struct WizardBuildChannelStep : IWizardStep
    {
        public enum ReleaseChannel
        {
            ALPHA,
            BETA,
            DEMO,
            RELEASE
        }

        public string StepName => "Update Channel Config";
        public bool RequireUnityDomainCompilation => false;
        public string RequireSteps => "Collect Project Settings";


        public string ChannelName;
        public string AppID;
        public string SecretKey;
        public int BundleCode;
        public ReleaseChannel VersionChannel;


        public StepReport ExecuteStep(IWizardRepository repository)
        {
            StepReport report = new()
            {
                StepId = StepName,
                Success = false,
            };

            if (!CheckDataIntegrity())
            {
                report.ErrorReport = "The provided data can't be empty.";
                return report;
            }

            if (!repository.ContainsData(WizardRepositoryKeys.SETTING_COMPANY))
            {
                report.ErrorReport = "Can't get the required data. Add Collect Project Settings step before this one.";
                return report;
            }

            PlayerSettings.productName = ChannelName;
            repository.AddData(WizardRepositoryKeys.SETTING_PRODUCT_NAME, ChannelName);

            StringBuilder identifierBuilder = new();
            identifierBuilder.Append("com.")
                .Append(repository.GetData<string>(WizardRepositoryKeys.SETTING_COMPANY))
                .Append(".")
                .Append(ChannelName);

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, identifierBuilder.ToString());
            repository.AddData(WizardRepositoryKeys.SETTING_IDENTIFIER, identifierBuilder.ToString());

            if (repository.ContainsData(WizardRepositoryKeys.SETTING_BUNDLE_CODE))
                PlayerSettings.Android.bundleVersionCode = repository.GetData<int>(WizardRepositoryKeys.SETTING_BUNDLE_CODE);
            else
            {
                PlayerSettings.Android.bundleVersionCode = BundleCode;
                repository.AddData(WizardRepositoryKeys.SETTING_BUNDLE_CODE, BundleCode);
            }

            repository.AddData(WizardRepositoryKeys.CHANNEL_APP_ID, AppID);
            repository.AddData(WizardRepositoryKeys.CHANNEL_SECRET, SecretKey);
            repository.AddData(WizardRepositoryKeys.CHANNEL_NAME, ChannelName);
            repository.AddData(WizardRepositoryKeys.CHANNEL_OCULUS_CHANNEL, VersionChannel.ToString());

            Oculus.Platform.PlatformSettings.AppID = AppID;
            Oculus.Platform.PlatformSettings.MobileAppID = AppID;

            report.Success = true;
            return report;
        }

        private bool CheckDataIntegrity()
        {
            bool integrity = true;

            if (string.IsNullOrEmpty(ChannelName))
                integrity = false;

            return integrity;
        }
    }
}