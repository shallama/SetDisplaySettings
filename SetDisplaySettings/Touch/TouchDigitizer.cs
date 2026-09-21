using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetDisplaySettings.Touch
{
    internal sealed class TouchDigitizer
    {
        public TouchDigitizer(
            string deviceId,
            string instanceId,
            string deviceDescription,
            string registryPath)
        {
            DeviceId = deviceId;
            InstanceId = instanceId;
            DeviceDescription = deviceDescription;
            RegistryPath = registryPath;
        }

        public string DeviceId { get; }

        public string InstanceId { get; }

        public string DeviceDescription { get; }

        public string RegistryPath { get; }

        public string HidPath => $"{DeviceId}\\{InstanceId}";
    }
}
