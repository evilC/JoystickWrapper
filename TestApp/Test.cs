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

            // Demo - three subscriptions requested

            // Subscription #1 - Axis 1 (X)
            dynamic axisHandler = new ExpandoObject();
            axisHandler.Handle = new Action<int>((value) =>
            {
                Console.WriteLine("First Axis Value: " + value);
            });
            jw.SubscribeAxis(dev.Guid, 1, axisHandler);
            //jw.SubscribeDev(devs[0].Axis.Last(), axisHandler);

            // Subscription #2 - Button 128
            dynamic buttonHandler = new ExpandoObject();
            buttonHandler.Handle = new Action<int>((value) =>
            {
                Console.WriteLine("First Button Value: " + value);
            });

            jw.SubscribeButton(dev.Guid, 1, buttonHandler);

            // Subscription #3 - POV 4
            dynamic povHandler = new ExpandoObject();
            povHandler.Handle = new Action<int>((value) =>
            {
                Console.WriteLine("First POV Value: " + value);
            });

            jw.SubscribePov(dev.Guid, 1, povHandler);

        }
    }
}
