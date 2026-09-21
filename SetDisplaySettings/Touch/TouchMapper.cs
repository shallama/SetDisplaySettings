using Microsoft.Win32;
using SetDisplaySettings.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace SetDisplaySettings.Touch
{
    internal static class TouchMapper
    {
        public static void ConfigureTouchMapping()
        {
            try
            {
                var digitizers = GetTouchDigitizers();
                var monitors = GetMonitors();

                Logger.Info($"Found {digitizers.Count} touch digitizer(s) and {monitors.Count} monitor(s).");

                if (digitizers.Count == 0)
                {
                    Logger.Info("No touch digitizers found.");
                    return;
                }

                foreach (var digitizer in digitizers)
                {
                    var monitor = FindMatchingMonitor(
                        digitizer,
                        digitizers.Count,
                        monitors);

                    if (monitor == null)
                    {
                        Logger.Info(
                            $"No matching monitor found for '{digitizer.HidPath}'.");
                        continue;
                    }

                    Logger.Info(
                        $"Mapping '{digitizer.HidPath}' to '{monitor.InstanceName}'.");

                    DigimonRegistry.WriteMapping(
                        digitizer,
                        monitor.InstanceName);
                }
            } 
            catch(Exception ex) 
            { 
                Logger.Error(ex.ToString()); 
            }
        }

        public static void RefreshTouchMapping()
        {

            if (!MappingIsCurrent())
            {
                Logger.Info("Touch mapping is not current. Refreshing touch mapping.");
                DigimonRegistry.Remove();
                ConfigureTouchMapping();
            }
        }

        public static bool MappingIsCurrent()
        {
            try
            {
                var digitizers = GetTouchDigitizers();
                var monitors = GetMonitors();

                using (var baseKey =  RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Wisp\Pen\Digimon"))
                    {
                        if (key == null)
                        {
                            Logger.Info("Digimon key does not exist.");

                            return false;
                        }

                        foreach (var digitizer in digitizers)
                        {
                            var monitor =
                                FindMatchingMonitor(
                                    digitizer,
                                    digitizers.Count,
                                    monitors);

                            if (monitor == null)
                            {
                                Logger.Info($"No monitor found for {digitizer.HidPath}");

                                return false;
                            }

                            var expectedName = DigimonRegistry.BuildValueName(digitizer);

                            var expectedValue = DigimonRegistry.BuildValueData(monitor.InstanceName);

                            var actualValue = key.GetValue(expectedName) as string;

                            if (!string.Equals(
                                    actualValue,
                                    expectedValue,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                Logger.Info(
                                    $"Digimon mapping mismatch. " +
                                    $"Name={expectedName} " +
                                    $"Expected={expectedValue} " +
                                    $"Actual={actualValue}");

                                return false;
                            }
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return false;
            }
        }

        private static List<TouchDigitizer> GetTouchDigitizers()
        {
            var result = new List<TouchDigitizer>();

            using (var hidKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\HID"))
            {
                if (hidKey == null)
                {
                    return result;
                }

                foreach (var deviceId in hidKey.GetSubKeyNames())
                {
                    using (var deviceKey = hidKey.OpenSubKey(deviceId))
                    {
                        if (deviceKey == null)
                            continue;

                        foreach (var instanceId in deviceKey.GetSubKeyNames())
                        {
                            using (var instanceKey = deviceKey.OpenSubKey(instanceId))
                            {
                                if (instanceKey == null)
                                    continue;

                                var description =
                                    instanceKey.GetValue("DeviceDesc") as string;

                                if (string.IsNullOrEmpty(description))
                                    continue;

                                if (description.IndexOf(
                                        "Touch",
                                        StringComparison.OrdinalIgnoreCase) < 0)
                                    continue;

                                var registryPath = @"SYSTEM\CurrentControlSet\Enum\HID\" + deviceId + "\\" + instanceId;

                                result.Add(
                                    new TouchDigitizer(
                                        deviceId,
                                        instanceId,
                                        description,
                                        registryPath));
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static List<MonitorInfo> GetMonitors()
        {
            var result = new List<MonitorInfo>();

            using (var searcher = new ManagementObjectSearcher(
                @"root\wmi",
                "SELECT InstanceName FROM WmiMonitorBasicDisplayParams"))
            {
                foreach (ManagementObject monitor in searcher.Get())
                {
                    var instanceName = monitor["InstanceName"] as string;

                    if (!string.IsNullOrEmpty(instanceName))
                    {
                        result.Add(new MonitorInfo(instanceName));
                    }
                }
            }

            return result;
        }

        private static MonitorInfo FindMatchingMonitor(TouchDigitizer digitizer, int digitizerCount, List<MonitorInfo> monitors)
        {
            foreach (var mapping in AppSettings.DigitizerToMonitor)
            {
                if (!digitizer.DeviceId.Contains(mapping.Key))
                {
                    continue;
                }

                return monitors.FirstOrDefault(
                    m => m.InstanceName.Contains(mapping.Value));
            }

            if (digitizerCount == 1)
            {
                return monitors.FirstOrDefault();
            }

            return null;
        }
    }
}
