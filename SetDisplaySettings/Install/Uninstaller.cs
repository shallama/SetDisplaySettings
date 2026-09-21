using SetDisplaySettings.Registry;

namespace SetDisplaySettings.Install
{
    internal static class Uninstaller
    {
        public static void Uninstall()
        {
            Logger.Info("Starting uninstall.");

            StartupRegistration.UnregisterStartup();

            StartupRegistration.DeleteScheduledTask();

            DigimonRegistry.Remove();

            Logger.Info("Uninstall completed.");
        }
    }
}
