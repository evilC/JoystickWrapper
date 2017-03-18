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

            //jw.SubscribeXboxAxis(1, 1, new Action<int>((value) => { Console.WriteLine("XBox Axis 1 Value: " + value); }), "LV1");
            //jw.SubscribeXboxButton(1, 1, new Action<int>((value) => { Console.WriteLine("XBox Button 1 Value: " + value); }), "LV1");
            jw.SubscribeXboxDpad(1, 1, new Action<int>((value) => { Console.WriteLine("XBox Dpad Up Value: " + value); }), "LV1");
            jw.UnSubscribeXboxDpad(1, 1, "LV1");
            return;

            // You can enumerate available devices like this...
            //var devs = jw.GetDevices();
            //var guidStr = devs[0].Guid;
            //var guidStr = jw.GetDeviceGuidByName("vjoy device"); // Case insensitive get guid from name
            //var guidStr = jw.GetDeviceGuidByName("Controller (XBOX 360 For Windows)"); // Case insensitive get guid from name

            // Or just pick any device like this...
            var guidStr = jw.GetAnyDeviceGuid();

            // Or hard-code a guid if you wish to test with a specific stick
            //var guidStr = "da2e2e00-19ea-11e6-8002-444553540000";  // evilC vJoy #1
            //var guidStr = "83f38eb0-7433-11e6-8007-444553540000";  // evilC vJoy #1w

            // Demo - three subscriptions requested
            // Subscription #1 - Axis
            jw.SubscribeAxis(guidStr, 1, new Action<int>((value) => { Console.WriteLine("Axis 1 Value: " + value); }), "LV1");

            // Subscription #2 - Button
            jw.SubscribeButton(guidStr, 1, new Action<int>((value) => { Console.WriteLine("Button 1 Value: " + value); }), "LV1");

            // Subscription #3 - POV
            jw.SubscribePov(guidStr, 1, new Action<int>((value) => { Console.WriteLine("POV 1 Value: " + value); }), "LV1");

            // Subscription #3 - POV *Direction* (Up)
            jw.SubscribePovDirection(guidStr, 1, 1, new Action<int>((value) => { Console.WriteLine("POV 1, Direction Up Value: " + value); }), "LV1");
        }
    }
}
