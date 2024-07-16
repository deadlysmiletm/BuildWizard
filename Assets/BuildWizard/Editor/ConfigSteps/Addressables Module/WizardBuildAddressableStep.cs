using BuildWizard.Core;
using BuildWizard.RepositoryKeys;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using System;
using System.Text;
using System.Linq;
using UnityEditor;

namespace BuildWizard.ConfigSteps.AddressablesModule
{
    internal struct WizardBuildAddressableStep : IWizardStep
    {
        [NonSerialized] private AddressableAssetSettings _settings;
        [NonSerialized] private IDataBuilder _builder;

        public string StepName => "Build Addressables";
        public bool RequireUnityDomainCompilation => false;
        public string RequireSteps => "Collect Addressables Data.";


        public string Profile;


        public StepReport ExecuteStep(IWizardRepository repository)
        {
            StepReport report = new()
            {
                StepId = StepName,
            };

            if (repository.ContainsData(WizardRepositoryKeys.ADDRESS_SETTINGS))
                _settings = repository.GetData<AddressableAssetSettings>(WizardRepositoryKeys.ADDRESS_SETTINGS);
            else
            {
                (report.Success, report.ErrorReport) = (false, "Can't find Addressables Settings on _repository. Add Collect Addressables Settings step before this one.");
                return report;
            }

            if (repository.ContainsData(WizardRepositoryKeys.ADDRESS_BUILDER))
                _builder = repository.GetData<IDataBuilder>(WizardRepositoryKeys.ADDRESS_BUILDER);
            else
            {
                (report.Success, report.ErrorReport) = (false, "Can't find Addressables Builder on _repository. Add Collect Addressables Settings step before this one.");
                return report;
            }

            SetProfile();
            SetBuilder();
            ClearBuildCache();

            var finalResult = StartBuildingContent();
            if (!finalResult.success)
            {
                StringBuilder errorBuilder = new("Error building Addressables. ");
                errorBuilder.Append(finalResult.result.Error);

                (report.Success, report.ErrorReport) = (false, errorBuilder.ToString());
                return report;
            }

            UploadResultToRepository(repository, finalResult.result);
            report.Success = true;
            return report;
        }

        private void SetProfile()
        {
            if (string.IsNullOrEmpty(Profile) || string.IsNullOrEmpty(_settings.profileSettings.GetProfileId(Profile)))
                Debug.LogWarning("WizardBuild: Can't find the profile requested. Using current profile instead.");
            else
            {
                _settings.activeProfileId = _settings.profileSettings.GetProfileId(Profile);
                EditorUtility.SetDirty(_settings);
            }
        }

        private void SetBuilder()
        {
            sbyte index = -1;
            for (byte i = 0; i < _settings.DataBuilders.Count; i++)
            {
                if (_settings.GetDataBuilder(i).Name == _builder.Name)
                {
                    index = (sbyte)i;
                    break;
                }
            }

            if (index == -1)
                Debug.LogWarning("WizardBuild: Can't find the builder requested. Using the last used instead.");
            else
                _settings.ActivePlayerDataBuilderIndex = index;
        }

        private void ClearBuildCache()
        {
            AddressableAssetSettings.CleanPlayerContent(_builder);
        }

        private (AddressablesPlayerBuildResult result, bool success) StartBuildingContent()
        {
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            return (result, string.IsNullOrEmpty(result.Error));
        }

        private void UploadResultToRepository(IWizardRepository repository, AddressableAssetBuildResult buildResult)
        {
            repository.AddData(WizardRepositoryKeys.ADDRESS_OUTPUT_PATH, buildResult.OutputPath);
            repository.AddData(WizardRepositoryKeys.ADDRESS_BUNDLE_PATHS, buildResult.FileRegistry.GetFilePaths().ToArray());
        }
    }
}