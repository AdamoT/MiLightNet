using MiLightNet.Controllers;
using MiLightNet.Controllers.V6;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiLightNet_TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TestV6().Wait();
            Console.WriteLine("Test finished");
            Console.ReadKey();
        }

        private static async Task TestV6()
        {
            var milight = new MiLightNet.MiLight();
            var controllers = new List<MiLightNet.IMiLightController>();
            var discoveryCancellation = new CancellationTokenSource(5000);
            var discoveryTask = milight.DiscoverControllers(controllers, discoveryCancellation.Token);

            //Wait for timeout or until at least one controller is found
            while (!discoveryTask.IsCompleted)
            {
                if (controllers.Count > 0)
                {
                    discoveryCancellation.Cancel();
                    break;
                }
                else await Task.Delay(100);
            }
            await discoveryTask;

            for(int i = 0; i < controllers.Count; ++i)
            {
                var controller = controllers[i];
                Console.WriteLine(controller.EndPoint);
            }

            if(controllers.Count > 0)
            {
                var controller = controllers[0];
                var v6 = controller as MiLightControllerV6;
                var zone = MiLightZones.One;
                var waitInterval = TimeSpan.FromMilliseconds(1000);

                var actions = new Dictionary<string, Func<Task>>();
                actions.Add("Turning light on", () => v6.SetOnOff(zone, true));
                actions.Add("Setting brightness to 10%", () => v6.SetBrightness(zone, 10));
                actions.Add("Setting brightness to 50%", () => v6.SetBrightness(zone, 50));
                actions.Add("Setting hue to 0", () => v6.SetHue(zone, 0));
                actions.Add("Turning on white", () => v6.SetWhite(zone));
                if (zone != MiLightZones.Bridge)
                {
                    actions.Add("Turning on night mode", () => v6.SetNightMode(zone));//Getting out of night mode doesn't work with changing hue? Requires setting white or turning on/off
                    actions.Add("Turning on white again", () => v6.SetWhite(zone));
                }
                actions.Add("Setting hue to 128", () => v6.SetHue(zone, 128));
                actions.Add("Chaning mode to 0", () => v6.SetDynamicMode(zone, MiLightV6DynamicModes.SevenColorGradualChange));
                actions.Add("Enyoing dynamic change", () => Task.Delay(2000));
                actions.Add("Turning light off", () => v6.SetOnOff(zone, false));

                foreach(var action in actions)
                {
                    Console.WriteLine(action.Key);
                    await action.Value();
                    await Task.Delay(waitInterval);
                }
            }
        }
    }
}
