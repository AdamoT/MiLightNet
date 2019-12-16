using System.Net;

namespace MiLightNet.Utils
{
    public class UdpExchangeResult
    {
        public IPEndPoint RemoteEndPoint { get; set; }
        public byte[] GetData() => _Data;

        private byte[] _Data;

        public UdpExchangeResult(IPEndPoint endPoint, byte[] data)
        {
            RemoteEndPoint = endPoint;
            _Data = data;
        }
    }
}
