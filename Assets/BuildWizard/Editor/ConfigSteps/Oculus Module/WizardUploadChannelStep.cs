using BuildWizard.Core;
using System.Diagnostics;
using UnityEngine;
using BuildWizard.RepositoryKeys;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace BuildWizard.ConfigSteps.OculusModule
{
    public struct WizardUploadChannelStep : IWizardStep
    {
        public enum EAgeTarget
        {
            None,
            TEENS_AND_ADULTS,
            MIXED_AGES,
            CHILDREN,
        }

        public string StepName => "Upload channel";
        public bool RequireUnityDomainCompilation => false;
        public string RequireSteps => "Update Channel Config, Build Path (collecte last buid or a build step).";

        public EAgeTarget ageTarget;
        public bool MarkAllBundlesHasRequired;
        public string ExcludePackagesFromRequired;
        public string IncludePackagesHasRequired;
        

        public StepReport ExecuteStep(IWizardRepository repository)
        {
            StepReport report = new()
            {
                StepId = StepName,
                Success = false,
            };

            if (!repository.ContainsData(WizardRepositoryKeys.CHANNEL_APP_ID) || !repository.ContainsData(WizardRepositoryKeys.BUILD_PATH))
            {
                report.ErrorReport = "Can't find the required arguments. Please check if the required steps are added before this one.";
                return report;
            }

            //string args = $"upload-quest-build --app-id {repository.GetData<string>(WizardRepositoryKeys.CHANNEL_APP_ID)} --app-secret {repository.GetData<string>(WizardRepositoryKeys.CHANNEL_SECRET)} --apk \"{repository.GetData<string>(WizardRepositoryKeys.BUILD_PATH).Replace('\\', '/')}\" --channel {repository.GetData<string>(WizardRepositoryKeys.CHANNEL_OCULUS_CHANNEL)}";
            StringBuilder argsBuilder = new();
            argsBuilder
                .Append("upload-quest-build --app-id ")
                .Append(repository.GetData<string>(WizardRepositoryKeys.CHANNEL_APP_ID))
                .Append(" --app-secret ")
                .Append(repository.GetData<string>(WizardRepositoryKeys.CHANNEL_SECRET))
                .Append(" --apk \"")
                .Append(repository.GetData<string>(WizardRepositoryKeys.BUILD_PATH).Replace('\\', '/'))
                .Append("\" ");

            if (repository.ContainsData(WizardRepositoryKeys.BUILD_EXTENTION_FILE))
            {
                argsBuilder.Append("--obb \"")
                    .Append(repository.GetData<string>(WizardRepositoryKeys.BUILD_EXTENTION_FILE).Replace("\\", "/"))
                    .Append("\" ");
            }

            if (repository.ContainsData(WizardRepositoryKeys.ADDRESS_EXPORTED_BUNDLE_PATH))
            {
                string exportedAddressablesPath = repository.GetData<string>(WizardRepositoryKeys.ADDRESS_EXPORTED_BUNDLE_PATH);
                argsBuilder.Append("--assets-dir \"")
                    .Append(exportedAddressablesPath)
                    .Append("\" ");

                if (MarkAllBundlesHasRequired || !string.IsNullOrEmpty(IncludePackagesHasRequired))
                {
                    string jsonConfig = GenerateJsonAddressablesConfig(repository.GetData<string>(WizardRepositoryKeys.ADDRESS_EXPORTED_BUNDLE_PATH));
                    StringBuilder jsonPathBuilder = new();
                    jsonPathBuilder.Append(repository.GetData<string>(WizardRepositoryKeys.BUILD_FOLDER))
                        .Append("/BundlesConfig.json");
                    File.WriteAllText(jsonPathBuilder.ToString(), jsonConfig);

                    argsBuilder.Append("--asset-files-config ")
                        .Append(jsonPathBuilder.ToString())
                        .Append(" ");
                }
            }

            if (ageTarget != EAgeTarget.None)
            {
                argsBuilder.Append("--age-group ")
                    .Append(ageTarget.ToString())
                    .Append(" ");
            }

            argsBuilder.Append("--channel ")
                .Append(repository.GetData<string>(WizardRepositoryKeys.CHANNEL_OCULUS_CHANNEL));

            string args = argsBuilder.ToString();
            UnityEngine.Debug.Log(args);

            Process uploadProcess = new();

            uploadProcess.StartInfo = new ProcessStartInfo()
            {
                FileName = $"{Application.dataPath}\\BuildWizard\\Editor\\ConfigSteps\\Oculus Module\\ovr-platform-util.exe",
                Arguments = args,
                UseShellExecute = true,
            };

            uploadProcess.EnableRaisingEvents = true;
            uploadProcess.Exited += delegate
            {
                UnityEngine.Debug.Log("Upload process ended.");
                string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Temp";
                var paths = Directory.GetFiles(path);

                UnityEngine.Debug.Log(paths.Length);
                DateTime newestFile = DateTime.MinValue;
                string logFile = "";

                for (int i = 0; i < paths.Length; i++)
                {
                    if (Regex.IsMatch(paths[i], @"oc_cli_.*\.log$"))
                    {
                        DateTime fileLastDate = File.GetLastWriteTime(paths[i]);
                        if (fileLastDate > newestFile)
                        {
                            newestFile = fileLastDate;
                            logFile = paths[i];
                        }
                    }
                }

                if (!string.IsNullOrEmpty(logFile))
                    Process.Start("notepad.exe", logFile);
            };
            uploadProcess.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                UnityEngine.Debug.Log("Error found when uploading. " + e.Data);
            };

            uploadProcess.Start();

            report.Success = true;
            return report;
        }

        public string GenerateJsonAddressablesConfig(string addressableFolder)
        {
            string[] allBundles = Directory.GetFiles(addressableFolder);
            string bundleName = string.Empty;
            
            HashSet<string> excludePacks = HashSetPool<string>.Get();

            if (!string.IsNullOrEmpty(ExcludePackagesFromRequired))
            {
                string[] allExclude = ExcludePackagesFromRequired.Split('.');
                string tempName = string.Empty;
                for (int i = 0; i < allExclude.Length; i++)
                {
                    tempName = allExclude[i].Replace(" ", "");

                    if (!excludePacks.Contains(tempName))
                        excludePacks.Add(tempName);
                }
            }

            HashSet<string> includePacks = HashSetPool<string>.Get();

            if (!string.IsNullOrEmpty(IncludePackagesHasRequired))
            {
                string[] allInclude = IncludePackagesHasRequired.Split('.');
                string tempName = string.Empty;
                for (int i = 0; i < allInclude.Length; i++)
                {
                    tempName = allInclude[i].Replace(" ", "");

                    if (!excludePacks.Contains(tempName))
                        excludePacks.Add(tempName);
                }
            }

            StringBuilder jsonBuilder = new("{");
            for (int i = 0; i < allBundles.Length; i++)
            {
                bundleName = allBundles[i].Split('\\')[^1];
                jsonBuilder.Append("\"")
                    .Append(bundleName)
                    .Append("\": {\"required\": ");

                if (MarkAllBundlesHasRequired && !excludePacks.Contains(bundleName) || includePacks.Contains(bundleName))
                    jsonBuilder.Append("true}");
                else
                    jsonBuilder.Append("false}");

                if (i < allBundles.Length - 1)
                    jsonBuilder.Append(", ");
            }
            jsonBuilder.Append("}");
            HashSetPool<string>.Release(includePacks);
            HashSetPool<string>.Release(excludePacks);

            return jsonBuilder.ToString();
        }
    }
}