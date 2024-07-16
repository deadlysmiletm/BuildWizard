using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using BuildWizard.Utilities;
using System.IO;
using BuildWizard.Core;
using System.Text;
using Newtonsoft.Json;
using BuildWizard.RepositoryKeys;
using System.Diagnostics;
using System.Threading.Tasks;
using System;

namespace BuildWizard.EditorTool
{
    public class WizardRunnerWindow : EditorWindow
    {
        [System.Serializable]
        private struct SerializedRunner
        {
            public DefaultRunnerRepository Repository;
            public int CurrentStep;
            public string Preset;
        }

        private static WizardRunnerWindow _instance;
        private static int _currentStep = 0;
        private Dictionary<string, WizardPresetAsset> _presets;
        private static WizardPresetAsset _currentPreset;
        private static DefaultRunnerRepository _repository;
        private List<string> _presetNames;
        private DropdownField presetsDropdown;

        private bool _customIdentifier = false;
        private bool _customBundlecode = false;
        private bool _advanceMode = false;

        private IntegerField _bundleCodeField;

        private string _buildIdentifier = "";
        private int _bundleCode = -1;

        [MenuItem("Build Wizard/Open Runner Window")]
        internal static void InitRunnerWindow()
        {
            _instance = EditorWindow.GetWindow<WizardRunnerWindow>();
            _currentStep = 0;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        }

        private void OnDestroy()
        {
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
        }

        public void CreateGUI()
        {
            WizardPresetAsset[] presets = LoadPresets();

            _presets = new();
            _presetNames = new();
            for (int i = 0; i < presets.Length; i++)
            {
                _presetNames.Add(presets[i].name);
                _presets.Add(presets[i].name, presets[i]);
            }

            VisualElement advanceMode = AdvanceModeView();
            //VisualElement simpleMode = SimpleMode();
            
            ////rootVisualElement.Add(container);
            //Toggle advanceToggle = new("Advance Mode:");
            //advanceToggle.RegisterValueChangedCallback(evt =>
            //{
            //    if (evt.newValue)
            //    {
            //        if (rootVisualElement.Contains(simpleMode))
            //            rootVisualElement.Remove(simpleMode);

            //        rootVisualElement.Add(advanceMode);
            //    }
            //    else
            //    {
            //        if (rootVisualElement.Contains(advanceMode))
            //            rootVisualElement.Remove(advanceMode);

            //        UpdateBundleField();
            //        rootVisualElement.Add(simpleMode);
            //    }
            //});
            //rootVisualElement.Add(advanceToggle);
            //Label title = new Label("Build Wizard Runner");
            //rootVisualElement.Add(title);


            rootVisualElement.Add(advanceMode);
        }

        private VisualElement SimpleMode()
        {
            VisualElement container = new();

            _bundleCodeField = new("Bundle Code:");
            _bundleCodeField.value = 0;
            _bundleCodeField.RegisterValueChangedCallback(evt => _bundleCode = evt.newValue);
            container.Add(_bundleCodeField);

            Button build = new(delegate
            {
                if (!_presets.ContainsKey("Build Quantum"))
                    return;

                _currentPreset = _presets["Build Quantum"];
                LaunchRunner(_bundleCode);
            });
            build.text = "Normal Build";
            build.style.marginBottom = 5;
            container.Add(build);


            Button buildAndUpload = new(delegate
            {
                if (!_presets.ContainsKey("Build Quantum -Autoupdate"))
                    return;

                _currentPreset = _presets["Build Quantum -Autoupdate"];
                LaunchRunner(_bundleCode);
            });
            buildAndUpload.text = "Build and Upload";
            container.Add(buildAndUpload);

            Button uploadLastBuild = new(delegate
            {

            });
            uploadLastBuild.text = "Upload last build";

            return container;
        }

        private void UpdateBundleField()
        {
            _bundleCodeField.value = _bundleCode;
        }

        private VisualElement AdvanceModeView()
        {
            VisualElement container = new();

            presetsDropdown = new("Presets:", _presetNames, _currentPreset != null ? _presetNames.IndexOf(_currentPreset.name) : 0);
            presetsDropdown.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == evt.previousValue)
                    return;

