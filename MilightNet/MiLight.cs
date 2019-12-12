using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Net.NetworkInformation;
using MiLightNet.Utils;
using MiLightNet.Controllers.V6;

namespace MiLightNet
{
    public class MiLight
    {
        #region Private Fields

        private const int DefaultV6DiscoveryPort = 48899;

        #endregion Private Fields

        #region Public Methods

        public Task DiscoverControllers(ICollection<IMiLightController> controllers, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                DiscoverControllersV6(controllers, DefaultV6DiscoveryPort, cancellationToken);
            }, cancellationToken);
        }

        #endregion Public Methods

        #region Private Methods

        private void DiscoverControllersV6(ICollection<IMiLightController> controllers, int portNumber, CancellationToken cancellationToken)
        {
            var receiveTasks = new Task[1];
            using (var udp = new System.Net.Sockets.UdpClient())
            {
                udp.Client.ReceiveTimeout = 1000;
                udp.Client.SendTimeout = 1000;
                udp.EnableBroadcast = true;
                var endPoint = new IPEndPoint(IPAddress.Broadcast, portNumber);

                var data = System.Text.Encoding.UTF8.GetBytes("HF-A11ASSISTHREAD");                    

                do
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        UdpUtils.Flush(udp);
                        udp.Send(data, data.Length, endPoint);
                            
                        var receiveTask = udp.ReceiveAsync();
                        receiveTasks[0] = receiveTask;
                        Task.WaitAny(receiveTasks, cancellationToken);

                        var result = receiveTask.Result;

                        //check if this controller is already in the list
                        if (!controllers.Any(x => x.EndPoint.Address.Equals(result.RemoteEndPoint.Address)))
                        {//That's a new controller
                            var identifiers = System.Text.Encoding.ASCII.GetString(result.Buffer)
                                .Split(',');
                            if(identifiers.Length < 3)
                            {//1. IP, 2. Mac, 3. Wifi card identifier
                                continue;
                            }

                            var mac = PhysicalAddress.Parse(identifiers[1]);
                            var controller = new MiLightControllerV6(new IPEndPoint(result.RemoteEndPoint.Address, MiLightControllerV6.DefaultV6Port), mac);
                            controllers.Add(controller);
                        }
                    }
                    catch(Exception e)
                    {
                        if(e is OperationCanceledException)
                            break;//Discovery cancelled
                        else if(e is System.Net.Sockets.SocketException socketException)
                        {
                            if (socketException.SocketErrorCode == System.Net.Sockets.SocketError.TimedOut)
                            {//Read timeout - continue
                                continue;
                            }
                            else throw;
                        }
                    }
                } 
                while (true);
            }
        }

        #endregion Private Methods
    }

    
}
