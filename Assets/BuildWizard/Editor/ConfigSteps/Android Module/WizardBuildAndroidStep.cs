using BuildWizard.Core;
using BuildWizard.RepositoryKeys;
using BuildWizard.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

namespace BuildWizard.ConfigSteps.AndroidModule
{
    public struct WizardBuildAndroidStep : IWizardStep
    {
        public string StepName => "Build Android";
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
            string buildPath = GenerateBuildName(today, identifierVersion, folderPath, repository);
            string endBuildPath = $"{buildPath}.apk";

            repository.AddData(WizardRepositoryKeys.BUILD_FOLDER, folderPath.Replace('\\', '/'));
            repository.AddData(WizardRepositoryKeys.BUILD_PATH, endBuildPath);

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
                target = BuildTarget.Android,
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

            string[] filesPath = Directory.GetFiles(folderPath);
            string obbPath = string.Empty;
            for (int i = 0; i < filesPath.Length; i++)
            {
                if (filesPath[i].EndsWith(".obb"))
                {
                    obbPath = filesPath[i];
                    break;
                }
            }

            if (!string.IsNullOrEmpty(obbPath))
            {
                string obbName = new StringBuilder("main.")
                        .Append(repository.GetData<int>(WizardRepositoryKeys.SETTING_BUNDLE_CODE))
                        .Append(".com.")
                        .Append(repository.GetData<string>(WizardRepositoryKeys.SETTING_COMPANY))
                        .Append(".")
                        .Append(repository.GetData<string>(WizardRepositoryKeys.SETTING_PRODUCT_NAME))
                        .Append(".obb")
                        .ToString();
                string finalObbPath = Path.Combine(folderPath, obbName);

                try
                {
                    File.Move(obbPath, finalObbPath);
                    repository.AddData(WizardRepositoryKeys.BUILD_EXTENTION_FILE, finalObbPath);
                }
                catch (Exception ex)
                {
                    report.ErrorReport = $"Moving OBB - {ex.ToString()}";
                    return report;
                }
            }

            if (repository.ContainsData(WizardRepositoryKeys.ADDRESS_BUNDLE_PATHS))
            {
                string[] paths = repository.GetData<string[]>(WizardRepositoryKeys.ADDRESS_BUNDLE_PATHS).Distinct().ToArray();
                string destinationFolder = Path.Combine(folderPath, "Assets");
                if (!Directory.Exists(destinationFolder))
                    Directory.CreateDirectory(destinationFolder);


                string finalBundlePath = string.Empty;
                for (int i = 0; i < paths.Length; i++)
                {
                    paths[i] = paths[i].Replace('\\', '/');
                    finalBundlePath = $"{destinationFolder}\\{paths[i].Split('/')[^1]}";

                    try
                    {
                        File.Move(paths[i], finalBundlePath);
                    }
                    catch (Exception ex)
                    {
                        report.ErrorReport = $"Moving Addressables - {ex.ToString()}";
                        return report;
                    }
                }

                UnityEngine.Debug.Log(destinationFolder);
                repository.AddData(WizardRepositoryKeys.ADDRESS_EXPORTED_BUNDLE_PATH, Path.Combine(folderPath, "Assets").Replace('\\', '/'));
            }

            UnityEngine.Debug.Log("Build size:" + unityReport.summary.totalSize + " bytes");
            //Process.Start(new ProcessStartInfo
            //{
            //    FileName = folderPath,
            //    UseShellExecute = true,
            //    Verb = "open"
            //});

            report.Success = true;
            return report;
        }


        private bool CheckRequiredData(IWizardRepository repository)
        {
            bool requirenmentSuccess = true;

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                requirenmentSuccess = false;

            if (!repository.ContainsData(WizardRepositoryKeys.SETTING_PRODUCT_NAME))
                requirenmentSuccess = false;

            return requirenmentSuccess;
        }

        private string GenerateBuildPath(DateTime today, string identifierVersion, IWizardRepository repository)
        {
            StringBuilder path = new();
            string originPath =  ConfigUtilities.GetOriginPath(OutputStartPath);

            if (!string.IsNullOrEmpty(originPath))
            {
                path.Append(Path.Combine(originPath))
                    .Append("\\")
                    .Append(OutputFolderPath);
            }
            else
                path.Append(OutputFolderPath);

            path.Append("\\")
                .Append(today.Day)
                .Append("-")
                .Append(today.Month)
                .Append("\\")
                .Append(identifierVersion)
                .Append("_")
                .Append(today.Hour)
                .Append("h\\");

            return path.ToString();
        }

        private string GenerateBuildName(DateTime today, string identifierVersion, string folder, IWizardRepository repository)
        {
            StringBuilder buildName = new(folder);
            buildName.Append(repository.GetData<string>(WizardRepositoryKeys.SETTING_PRODUCT_NAME))
                .Append("_")
                .Append(today.Hour)
                .Append("h-")
                .Append(today.Minute)
                .Append("m_")
                .Append(identifierVersion);
            return buildName.ToString();
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