                _currentPreset = _presets[evt.newValue];
            });
            if (!_currentPreset)
                _currentPreset = _presets[_presetNames[0]];

            container.Add(presetsDropdown);

            TextField buildIdentifierField = new("Build Identifier:");
            buildIdentifierField.RegisterValueChangedCallback(evt => _buildIdentifier = evt.newValue);

            Toggle identifierToggle = new("Custom Identifier");
            identifierToggle.RegisterValueChangedCallback(evt =>
            {
                _customIdentifier = evt.newValue;
                if (_customIdentifier)
                {
                    if (!container.Contains(buildIdentifierField))
                        container.Insert(container.IndexOf(identifierToggle) + 1, buildIdentifierField);
                }
                else
                {
                    _buildIdentifier = "";
                    if (container.Contains(buildIdentifierField))
                        container.Remove(buildIdentifierField);
                }
            });
            container.Add(identifierToggle);


            IntegerField bundleCodeField = new("Bundle Code:");
            bundleCodeField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue < 0)
                {
                    bundleCodeField.value = evt.previousValue;
                    return;
                }

                _bundleCode = evt.newValue;
            });

            Toggle bundleCodeToggle = new("Custom bundle code");
            bundleCodeToggle.RegisterValueChangedCallback(evt =>
            {
                _customBundlecode = evt.newValue;
                if (_customBundlecode)
                {
                    if (!container.Contains(bundleCodeField))
                        container.Insert(container.IndexOf(bundleCodeToggle) + 1, bundleCodeField);
                }
                else
                {
                    if (container.Contains(bundleCodeField))
                        container.Remove(bundleCodeField);
                }
            });
            container.Add(bundleCodeToggle);

            Button launchButton = new(delegate
            {
				if (_customBundlecode)
					LaunchRunner(_bundleCode);
				else
					LaunchRunner();
            });
            launchButton.text = "Execute Runner";
            container.Add(launchButton);
            return container;
        }

        private static WizardPresetAsset[] LoadPresets()
        {
            string[] paths = AssetDatabase.FindAssets("t:WizardPresetAsset");
            WizardPresetAsset[] presets = new WizardPresetAsset[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                presets[i] = (WizardPresetAsset)AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(paths[i]));
            }

            return presets;
        }

        private async void LaunchRunner(int build = -1)
        {
            StringBuilder str = new();
            str.Append("Runner: ")
                .Append(_currentPreset.name)
                .Append(" <color=#32cd32>")
                .Append("Started")
                .Append("</color>")
                .Append("!");
            UnityEngine.Debug.Log(str.ToString());
            var progress = CreateProgressPanel();
            rootVisualElement.Add(progress);
            _repository = new DefaultRunnerRepository();

            if (build >= 0)
                _repository.AddData(WizardRepositoryKeys.SETTING_BUNDLE_CODE, _bundleCode);
            if (_customIdentifier && !string.IsNullOrEmpty(_buildIdentifier))
                _repository.AddData(WizardRepositoryKeys.BUILD_IDENTIFIER, _buildIdentifier);

            await ProcessSteps(_repository, 0);
        }

        public async static void ContinueRunner(string data)
        {
            InitRunnerWindow();

            SerializedRunner tempData = JsonConvert.DeserializeObject<SerializedRunner>(data);
            WizardPresetAsset[] presets = LoadPresets();

            for (int i = 0; i < presets.Length; i++)
            {
                if (presets[i].name == tempData.Preset)
                {
                    _currentPreset = presets[i];
                    break;
                }
            }
            _repository = tempData.Repository;
            _currentStep = tempData.CurrentStep;

            await _instance.ProcessSteps(_repository, _currentStep);
        }


        private async Task ProcessSteps(IWizardRepository repository, int startStep)
        {
            StepReport report = default;
            bool requireCompilation = false;
            for (int i = startStep; i < _currentPreset.buildSteps.Count; i++)
            {
                requireCompilation = _currentPreset.buildSteps[i].ParsedStep.RequireUnityDomainCompilation;
                if (requireCompilation)
                {
                    UnityEngine.Debug.Log("<color=Yellow>Waiting</color> for compilation...");
                    _currentStep = i + 1;
                    CreateTempInstruction();
                }

                report = _currentPreset.buildSteps[i].ParsedStep.ExecuteStep(repository);

                if (report.Success)
                {
                    UnityEngine.Debug.Log($"{report.StepId}: <color=#32cd32>Success</color>!");
                    if (repository.ContainsData(WizardRepositoryKeys.BUILD_FOLDER))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = repository.GetData<string>(WizardRepositoryKeys.BUILD_FOLDER),
                            UseShellExecute = true,
                            Verb = "open"
                        });
                    }
                }
                else
                {
                    UnityEngine.Debug.Log($"{report.StepId}: <color=#8b0000>Error</color>-> {report.ErrorReport}");
                    break;
                }

                if (requireCompilation)
                    Close();

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            EndProcessAndShowReport(report);
        }

        private void EndProcessAndShowReport(StepReport report)
        {
            if (File.Exists(GetTempFilePath()))
                File.Delete(GetTempFilePath());

            if (_instance == null)
                InitRunnerWindow();

            _instance.rootVisualElement.Add(new Label("Success!"));
        }

        private void CreateTempInstruction()
        {
            using (StreamWriter writer = File.CreateText(GetTempFilePath()))
            {
                writer.Write(JsonConvert.SerializeObject(new SerializedRunner
                {
                    CurrentStep = _currentStep,
                    Preset = _currentPreset.name,
                    Repository = _repository,
                }));
                writer.Flush();
            }
        }

        private VisualElement CreateProgressPanel()
        {
            Box progressContainer = new();
            return progressContainer;
        }

        public static string GetTempFilePath()
        {
            StringBuilder str = new();
            str.Append(Application.dataPath)
                .Append("/runner.temp");
            return str.ToString();
        }
    }

    public class ItemProgressElement : VisualElement
    {
        public Image progresImage;

        public ItemProgressElement(string stepName)
        {
            progresImage = new();
            progresImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Scripts/EditorTools/BuildWizard/Editor/cycle.png");

            this.contentContainer.style.flexDirection = FlexDirection.Row;
            this.contentContainer.Add(progresImage);
            this.contentContainer.Add(new Label(stepName));
        }

        public void CompleteProgress()
        {
            progresImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Scripts/EditorTools/BuildWizard/Editor/check-mark.png");
        }

        public void ErrorProgress()
        {
            progresImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Scripts/EditorTools/BuildWizard/Editor/cancel.png");
        }
    }
}