using UnityEditor;
using BuildWizard.EditorTool;
using System.IO;

namespace BuildWizard.Utilities
{
    public static class WizardRunnerTempLoader
    {
        [InitializeOnLoadMethod]
        private static void LoadRunnerTempFile()
        {
            string path = WizardRunnerWindow.GetTempFilePath();
            if (File.Exists(path))
            {
                using (StreamReader reader = File.OpenText(path))
                {
                    string data = reader.ReadToEnd();
                    WizardRunnerWindow.ContinueRunner(data);
                }

                File.Delete(path);
            }
        }
    }
}