using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JWNameSpace;

namespace TestApp
{
    class Test
    {
        static void Main(string[] args)
        {
            var jw = new JoystickWrapper();
            var devs = jw.GetDevices();
            var guid = devs[0].Guid;
            ////jw.ConsolePollStick(guid);
            //while (true)
            //{
            //    jw.PollStick(guid);
            //}
        }
    }
}
