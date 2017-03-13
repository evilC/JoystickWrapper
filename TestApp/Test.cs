using System;
using System.Diagnostics;

using JWNameSpace;
using System.Threading;
//using SharpDX.DirectInput;

// App to test JoystickWrapper's functionality just using C#, so you can debug in VS etc
namespace TestApp
{
    class Test
    {
        static void Main(string[] args)
        {
            Debug.WriteLine("DBGVIEWCLEAR");
            var jw = new JoystickWrapper();

            //var devs = jw.GetDevices();
            //var dev = devs[0];
            //var guid = devs[0].Guid;
            var guidStr = jw.GetAnyDeviceGuid();

            //var guidStr = "da2e2e00-19ea-11e6-8002-444553540000";  // Hard-code a specific stick here if you wish
            //var guidStr = "83f38eb0-7433-11e6-8007-444553540000";  // Hard-code a specific stick here if you wish

            //// Anonymous funcs to monitor callbacks
            dynamic axisHandler = new Action<int>((value) =>
            {
                Console.WriteLine("Axis1 Value: " + value);
            });

            dynamic buttonHandler = new Action<int>((value) =>
            {
                Console.WriteLine("Button Value: " + value);
            });
            dynamic povHandler = new Action<int>((value) =>
            {
                Console.WriteLine("POV Value: " + value);
            });

            // Demo - three subscriptions requested
            // Subscription #1 - Axis 1 (X)
            jw.SubscribeAxis(guidStr, 1, axisHandler, "LV1");

            // Subscription #2 - Button 128
            jw.SubscribeButton(guidStr, 1, buttonHandler, "LV1");

            // Subscription #3 - POV 4
            jw.SubscribePov(guidStr, 1, povHandler, "LV1");
        }
    }
}
