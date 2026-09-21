using SetDisplaySettings.Display;
using SetDisplaySettings.Install;
using SetDisplaySettings.Touch;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;

namespace SetDisplaySettings
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            string argsErr = "Invalid command.\nUsage:\nSetDisplaySettings.exe Install\nSetDisplaySettings.exe Uninstall\nSetDisplaySettings.exe ApplyResolution\nSetDisplaySettings.exe SetPrimaryDisplay\nSetDisplaySettings.exe RefreshTouchMapping\n\n";
            
            if (args.Length < 1)
            {
                Logger.Error(argsErr);
                return 1;
            }

            LoadSettings();

            Thread.Sleep(TimeSpan.FromSeconds(10));
            
            var commands = args.Select(a => a.ToLowerInvariant()).ToHashSet();

            if (commands.Contains("install") &&
                commands.Contains("uninstall"))
            {
                Logger.Error("Install and Uninstall are mutually exclusive.");

                return 1;
            }

            if (commands.Contains("install"))
            {
                Installer.Install();
            }

            if (commands.Contains("setprimarydisplay"))
            {
                DisplayManager.SetSecondaryAsPrimary();
            }

            if (commands.Contains("applyresolution"))
            {
                DisplayManager.ApplyResolution();
            }

            if (commands.Contains("refreshtouchmapping"))
            {
                TouchMapper.RefreshTouchMapping();
            }

            if (commands.Contains("uninstall"))
            {
                Uninstaller.Uninstall();
            }

            return 0;
        }

        private static void LoadSettings()
        {
            if (int.TryParse(ConfigurationManager.AppSettings["Display.Width"], out var width))
                AppSettings.DisplayWidth = width;

            if (int.TryParse(ConfigurationManager.AppSettings["Display.Height"], out var height))
                AppSettings.DisplayHeight = height;

            if (bool.TryParse(ConfigurationManager.AppSettings["TouchMapping.Enabled"], out var touchMappingEnabled))
                AppSettings.TouchMappingEnabled = touchMappingEnabled;

            if (bool.TryParse(ConfigurationManager.AppSettings["Startup.ApplyResolution"], out var applyResolutionAtStartup))
            {
                AppSettings.ApplyResolutionAtStartup = applyResolutionAtStartup;
            }

            if (bool.TryParse(ConfigurationManager.AppSettings["Startup.RefreshTouchMapping"], out var refreshTouchMappingAtStartup))
            {
                AppSettings.RefreshTouchMappingAtStartup = refreshTouchMappingAtStartup;
            }

            var digitizerToMonitor = new Dictionary<string, string>();

            const string prefix = "DigitizerToMonitor.";

            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                var digitizer = key.Substring(prefix.Length);
                var monitor = ConfigurationManager.AppSettings[key];

                if (!string.IsNullOrWhiteSpace(monitor))
                    digitizerToMonitor[digitizer] = monitor;
            }

            AppSettings.DigitizerToMonitor = digitizerToMonitor;
        }
    }
}
