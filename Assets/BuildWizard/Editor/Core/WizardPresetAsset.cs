using BuildWizard.Core;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace BuildWizard.Core
{
    [CreateAssetMenu(menuName = "Build Wizard/New Preset")]
    internal class WizardPresetAsset : ScriptableObject
    {
        [Serializable]
        internal struct WizardStepData
        {
            [SerializeReference]
            public IWizardStep ParsedStep;
        }

        [SerializeField]
        public List<WizardStepData> buildSteps = new();
    }
}