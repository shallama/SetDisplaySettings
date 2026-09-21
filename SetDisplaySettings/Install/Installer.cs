using SetDisplaySettings.Display;
using SetDisplaySettings.Touch;

namespace SetDisplaySettings.Install
{
    internal static class Installer
    {
        public static void Install()
        {
            if (AppSettings.TouchMappingEnabled)
            {
                TouchMapper.ConfigureTouchMapping();
            }

            DisplayManager.ApplyResolution();

            if (AppSettings.ApplyResolutionAtStartup)
            {
                StartupRegistration.RegisterStartup();
            }

            if (AppSettings.TouchMappingEnabled &&
                AppSettings.RefreshTouchMappingAtStartup)
            {
                StartupRegistration.CreateScheduledTask();
            }


            Logger.Info("Installation completed.");
        }
    }
}
