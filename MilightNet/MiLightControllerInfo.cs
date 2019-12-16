using System.Net;
using System.Net.NetworkInformation;

namespace MiLightNet
{
    public class MiLightControllerInfo
    {
        public IPAddress IP { get; }
        public int Port { get; }
        public PhysicalAddress MAC { get; }
        public MiLightControllerVersion Version { get; }
        public MiLightControllerType Type { get; }

        public MiLightControllerInfo(IPAddress ip, int port, PhysicalAddress mac, MiLightControllerVersion version, MiLightControllerType type)
        {
            IP = ip;
            Port = port;
            MAC = mac;
            Version = version;
            Type = type;
        }
    }
}
