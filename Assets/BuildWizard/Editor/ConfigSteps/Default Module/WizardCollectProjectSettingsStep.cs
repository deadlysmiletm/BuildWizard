using BuildWizard.Core;
using BuildWizard.RepositoryKeys;
using System.Text;
using UnityEditor;

namespace BuildWizard.ConfigSteps.DefaultModule
{
    internal struct WizardCollectProjectSettingsStep : IWizardStep
    {
        public string StepName => "Collect Project Settings";
        public bool RequireUnityDomainCompilation => false;
        public string RequireSteps => string.Empty;


        public StepReport ExecuteStep(IWizardRepository repository)
        {
            StepReport report = new()
            {
                StepId = StepName,
                Success = true,
            };

            repository.AddData(WizardRepositoryKeys.SETTING_COMPANY, PlayerSettings.companyName);
            repository.AddData(WizardRepositoryKeys.SETTING_PRODUCT_NAME, PlayerSettings.productName);
            repository.AddData(WizardRepositoryKeys.SETTING_USE_APK_BUNDLES, PlayerSettings.Android.useAPKExpansionFiles);

            StringBuilder identifierBuilder = new();
            identifierBuilder.Append("com.")
                .Append(PlayerSettings.companyName)
                .Append(".")
                .Append(PlayerSettings.productName);

            repository.AddData(WizardRepositoryKeys.SETTING_IDENTIFIER, identifierBuilder);

            return report;
        }
    }
}