using System.Collections.Generic;
using static SetDisplaySettings.Program;

namespace SetDisplaySettings
{
    internal static class AppSettings
    {
        public static int DisplayWidth = 800;

        public static int DisplayHeight = 600;

        public static bool TouchMappingEnabled = true;

        public static bool ApplyResolutionAtStartup = true;

        public static bool RefreshTouchMappingAtStartup = true;

        public const string LogFileName = "SetDisplaySettings.exe.log";

        public static IReadOnlyDictionary<string, string> DigitizerToMonitor;
    }
    internal enum SettingType
    {
        TouchMapper,
        DisplaySettngs
    }
    internal enum StartupMode
    {
        Run,
        TaskScheduler
    }
}
