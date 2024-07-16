using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using BuildWizard.Core;
using BuildWizard.Utilities;
using System.Text;
using System.Linq;

namespace BuildWizard.EditorTool
{
    [CustomEditor(typeof(WizardPresetAsset))]
    public class WizardPresetAssetEditor : Editor
    {
        private WizardPresetAsset _target;
        private Dictionary<string, Type> _stepsTypesDic;
        private List<string> _stepNames;


        private void OnEnable()
        {
            _target = (WizardPresetAsset)target;
            InitializeEditorData();
        }

        private void OnDisable() => serializedObject.ApplyModifiedProperties();


        public override VisualElement CreateInspectorGUI()
        {
            VisualElement container = new();
            VisualElement header = CreateHeader();
            VisualElement content = CreateMainContent();

            container.Add(header);
            container.Add(content);

            return container;
        }


        private void InitializeEditorData()
        {
            Span<Type> allStepTypes = ConfigUtilities.GetAllScriptsByInterface<IWizardStep>();
            _stepNames = new();
            _stepsTypesDic = new();
            string stepName = "";

            for (int i = 0; i < allStepTypes.Length; i++)
            {
                stepName = ConfigUtilities.CreateNewValueInstance<IWizardStep>(allStepTypes[i]).StepName;
                _stepNames.Add(stepName);
                _stepsTypesDic.Add(stepName, allStepTypes[i]);
            }
            _stepNames = _stepNames.OrderBy(s => s).ToList();
        }

        private VisualElement CreateHeader()
        {
            VisualElement container = new();
            container.style.marginBottom = 10;

            Image logoImage = new()
            {
                sprite = Resources.Load<Sprite>("PressetAssetLogo"),
                style =
                {
                    maxHeight = 240,
                    marginTop = -27,
                }
            };

            Label titleLabel = new("Wizard Preset Asset")
            {
                style =
                {
                    alignSelf = Align.Center,
                    fontSize = 22,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 10,
                    marginTop = 0,
                    borderBottomWidth = 4.5f,
                    borderBottomLeftRadius = 20,
                    borderBottomRightRadius = 20,
                    borderBottomColor = new Color(128, 128, 128, 1f),
                }
            };

            Label subTitleLabel = new(_target.name)
            {
                style =
                {
                    alignSelf = Align.Center,
                    fontSize = 18,
                    unityFontStyleAndWeight = FontStyle.Italic,
                    marginBottom = 30,
                }
            };

            container.Add(logoImage);
            container.Add(titleLabel);
            container.Add(subTitleLabel);

            return container;
        }

        private VisualElement CreateMainContent()
        {
            VisualElement container = new();

            Box listPanel = new()
            {
                style =
                {
                    borderTopLeftRadius = 20,
                    borderTopRightRadius = 20,
                }
            };
            ListView list = default;
            list = new(_target.buildSteps,
                makeItem: delegate
                {
                    return new StepVisualElement();
                },
                bindItem: (el, index) =>
                {
                    var stepVisual = (StepVisualElement)el;
                    SerializedProperty buildStepProperty = serializedObject.FindProperty("buildSteps");
                    SerializedProperty stepProperty = buildStepProperty.GetArrayElementAtIndex(index).FindPropertyRelative("ParsedStep");

                    StepVisualElement.StepVisualArgs args = new()
                    {
                        editorRef = this,
                        stepDataProperty = stepProperty,
                        list = list,
                    };

                    stepVisual.Config(args);
                    serializedObject.ApplyModifiedProperties();
                });
            list.itemIndexChanged += (int prevIndex, int newIndex) =>
            {
                serializedObject.ApplyModifiedProperties();

                SerializedProperty stepProperty = serializedObject.FindProperty("buildSteps");
                SerializedProperty prevIndexProperty = stepProperty.GetArrayElementAtIndex(prevIndex);
                SerializedProperty newIndexProperty = stepProperty.GetArrayElementAtIndex(newIndex);
                var prevValueBoxed = prevIndexProperty.boxedValue;

                prevIndexProperty.boxedValue = newIndexProperty.boxedValue;
                newIndexProperty.boxedValue = prevValueBoxed;
                serializedObject.ApplyModifiedProperties();
            };
            list.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            list.reorderable = true;
            list.reorderMode = ListViewReorderMode.Animated;
            list.selectionType = SelectionType.None;

            Button newStepButton = new(delegate
            {
                AddNewStep();
                serializedObject.ApplyModifiedProperties();
                list.Rebuild();
            });
            newStepButton.text = "Add new Step";
            listPanel.Add(list);

            container.Add(listPanel);
            container.Add(newStepButton);

            return container;
        }

        private void AddNewStep()
        {
            SerializedProperty stepsProperty = serializedObject.FindProperty("buildSteps");
            stepsProperty.arraySize += 1;
            stepsProperty.GetArrayElementAtIndex(stepsProperty.arraySize - 1).boxedValue = new WizardPresetAsset.WizardStepData();

            SerializedProperty newStepProperty = stepsProperty.GetArrayElementAtIndex(stepsProperty.arraySize - 1).FindPropertyRelative("ParsedStep");
            newStepProperty.managedReferenceValue = ConfigUtilities.CreateNewValueInstance<IWizardStep>(_stepsTypesDic[_stepNames[0]]);
        }


