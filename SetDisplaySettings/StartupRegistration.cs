using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Reflection;

namespace SetDisplaySettings
{
    internal static class StartupRegistration
    {
        private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "SetDisplaySettings";
        private const string TaskName = "SetDisplaySettings";

        public static void RegisterStartup()
        {
            try
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = baseKey.OpenSubKey(RunKey, writable: true))
                {
                    if (key == null)
                        throw new InvalidOperationException("Unable to open Run registry key.");
                    var exePath = Assembly.GetExecutingAssembly().Location;
                    key.SetValue(
                        AppName,
                        $"\"{exePath}\" applyresolution",
                        RegistryValueKind.String);
                }
                Logger.Info("Startup registration updated: SetDisplaySettings will run at user logon.");
            }
            catch (Exception ex)
            {
                Logger.Error("RegisterStartup failed. " + ex.ToString());
            }
        }

        public static void UnregisterStartup()
        {
            try
            {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = baseKey.OpenSubKey(RunKey, writable: true))
                {
                    if (key == null)
                        throw new InvalidOperationException("Unable to open Run registry key.");

                    key.SetValue(
                        AppName,
                        "\"C:\\Windows\\System32\\displayswitch.exe\" 1",
                        RegistryValueKind.String);
                }
                Logger.Info("Startup registration restored to DisplaySwitch.");
            }
            catch (Exception ex)
            {
                Logger.Error("UnregisterStartup failed. " + ex.ToString());
            }
        }

        public static void CreateScheduledTask()
        {
            Logger.Info("Creating scheduled task.");

            using (var service = new TaskService())
            {
                var task = service.NewTask();

                task.Principal.UserId = "SYSTEM";
                task.Principal.LogonType = TaskLogonType.ServiceAccount;
                task.Principal.RunLevel = TaskRunLevel.Highest;

                task.Triggers.Add(new LogonTrigger());

                task.Actions.Add(
                    new ExecAction(
                        Assembly.GetExecutingAssembly().Location,
                        "refreshtouchmapping"));

                task.Settings.DisallowStartIfOnBatteries = false;
                task.Settings.StopIfGoingOnBatteries = false;
                task.Settings.StartWhenAvailable = true;
                task.Settings.ExecutionTimeLimit = TimeSpan.FromMinutes(5);

                service.RootFolder.RegisterTaskDefinition(
                    TaskName,
                    task,
                    TaskCreation.CreateOrUpdate,
                    "SYSTEM",
                    null,
                    TaskLogonType.ServiceAccount);
            }

            Logger.Info("Scheduled task created.");
        }

        public static void DeleteScheduledTask()
        {
            Logger.Info("Removing scheduled task.");

            using (var service = new TaskService())
            {
                service.RootFolder.DeleteTask(TaskName, false);
            }

            Logger.Info("Scheduled task removed.");
        }
    }
}
