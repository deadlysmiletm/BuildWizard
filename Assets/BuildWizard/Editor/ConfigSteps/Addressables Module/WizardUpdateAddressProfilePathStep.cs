using BuildWizard.Core;
using BuildWizard.RepositoryKeys;
using System;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;

namespace BuildWizard.ConfigSteps.AddressablesModule
{
    internal struct WizardUpdateAddressProfilePathStep : IWizardStep
    {
        public string StepName => "Set Channel to Addressables Profile";
        public bool RequireUnityDomainCompilation => false;
        public string RequireSteps => "Collect Project Settings, Collect Addressables Data.";


        public string Profile;

        [NonSerialized] private AddressableAssetSettings _settings;


        public StepReport ExecuteStep(IWizardRepository repository)
        {
            StepReport report = new()
            {
                StepId = StepName,
                Success = false,
            };

            if (repository.ContainsData(WizardRepositoryKeys.ADDRESS_SETTINGS))
                _settings = repository.GetData<AddressableAssetSettings>(WizardRepositoryKeys.ADDRESS_SETTINGS);
            else
            {
                report.ErrorReport = "Can't find Addressables Settings on _repository. Add Collect Addressables Settings step before this one.";
                return report;
            }

            if (!CheckDataIntegrity())
            {
                StringBuilder errorBuilder = new();
                errorBuilder.Append("Can't find the profile \"")
                    .Append(Profile)
                    .Append("\". Check if the given profile exist and try again.");

                report.ErrorReport = errorBuilder.ToString();
                return report;
            }

            if (!repository.ContainsData(WizardRepositoryKeys.SETTING_COMPANY) || !repository.ContainsData(WizardRepositoryKeys.SETTING_PRODUCT_NAME))
            {
                report.ErrorReport = "The _repository don't had the required settings values. Add Collect Project Settings step before this one.";
                return report;
            }

            UpdateAddressProfilePath(repository);
            report.Success = true;
            return report;
        }

        private void UpdateAddressProfilePath(IWizardRepository repository)
        {
            string identifier = "";
            if (repository.ContainsData(WizardRepositoryKeys.SETTING_IDENTIFIER))
            {
                StringBuilder loadPathBuilder = new();
                loadPathBuilder.Append("sdcard/Android/obb/")
                    .Append(repository.GetData<string>(WizardRepositoryKeys.SETTING_IDENTIFIER));

                identifier = loadPathBuilder.ToString();
            }
            else
            {

                StringBuilder loadPathBuilder = new();
                loadPathBuilder.Append("sdcard/Android/obb/com.")
                    .Append(repository.GetData<string>(WizardRepositoryKeys.SETTING_COMPANY))
                    .Append(".")
                    .Append(repository.GetData<string>(WizardRepositoryKeys.SETTING_PRODUCT_NAME));

                identifier = loadPathBuilder.ToString();
            }

            _settings.profileSettings.SetValue(_settings.profileSettings.GetProfileId(Profile), "Local.LoadPath", identifier);
            EditorUtility.SetDirty(_settings);
        }

        private bool CheckDataIntegrity()
        {
            bool integrity = true;
            if (string.IsNullOrEmpty(Profile))
                integrity = false;
            if (string.IsNullOrEmpty(_settings.profileSettings.GetProfileId(Profile)))
                integrity = false;

            return integrity;
        }
    }
}