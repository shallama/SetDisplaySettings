using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetDisplaySettings.Touch
{
    internal sealed class MonitorInfo
    {
        public MonitorInfo(string instanceName)
        {
            InstanceName = instanceName;
        }

        public string InstanceName { get; }
    }
}
