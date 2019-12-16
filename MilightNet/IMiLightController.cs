using System;
using System.Net;
using System.Net.NetworkInformation;

namespace MiLightNet
{
    public interface IMiLightController : IDisposable
    {
        IPEndPoint EndPoint { get; }
        PhysicalAddress Mac { get; }
    }
}
