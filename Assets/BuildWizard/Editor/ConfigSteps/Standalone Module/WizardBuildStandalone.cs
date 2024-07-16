using BuildWizard.Core;
using UnityEditor;
using UnityEngine;
using BuildWizard.Utilities;
using BuildWizard.RepositoryKeys;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BuildWizard.ConfigSteps.StandaloneModule
{
    public struct WizardBuildStandalone : IWizardStep
    {
        public string StepName => "Build Standalone & Server";
        public bool RequireUnityDomainCompilation => false;
        public string RequireSteps => "Collect Project settings";


        public PathType OutputStartPath;
        public string OutputFolderPath;
        public BuildOptions ExtraOption;
        public string ExtraScriptingDefines;
        public bool DeleteDoNotShipFolders;


        public StepReport ExecuteStep(IWizardRepository repository)
        {
            StepReport report = new()
            {
                Success = false,
                StepId = StepName,
            };

            if (!CheckRequiredData(repository))
            {
                report.ErrorReport = "Errour found! Can't find the required data. Check if all steps are setted correctly.";
                return report;
            }

            string identifierVersion = "";
            if (repository.ContainsData(WizardRepositoryKeys.BUILD_IDENTIFIER))
                identifierVersion = repository.GetData<string>(WizardRepositoryKeys.BUILD_IDENTIFIER);
            else
                identifierVersion = "build";

            string[] scenesPath = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < scenesPath.Length; i++)
                scenesPath[i] = EditorBuildSettings.scenes[i].path;

            DateTime today = DateTime.Now;
            string folderPath = GenerateBuildPath(today, identifierVersion, repository);
            string endBuildPath = $"{folderPath}\\Game.exe";
            repository.AddData(WizardRepositoryKeys.BUILD_PATH, endBuildPath);
            repository.AddData(WizardRepositoryKeys.BUILD_FOLDER, folderPath.Replace('\\', '/'));

            string[] extraDefines = Array.Empty<string>();

            if (!string.IsNullOrEmpty(ExtraScriptingDefines))
                extraDefines = ExtraScriptingDefines.Split(',');

            for (int i = 0; i < extraDefines.Length; i++)
            {
                if (extraDefines[i].StartsWith(' '))
                    extraDefines[i] = extraDefines[i].Replace(" ", "");
            }

            BuildPlayerOptions buildOptions = new()
            {
                locationPathName = endBuildPath,
                scenes = scenesPath,
                target = EditorUserBuildSettings.activeBuildTarget,
#if UNITY_SERVER
                subtarget = (int)StandaloneBuildSubtarget.Server,
#else
                subtarget = (int)StandaloneBuildSubtarget.Player,
#endif
                options = ExtraOption,
                extraScriptingDefines = extraDefines,
            };

            UnityEditor.Build.Reporting.BuildReport unityReport = BuildPipeline.BuildPlayer(buildOptions);
            if (unityReport.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                var summary = unityReport.summary;
                StringBuilder str = new();
                str.Append("Error found when building. Reason: ")
                    .Append(summary.result);
                report.ErrorReport = str.ToString();
                return report;
            }

            if (DeleteDoNotShipFolders)
                DeleteDebugFolders(folderPath);

            Debug.Log("Build size:" + unityReport.summary.totalSize + " bytes");
            report.Success = true;
            return report;
        }


        private bool CheckRequiredData(IWizardRepository repository)
        {
            bool requirenmentSuccess = true;

            if (EditorUserBuildSettings.standaloneBuildSubtarget != StandaloneBuildSubtarget.Server)
                requirenmentSuccess = false;

            return requirenmentSuccess;
        }

        private string GenerateBuildPath(DateTime today, string identifierVersion, IWizardRepository repository)
        {
            StringBuilder path = new();
            string originPath = ConfigUtilities.GetOriginPath(OutputStartPath);

            if (!string.IsNullOrEmpty(originPath))
            {
                path.Append(Path.Combine(originPath))
                    .Append("\\")
                    .Append(OutputFolderPath);
            }
            else
                path.Append(OutputFolderPath);

#if UNITY_SERVER
            path.Append("\\")
                .Append(today.Day)
                .Append("-")
                .Append(today.Month)
                .Append("\\Server")
                .Append(identifierVersion)
                .Append("\\");
#else
            path.Append("\\")
                .Append(today.Day)
                .Append("-")
                .Append(today.Month)
                .Append("\\")
                .Append(identifierVersion)
                .Append("_")
                .Append(today.Hour)
                .Append("h\\");
#endif

            return path.ToString();
        }

        private void DeleteDebugFolders(string buildPath)
        {
            string[] allDirecties = Directory.GetDirectories(buildPath);

            for (int i = 0; i < allDirecties.Length; i++)
            {
                if (Regex.IsMatch(allDirecties[i], @"DoNotShip.*"))
                    Directory.Delete(allDirecties[i], true);
            }
        }
    }
}