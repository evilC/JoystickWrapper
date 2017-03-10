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

            // Demo - two subscriptions requested for two axes
            dynamic xHandler = new ExpandoObject();
            xHandler.Handle = new Action<int>((value) =>
            {
                Console.WriteLine("X Value: " + value);
            });

            jw.SubscribeAxis(guid, "x", xHandler);

            dynamic yHandler = new ExpandoObject();
            yHandler.Handle = new Action<int>((value) =>
            {
                Console.WriteLine("Y Value: " + value);
            });

            jw.SubscribeAxis(guid, "y", yHandler);

        }
    }
}
