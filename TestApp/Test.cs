using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JWNameSpace;

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
            jw.AcquireStick(guid);
            // Dunno at this point how to implement a handler in C#
            // Only know how to do it in AHK...
            //jw.MonitorStick(handler, guid);
        }
    }
}
