using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace SetDisplaySettings.Display
{
    internal static class DisplayManager
    {
        public static void ApplyResolution()
        {
            var screens = Screen.AllScreens;

            Logger.Info($"Found {screens.Length} display(s).");

            if (screens.Length > 1)
            {
                var result = ExtendDesktop();
                Logger.Info($"SetDisplayConfig returned {result}.");
            }

            foreach (var screen in screens)
            {
                Logger.Info($"Screen {screen.DeviceName} " + $"Primary={screen.Primary} " + $"Bounds={screen.Bounds.Width}x{screen.Bounds.Height}");
            }

            for (var attempt = 0; attempt < 10; attempt++)
            {
                if (SetResolution(AppSettings.DisplayWidth, AppSettings.DisplayHeight))
                {
                    Logger.Info("Customer display(s) configured.");
                    return;
                }

                Thread.Sleep(1000);
            }

            Logger.Error("Failed to configure display resolution.");
        }

        public static void SetSecondaryAsPrimary()
        {
            var screens = Screen.AllScreens;

            if (screens.Length != 2)
            {
                Logger.Info(
                    $"SetSecondaryAsPrimary: Expected 2 displays, found {screens.Length}.");
                return;
            }

            var primary = Screen.AllScreens.FirstOrDefault(s => s.Primary);
            var secondary = Screen.AllScreens.FirstOrDefault(s => !s.Primary);

            foreach (var screen in Screen.AllScreens)
            {
                Logger.Info(
                    $"SetSecondaryAsPrimary: Device={screen.DeviceName}, Primary={screen.Primary}, Bounds={screen.Bounds}");
            }

            if (primary == null || secondary == null)
            {
                Logger.Info("SetSecondaryAsPrimary: Primary or secondary display not found.");
                return;
            }

            var primaryMode = new Display.DEVMODE();
            primaryMode.dmSize = (short)Marshal.SizeOf(typeof(Display.DEVMODE));

            var secondaryMode = new Display.DEVMODE();
            secondaryMode.dmSize = (short)Marshal.SizeOf(typeof(Display.DEVMODE));

            Logger.Info($"SetSecondaryAsPrimary: Primary {primary.DeviceName}: " + $"X={primaryMode.dmPositionX}, Y={primaryMode.dmPositionY}");
            Logger.Info($"SetSecondaryAsPrimary: Secondary {secondary.DeviceName}: " + $"X={secondaryMode.dmPositionX}, Y={secondaryMode.dmPositionY}");

            if (Display.EnumDisplaySettings(
                    primary.DeviceName,
                    Display.ENUM_CURRENT_SETTINGS,
                    ref primaryMode) == 0)
            {
                Logger.Error($"SetSecondaryAsPrimary: EnumDisplaySettings failed for {primary.DeviceName}");
                return;
            }

            if (Display.EnumDisplaySettings(
                    secondary.DeviceName,
                    Display.ENUM_CURRENT_SETTINGS,
                    ref secondaryMode) == 0)
            {
                Logger.Error($"SetSecondaryAsPrimary: EnumDisplaySettings failed for {secondary.DeviceName}");
                return;
            }

            int offsetX = secondaryMode.dmPositionX;
            int offsetY = secondaryMode.dmPositionY;

            primaryMode.dmFields |= Display.DM_POSITION;
            secondaryMode.dmFields |= Display.DM_POSITION;

            secondaryMode.dmPositionX = 0;
            secondaryMode.dmPositionY = 0;

            primaryMode.dmPositionX -= offsetX;
            primaryMode.dmPositionY -= offsetY;

            int result = Display.ChangeDisplaySettingsEx(
                secondary.DeviceName,
                ref secondaryMode,
                IntPtr.Zero,
                Display.CDS_SET_PRIMARY |
                Display.CDS_UPDATEREGISTRY |
                Display.CDS_NORESET,
                IntPtr.Zero);

            Logger.Info($"SetSecondaryAsPrimary: Secondary result = {result}");

            result = Display.ChangeDisplaySettingsEx(
                primary.DeviceName,
                ref primaryMode,
                IntPtr.Zero,
                Display.CDS_UPDATEREGISTRY |
                Display.CDS_NORESET,
                IntPtr.Zero);

            Logger.Info($"SetSecondaryAsPrimary: Primary result = {result}");

            result = Display.ChangeDisplaySettingsEx(
                null,
                IntPtr.Zero,
                IntPtr.Zero,
                0,
                IntPtr.Zero);

            Logger.Info($"SetSecondaryAsPrimary: Apply result = {result}");
        }

        private static int ExtendDesktop()
        {
            return Display.SetDisplayConfig(
                0,
                IntPtr.Zero,
                0,
                IntPtr.Zero,
                Display.SDC_TOPOLOGY_EXTEND |
                Display.SDC_APPLY);
        }

        private static bool SetResolution(int width, int height)
        {
            var success = true;

            foreach (var screen in Screen.AllScreens)
            {
                var mode = new Display.DEVMODE();
                mode.dmSize = (short)Marshal.SizeOf(typeof(Display.DEVMODE));

                if (Display.EnumDisplaySettings(
                        screen.DeviceName,
                        Display.ENUM_CURRENT_SETTINGS,
                        ref mode) == 0)
                {
                    Logger.Error($"EnumDisplaySettings failed for {screen.DeviceName}");
                    success = false;
                    continue;
                }

                if (mode.dmPelsWidth == width &&
                    mode.dmPelsHeight == height)
                {
                    Logger.Info($"{screen.DeviceName} already at {width}x{height}");
                    continue;
                }

                mode.dmPelsWidth = width;
                mode.dmPelsHeight = height;
                mode.dmFields =
                    Display.DM_PELSWIDTH |
                    Display.DM_PELSHEIGHT;

                var result = Display.ChangeDisplaySettingsEx(
                    screen.DeviceName,
                    ref mode,
                    IntPtr.Zero,
                    Display.CDS_UPDATEREGISTRY,
                    IntPtr.Zero);

                Logger.Info($"ChangeDisplaySettingsEx on {screen.DeviceName} to {width}x{height} returned {result}");

                if (result != 0)
                {
                    success = false;
                }
            }

            return success;
        }

        private static bool SetSecondaryResolution(int width, int height, bool logDevices)
        {
            var screens = Screen.AllScreens;

            if (logDevices)
            {
                foreach (var screen in screens)
                {
                    Logger.Info(
                        $"Screen {screen.DeviceName} " +
                        $"Primary={screen.Primary} " +
                        $"Bounds={screen.Bounds.Width}x{screen.Bounds.Height}");
                }
            }

            Screen target = null;

            foreach (var screen in screens)
            {
                if (!screen.Primary)
                {
                    target = screen;
                    break;
                }
            }

            if (target == null)
            {
                return false;
            }

            var mode = new Display.DEVMODE();

            mode.dmSize = (short)Marshal.SizeOf(typeof(Display.DEVMODE));

            if (Display.EnumDisplaySettings(
                    target.DeviceName,
                    Display.ENUM_CURRENT_SETTINGS,
                    ref mode) == 0)
            {
                Logger.Error($"EnumDisplaySettings failed for {target.DeviceName}");

                return false;
            }

            if (mode.dmPelsWidth == width &&
                mode.dmPelsHeight == height)
            {
                Logger.Info(
                    $"{target.DeviceName} already at {width}x{height}");

                return true;
            }

            mode.dmPelsWidth = width;
            mode.dmPelsHeight = height;

            mode.dmFields =
                Display.DM_PELSWIDTH |
                Display.DM_PELSHEIGHT;

            var result =
                Display.ChangeDisplaySettingsEx(
                    target.DeviceName,
                    ref mode,
                    IntPtr.Zero,
                    Display.CDS_UPDATEREGISTRY,
                    IntPtr.Zero);

            Logger.Info($"ChangeDisplaySettingsEx on {target.DeviceName} to {width}x{height} returned {result}");

            return result == 0;
        }

    }
}
