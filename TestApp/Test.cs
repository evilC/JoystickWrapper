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
            jw.AcquireStick(guid);
            dynamic obj = new ExpandoObject();
            obj.Handle = new Action<JoystickWrapper.DeviceReport>((report) => {
                Console.WriteLine(String.Format("AHK| Device: {0}, Axis: {1}, Value: {2}", report.Guid, report.DeviceReports[0].InputName, report.DeviceReports[0].Value));
            });
            jw.MonitorStick(obj, guid);
        }
    }
}