        private class StepVisualElement : VisualElement
        {
            private WizardPresetAssetEditor _configReference;
            private SerializedProperty _stepProperty;
            private VisualElement _fieldsContainer;
            private DropdownField _stepsTypesDropdown;
            private Button _closeButton;
            private ListView _list;
            private HelpBox _requirementPanel;
            private VisualElement _rightBox;

            public StepVisualElement() { }

            internal struct StepVisualArgs
            {
                public WizardPresetAssetEditor editorRef;
                public SerializedProperty stepDataProperty;
                public ListView list;
            }

            public void Config(StepVisualArgs args)
            {
                contentContainer.Clear();

                _configReference = args.editorRef;
                _stepProperty = args.stepDataProperty;
                _list = args.list;
                _requirementPanel = new("", HelpBoxMessageType.Info)
                {
                    style =
                    {
                        marginLeft = 30,
                    }
                };

                contentContainer.style.borderBottomColor = new Color(0f, 0f, 0f, 0.41f);
                contentContainer.style.borderBottomWidth = 0.6f;

                VisualElement root = GenerateRootContainer();
                VisualElement leftBox = new()
                {
                    style =
                    {
                        alignSelf = Align.Center,
                    }
                };
                _rightBox = new();

                root.Add(_rightBox);
                root.Add(leftBox);
                contentContainer.Add(root);

                _fieldsContainer = new();
                _stepsTypesDropdown = new("Steps:", args.editorRef._stepNames, args.editorRef._stepNames.IndexOf((args.stepDataProperty.managedReferenceValue as IWizardStep).StepName))
                {
                    style =
                    {
                        marginBottom = 6,
                    }
                };
                _stepsTypesDropdown.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue == evt.previousValue)
                        return;

                    UpdateCurrentStep(evt.newValue);
                });
                _rightBox.Add(_stepsTypesDropdown);
                _rightBox.Add(_fieldsContainer);

                _closeButton = new(delegate
                {
                    lock (this)
                    {
                        int index = -1;
                        SerializedProperty property = _configReference.serializedObject.FindProperty("buildSteps");
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            if (property.GetArrayElementAtIndex(i).FindPropertyRelative("ParsedStep").boxedValue == _stepProperty.boxedValue)
                            {
                                index = i;
                                break;
                            }
                        }
                        _configReference.serializedObject.FindProperty("buildSteps").DeleteArrayElementAtIndex(index);
                        _configReference.serializedObject.ApplyModifiedProperties();
                        _list.Rebuild();
                    }
                })
                {
                    text = "X",
                    style =
                    {
                        marginLeft = 10,
                        paddingBottom = 5,
                        paddingTop = 5,
                    }
                };

                leftBox.Add(_closeButton);

                if (args.stepDataProperty.managedReferenceValue == null)
                    UpdateCurrentStep(_configReference._stepNames[0]);
                else
                    UpdatePropertiesFields();
            }

            private VisualElement GenerateRootContainer()
            {
                VisualElement container = new()
                {
                    style =
                    {
                        marginBottom = 15,
                        marginTop = 15,
                        marginLeft = 5,
                        marginRight = 5,
                        letterSpacing = 9,
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.SpaceBetween,
                    }
                };
                return container;
            }

            private void UpdatePropertiesFields()
            {
                _stepProperty.serializedObject.ApplyModifiedProperties();
                _fieldsContainer.Clear();

                Span<(string fieldName, Type fieldType)> fields = ConfigUtilities.GetFields(_stepProperty.boxedValue.GetType());
                for (int i = 0; i < fields.Length; i++)
                {
                    _fieldsContainer.Add(SerializedPropertyBinder.GenerateAndBindPropertyField(fields[i].fieldName, fields[i].fieldType, _stepProperty));
                }
                string stepRequirements = ConfigUtilities.GetRequireStep(_stepProperty.boxedValue);

                if (!string.IsNullOrEmpty(stepRequirements))
                {
                    StringBuilder str = new("<b>Requirement:</b> ");
                    str.Append(stepRequirements);
                    _requirementPanel.text = str.ToString();

                    if (!_rightBox.Contains(_requirementPanel))
                        _rightBox.Add(_requirementPanel);
                }
                else
                {
                    if (_rightBox.Contains(_requirementPanel))
                        _rightBox.Remove(_requirementPanel);
                }

                _stepProperty.serializedObject.ApplyModifiedProperties();
            }

            private void UpdateCurrentStep(string newStep)
            {
                _stepProperty.managedReferenceValue = ConfigUtilities.CreateNewValueInstance<IWizardStep>(_configReference._stepsTypesDic[newStep]);
                UpdatePropertiesFields();
            }
        }
    }
}