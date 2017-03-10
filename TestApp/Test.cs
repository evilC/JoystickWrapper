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
            var guid = devs[0].Guid;
            //var guid = "da2e2e00-19ea-11e6-8002-444553540000";  // Hard-code a specific stick here if you wish

            // Demo - three subscriptions requested

            // Subscription #1 - Axis 1 (X)
            dynamic axisHandler = new ExpandoObject();
            axisHandler.Handle = new Action<int>((value) =>
            {
                Console.WriteLine("X Value: " + value);
            });
            jw.Subscribe(guid, JoystickWrapper.InputType.AXIS, 1, axisHandler);

            // Subscription #2 - Button 128
            dynamic buttonHandler = new ExpandoObject();
            buttonHandler.Handle = new Action<int>((value) =>
            {
                Console.WriteLine("Button 128 Value: " + value);
            });

            jw.Subscribe(guid, JoystickWrapper.InputType.BUTTON, 128, buttonHandler);

            // Subscription #3 - POV 4
            dynamic povHandler = new ExpandoObject();
            povHandler.Handle = new Action<int>((value) =>
            {
                Console.WriteLine("POV 4 Value: " + value);
            });

            jw.Subscribe(guid, JoystickWrapper.InputType.POV, 4, povHandler);

        }
    }
}
