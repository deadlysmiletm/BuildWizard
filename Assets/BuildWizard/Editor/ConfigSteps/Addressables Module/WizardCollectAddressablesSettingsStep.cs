using BuildWizard.Core;
using BuildWizard.RepositoryKeys;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace BuildWizard.ConfigSteps.AddressablesModule
{
    internal struct WizardCollectAddressablesSettingsStep : IWizardStep
    {
        internal static class WizardAddressDefaultProvider
        {
            public static readonly string BuildScriptPath = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
            public static readonly string SettingsAssetPath = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        }

        public string StepName => "Collect Addressables Data";
        public bool RequireUnityDomainCompilation => false;
        public string RequireSteps => string.Empty;


        public StepReport ExecuteStep(IWizardRepository repository)
        {
            StepReport report = new()
            {
                StepId = StepName,
                Success = false,
            };

            if (CheckRepositoryDataDuplicated(repository))
            {
                report.ErrorReport = "The data requested already exist on the _repository. Check the builds steps to avoid an unexpected behaviours.";
                return report;
            }

            AddressableAssetSettings settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(WizardAddressDefaultProvider.SettingsAssetPath);
            IDataBuilder builder = (IDataBuilder)AssetDatabase.LoadAssetAtPath<ScriptableObject>(WizardAddressDefaultProvider.BuildScriptPath);

            if (settings == null || builder == null)
            {
                report.ErrorReport = "Can't find the requested files. Check the provided paths.";
                return report;
            }

            repository.AddData(WizardRepositoryKeys.ADDRESS_SETTINGS, settings);
            repository.AddData(WizardRepositoryKeys.ADDRESS_BUILDER, builder);

            report.Success = true;
            return report;
        }

        private bool CheckRepositoryDataDuplicated(IWizardRepository repository)
        {
            if (repository.ContainsData(WizardRepositoryKeys.ADDRESS_SETTINGS) || repository.ContainsData(WizardRepositoryKeys.ADDRESS_BUILDER))
                return true;
            return false;
        }
    }
}