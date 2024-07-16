using UnityEditor.Build;
using BuildWizard.Core;
using UnityEditor;

namespace BuildWizard.ConfigSteps.DefaultModule
{
    internal struct WizardChangePlatformStep : IWizardStep
    {
        internal enum BuildPlatform
        {
            None = 0,
            Standalone_Windows = 1,
            Standalone_Linux = 2,
            Standalone_OSX = 3,
            
            Server_Windows = 10,
            Server_Linux = 11,
            Server_OSX = 12,
            Linux_Headless_Simulation = 13,
            Embedded_Linux = 14,

            iOS = 20,
            Android = 21,
            VisionOS = 22,
            TvOS = 23,

            PS4 = 30,
            PS5 = 31,
            XboxOne = 32,
            GameCore_XboxOne = 33,
            GameCore_XboxSeries = 34,
            Nintendo_Switch = 35,
            Stadia = 36,

            WebGL = 40,
            Windows_Store_Apps = 41,
            QNX = 42,
        }

        public string StepName => "Change Platform";
        public bool RequireUnityDomainCompilation => CheckIfNeedChangePlatform();
        public string RequireSteps => string.Empty;


        public BuildPlatform Platform;
        

        private bool CheckIfNeedChangePlatform()
        {
            if (Platform == BuildPlatform.None)
                return false;

            byte pId = (byte)Platform;
            return pId switch
            {
                var id when id < 10 => !CheckStandaloneTarget(),
                var id when id < 20 => !CheckServerTarget(),
                var id when id < 30 => !CheckMobileTarget(),
                var id when id < 40 => !CheckConsoleTarget(),
                _ => !CheckOtherTarget(),
            };
        }

        private bool CheckStandaloneTarget()
        {
            if (EditorUserBuildSettings.standaloneBuildSubtarget == StandaloneBuildSubtarget.Server)
                return false;

            return Platform switch
            {
                BuildPlatform.Standalone_Windows => EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64,
                BuildPlatform.Standalone_OSX => EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX,
                BuildPlatform.Standalone_Linux => EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux64,
                _ => false,
            };
        }

        private bool CheckServerTarget()
        {
            if (EditorUserBuildSettings.standaloneBuildSubtarget == StandaloneBuildSubtarget.Player)
                return false;

            return Platform switch
            {
                BuildPlatform.Server_Windows => EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64,
                BuildPlatform.Server_Linux => EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux64,
                BuildPlatform.Server_OSX => EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX,
                BuildPlatform.Linux_Headless_Simulation => EditorUserBuildSettings.activeBuildTarget == BuildTarget.LinuxHeadlessSimulation,
                BuildPlatform.Embedded_Linux => EditorUserBuildSettings.activeBuildTarget == BuildTarget.EmbeddedLinux,
                _ => false
            };
        }

        private bool CheckMobileTarget()
        {
            return Platform switch
            {
                BuildPlatform.Android => EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android,
                BuildPlatform.iOS => EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS,
                BuildPlatform.VisionOS => EditorUserBuildSettings.activeBuildTarget == BuildTarget.VisionOS,
                BuildPlatform.TvOS => EditorUserBuildSettings.activeBuildTarget == BuildTarget.tvOS,
                _ => false,
            };
        }

        private bool CheckConsoleTarget()
        {
            return Platform switch
            {
                BuildPlatform.PS4 => EditorUserBuildSettings.activeBuildTarget == BuildTarget.PS4,
                BuildPlatform.PS5 => EditorUserBuildSettings.activeBuildTarget == BuildTarget.PS5,
                BuildPlatform.XboxOne => EditorUserBuildSettings.activeBuildTarget == BuildTarget.XboxOne,
                BuildPlatform.GameCore_XboxSeries => EditorUserBuildSettings.activeBuildTarget == BuildTarget.GameCoreXboxSeries,
                BuildPlatform.GameCore_XboxOne => EditorUserBuildSettings.activeBuildTarget == BuildTarget.GameCoreXboxOne,
                BuildPlatform.Nintendo_Switch => EditorUserBuildSettings.activeBuildTarget == BuildTarget.Switch,
                BuildPlatform.Stadia => EditorUserBuildSettings.activeBuildTarget == BuildTarget.Stadia,
                _ => false
            };
        }

        private bool CheckOtherTarget()
        {
            return Platform switch
            {
                BuildPlatform.WebGL => EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL,
                BuildPlatform.Windows_Store_Apps => EditorUserBuildSettings.activeBuildTarget == BuildTarget.WSAPlayer,
                BuildPlatform.QNX => EditorUserBuildSettings.activeBuildTarget == BuildTarget.QNX,
                _ => false,
            };
        }


        public StepReport ExecuteStep(IWizardRepository repository)
        {
            StepReport report = new()
            {
                StepId = StepName,
                Success = false,
            };

            if (Platform == BuildPlatform.None)
            {
                report.ErrorReport = "The platform can't be None.";
                return report;
            }

            if (SwitchActivePlatform())
                report.Success = true;
            else
                report.ErrorReport = "Error trying to switch platform. Check if you had the requested module correctly installed.";

            return report;
        }

        private bool SwitchActivePlatform()
        {
            return Platform switch
            {
                BuildPlatform.Standalone_Windows => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Standalone, BuildTarget.StandaloneWindows64),
                BuildPlatform.Standalone_Linux => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Standalone, BuildTarget.StandaloneLinux64),
                BuildPlatform.Standalone_OSX => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Standalone, BuildTarget.StandaloneOSX),

                BuildPlatform.Server_Windows => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Server, BuildTarget.StandaloneWindows64),
                BuildPlatform.Server_Linux => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Server, BuildTarget.StandaloneLinux64),
                BuildPlatform.Server_OSX => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Server, BuildTarget.StandaloneOSX),
                BuildPlatform.Linux_Headless_Simulation => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.LinuxHeadlessSimulation, BuildTarget.LinuxHeadlessSimulation),
                BuildPlatform.Embedded_Linux => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.EmbeddedLinux, BuildTarget.EmbeddedLinux),

                BuildPlatform.iOS => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.iOS, BuildTarget.iOS),
                BuildPlatform.Android => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Android, BuildTarget.Android),
                BuildPlatform.VisionOS => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.VisionOS, BuildTarget.VisionOS),
                BuildPlatform.TvOS => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.tvOS, BuildTarget.tvOS),

                BuildPlatform.PS4 => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.PS4, BuildTarget.PS4),
                BuildPlatform.PS5 => EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.PS5, BuildTarget.PS5),
                BuildPlatform.XboxOne => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.XboxOne, BuildTarget.XboxOne),
                BuildPlatform.GameCore_XboxOne => EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.GameCoreXboxOne, BuildTarget.GameCoreXboxOne),
                BuildPlatform.GameCore_XboxSeries => EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.GameCoreXboxSeries, BuildTarget.GameCoreXboxSeries),
                BuildPlatform.Nintendo_Switch => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.NintendoSwitch, BuildTarget.Switch),
                BuildPlatform.Stadia => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Stadia, BuildTarget.Stadia),

                BuildPlatform.WebGL => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.WebGL, BuildTarget.WebGL),
                BuildPlatform.Windows_Store_Apps => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.WindowsStoreApps, BuildTarget.WSAPlayer),
                BuildPlatform.QNX => EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.QNX, BuildTarget.QNX),
                _ => false,
            };
        }
    }
}