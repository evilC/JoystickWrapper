using System;
using System.Collections.Generic;
using System.Threading;

using SharpDX.DirectInput;
using System.Diagnostics;

namespace JWNameSpace
{
    public class JoystickWrapper
    {
        private DirectInput directInput;
        private Dictionary<string, Joystick> acquiredSticks = new Dictionary<string, Joystick>(StringComparer.OrdinalIgnoreCase);
        public bool threadRunning = false;

        public class DeviceReports
        {
            public string InputName { get; set; }
            public int Value { get; set; }
        }

        public class DeviceReport
        {
            public string Guid { get; set; }
            public DeviceReports[] DeviceReports { get; set; }
        }

        public class DeviceInfo
        {
            public DeviceInfo(DeviceInstance deviceInstance)
            {
                Name = deviceInstance.InstanceName;
                Guid = deviceInstance.InstanceGuid.ToString();
            }

            public string Name { get; set; }
            public string Guid { get; set; }
        }

        // =================================================================================
        public JoystickWrapper()
        {
            directInput = new DirectInput();
        }

        public void MonitorStick(dynamic handler, string guid = "")
        {
            var t = new Thread(new ThreadStart(() =>
            {
                Joystick joystick;
                // Avoid race condition.
                // Without this lock, calling MonitorStick twice in rapid succession will interrupt the first call
                lock (typeof(JoystickWrapper))
                {
                    AcquireStick(guid);
                    joystick = acquiredSticks[guid];
                }
                threadRunning = true;
                while (threadRunning)
                {
                    var devReports = new List<DeviceReports>();
                    joystick.Poll();
                    var datas = joystick.GetBufferedData();

                    foreach (var state in datas)
                    {
                        devReports.Add(new DeviceReports() { InputName = state.Offset.ToString(), Value = state.Value });
                    }
                    if (devReports.Count == 0)
                        continue;

                    //Debug.WriteLine(String.Format("AHK| DevReports Length: {0}", devReports.Count));
                    handler.Handle(new DeviceReport() { Guid = guid, DeviceReports = devReports.ToArray() });
                    Thread.Sleep(10);
                }
            }));
            t.Start();
        }

        public void Stop()
        {
            threadRunning = false;
        }

        public DeviceInfo[] GetDevices()
        {
            var devices = directInput.GetDevices();
            List<DeviceInfo> deviceList = new List<DeviceInfo>();
            foreach (var deviceInstance in devices)
            {
                if (deviceInstance.Type != DeviceType.Joystick
                    && deviceInstance.Type != DeviceType.Gamepad
                    && deviceInstance.Type != DeviceType.FirstPerson
                    && deviceInstance.Type != DeviceType.Flight
                    && deviceInstance.Type != DeviceType.Driving)
                    continue;
                deviceList.Add(new DeviceInfo(deviceInstance));
            }
            return deviceList.ToArray();
        }

        public int AcquireStick(string guid)
        {
            var joystick = new Joystick(directInput, new Guid(guid));

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            joystick.Acquire();
            acquiredSticks[guid] = joystick;
            return 1;
        }
    }
}