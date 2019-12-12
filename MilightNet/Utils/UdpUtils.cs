using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MiLightNet.Utils
{
    public static class UdpUtils
    {
        #region Public Methods

        public static void Flush(UdpClient udp)
        {
            IPEndPoint endPoint = null;
            while(udp.Available > 0)
            {
                udp.Receive(ref endPoint);
            }
        }

        public static Task<UdpExchangeResult> ExchangeDatagram(UdpClient udp, byte[] data, TimeSpan timeout)
        {
            return Task.Run(() =>
            {
                udp.Client.ReceiveTimeout = udp.Client.SendTimeout = (int)timeout.TotalMilliseconds;
                var bytesSent = udp.Send(data, data.Length);
                if (bytesSent != data.Length)
                    throw new System.IO.IOException("Failed to send all data");

                //Receive
                IPEndPoint remoteEndPoint = null;
                var response = udp.Receive(ref remoteEndPoint);
                if (response == null)
                    throw new System.IO.IOException("Failed to read response");

                return new UdpExchangeResult
                {
                    RemoteEndPoint = remoteEndPoint,
                    Data = response,
                };
            });
        }

        public static Task<UdpExchangeResult> ExchangeDatagram(UdpClient udp, byte[] data, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                udp.Client.SendTimeout = 100;//Arbitrary value
                udp.Client.ReceiveTimeout = 100;//Arbitrary value

                var bytesSent = udp.Send(data, data.Length);
                if (bytesSent != data.Length)
                    throw new System.IO.IOException("Failed to send all data");

                //Receive
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        IPEndPoint remoteEndPoint = null;
                        var response = udp.Receive(ref remoteEndPoint);
                        if (response == null)
                            throw new System.IO.IOException("Failed to read response");

                        return new UdpExchangeResult
                        {
                            RemoteEndPoint = remoteEndPoint,
                            Data = response,
                        };
                    }
                    catch(SocketException socketException)
                    {
                        if (socketException.SocketErrorCode == SocketError.TimedOut)
                            continue;
                        else throw;
                    }
                }
            }, cancellationToken);
        }

        #endregion Public Methods

        #region Types

        public struct UdpExchangeResult
        {
            public IPEndPoint RemoteEndPoint;
            public byte[] Data;
        }

        #endregion Types
    }
}
