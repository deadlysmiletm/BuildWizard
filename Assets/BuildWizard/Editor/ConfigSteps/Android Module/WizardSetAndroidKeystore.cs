using UnityEditor;
using BuildWizard.Core;
using System.IO;
using System;
using UnityEngine;
using BuildWizard.Utilities;

namespace BuildWizard.ConfigSteps.AndroidModule
{
    internal struct WizardSetAndroidKeystore : IWizardStep
    {
        public string StepName => "Set Android KeyStore";
        public bool RequireUnityDomainCompilation => false;
        public string RequireSteps => string.Empty;

        public PathType PathOrigin;
        public string KeystorePath;
        public string KeystorePassword;
        public string KeystoreAlias;
        public string KeystoreAliasPassword;

        private string _path;

        public StepReport ExecuteStep(IWizardRepository repository)
        {
            StepReport report = new()
            {
                StepId = StepName,
                Success = false,
            };

            if (!CheckDataIntegrity())
            {
                report.ErrorReport = "Invalid Keystore data. Check the provided data and if the keystore file exist.";
                return report;
            }

            PlayerSettings.Android.keystoreName = _path;
            PlayerSettings.Android.keystorePass = KeystorePassword;
            PlayerSettings.Android.keyaliasName = KeystoreAlias;
            PlayerSettings.Android.keyaliasPass = KeystoreAliasPassword;

            report.Success = true;
            return report;
        }

        private bool CheckDataIntegrity()
        {
            bool integrity = true;
            if (string.IsNullOrEmpty(KeystorePassword))
                integrity = false;
            if (string.IsNullOrEmpty(KeystoreAlias))
                integrity = false;
            if (string.IsNullOrEmpty(KeystoreAliasPassword))
                integrity = false;

            _path = PathOrigin == PathType.FullCustom ? KeystorePath : $"{ConfigUtilities.GetOriginPath(PathOrigin)}/{KeystorePath}";

            if (string.IsNullOrEmpty(KeystorePath) || !File.Exists(_path))
            {
                Debug.Log($"File exist? {File.Exists(_path)}");
                integrity = false;
            }

            return integrity;
        }
    }
}