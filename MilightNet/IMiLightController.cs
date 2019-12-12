using System.Net;
using System.Net.NetworkInformation;

namespace MiLightNet
{
    public interface IMiLightController
    {
        IPEndPoint EndPoint { get; }
        PhysicalAddress Mac { get; }
    }
}
