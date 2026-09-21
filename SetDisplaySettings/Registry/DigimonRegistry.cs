using Microsoft.Win32;
using System;
using System.Text.RegularExpressions;
using SetDisplaySettings.Touch;

namespace SetDisplaySettings.Registry
{
    internal static class DigimonRegistry
    {
        private const string RegistryPath = @"SOFTWARE\Microsoft\Wisp\Pen\Digimon";

        public static void WriteMapping(
            TouchDigitizer digitizer,
            string monitorInstanceName)
        {
            try
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var key = baseKey.CreateSubKey(RegistryPath))
                    {
                        if (key == null)
                        {
                            Logger.Error("Failed to open Digimon registry key.");
                            return;
                        }

                        var valueName = BuildValueName(digitizer);
                        var valueData = BuildValueData(monitorInstanceName);
                        
                        key.SetValue(valueName, valueData);

                        Logger.Info($"Mapped '{valueName}' -> '{valueData}'.");
                        Logger.Info($"Registry contains {key.ValueCount} value(s):");
                        foreach (var name in key.GetValueNames())
                        {
                            Logger.Info($" {name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        public static void Remove()
        {
            try
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    baseKey.DeleteSubKeyTree(
                        RegistryPath,
                        false);
                }

                Logger.Info("Digimon registry key removed.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        internal static string BuildValueName(TouchDigitizer digitizer)
        {
            var path = @"HID\" + digitizer.HidPath;

            path = path.Replace('\\', '#');

            return
                @"20-\\?\" +
                path +
                "#{4d1e55b2-f16f-11cf-88cb-001111000030}";
        }

        internal static string BuildValueData(string instanceName)
        {
            return Regex.Replace(
                instanceName.Replace('\\', '#'),
                "(.*)_.*",
                @"\\?\$1#{e6f07b5f-ee97-4a90-b076-33f57bf4eaa7}");
        }
    }
}