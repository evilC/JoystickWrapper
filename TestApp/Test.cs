using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JWNameSpace;
using System.Dynamic;

// App to test JoystickWrapper's functionality just using C#, so you can debug in VS etc
namespace TestApp
{
    class Test
    {
        static void Main(string[] args)
        {
            var jw = new JoystickWrapper();

            var devs = jw.GetDevices();
            var dev = devs[0];

            //var guid = devs[0].Guid;
            //var guid = "da2e2e00-19ea-11e6-8002-444553540000";  // Hard-code a specific stick here if you wish

            // Anonymous funcs to monitor callbacks
            dynamic axisHandler = new Action<int>((value) =>
            {
                Console.WriteLine("Axis Value: " + value);
            });
            dynamic buttonHandler = new Action<int>((value) =>
            {
                Console.WriteLine("Button Value: " + value);
            });
            dynamic povHandler = new Action<int>((value) =>
            {
                Console.WriteLine("POV Value: " + value);
            });

            jw.SubscribeAxis(dev.Guid, 1, axisHandler, "LV1");
            jw.SubscribeAxis(dev.Guid, 1, axisHandler, "LV1");
            //jw.UnSubscribeAxis(dev.Guid, 1, "LV1");

            //// Demo - three subscriptions requested
            //// Subscription #1 - Axis 1 (X)
            //jw.SubscribeAxis(dev.Guid, 1, axisHandler, "LV1");

            //// Subscription #2 - Button 128
            //jw.SubscribeButton(dev.Guid, 1, buttonHandler);

            //// Subscription #3 - POV 4
            //jw.SubscribePov(dev.Guid, 1, povHandler);
        }
    }
}
