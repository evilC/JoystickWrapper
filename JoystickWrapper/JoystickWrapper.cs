using System;
using System.Collections.Generic;
using System.Threading;

using SharpDX.DirectInput;
using System.Diagnostics;

namespace JWNameSpace
{
    public class JoystickWrapper
    {
        static private DirectInput directInput;
        public bool threadRunning = false;

        // A list of stick subscriptions, indexed by Guid
        public Dictionary<Guid, StickSubscriptions> subscribedSticks = new Dictionary<Guid, StickSubscriptions>();

        // Handles the subscriptions for a specific stick
        // Multiple subscriptions to the same input are possible
        // ToDo - Give each subscription a UID, so it can be removed.
        public class StickSubscriptions
        {
            public Joystick joystick { get; set; }
            //The SharpDX.DirectInput.JoystickOffset property from a joystick report identifies the button or axis that the data came from
            public Dictionary<SharpDX.DirectInput.JoystickOffset, List<dynamic>> subscriptions { get; set; }

            public StickSubscriptions(Guid guid)
            {
                subscriptions = new Dictionary<JoystickOffset, List<dynamic>>();
                joystick = new Joystick(directInput, guid);
                // Set BufferSize in order to use buffered data.
                joystick.Properties.BufferSize = 128;

                joystick.Acquire();
            }

            public void Add(string axisStr, dynamic handler)
            {
                var axis = joystickAxes[axisStr];
                if (!subscriptions.ContainsKey(axis))
                {
                    subscriptions[axis] = new List<dynamic>();
                }
                subscriptions[axis].Add(handler);
            }
        }

        // Lookup table from string to Axis Identifier
        public static Dictionary<string, SharpDX.DirectInput.JoystickOffset> joystickAxes = new Dictionary<string, JoystickOffset>(StringComparer.OrdinalIgnoreCase)
        {
            { "X", SharpDX.DirectInput.JoystickOffset.X},
            { "Y", SharpDX.DirectInput.JoystickOffset.Y},
            { "Z", SharpDX.DirectInput.JoystickOffset.Z},
            { "Rx", SharpDX.DirectInput.JoystickOffset.RotationX},
            { "Ry", SharpDX.DirectInput.JoystickOffset.RotationY},
            { "Rz", SharpDX.DirectInput.JoystickOffset.RotationZ},
            { "S0", SharpDX.DirectInput.JoystickOffset.Sliders0},
            { "S1", SharpDX.DirectInput.JoystickOffset.Sliders1}
        };

        // Reply for GetDevices()
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
            MonitorSticks();
        }

        /// <summary>
        /// Adds a Subscription to an axis
        /// </summary>
        /// <param name="guidStr">Guid of the stick to subscribe to</param>
        /// <param name="axisStr">Name of the axis to subscribe to</param>
        /// <param name="handler">Callback to fire when axis changes</param>
        public void SubscribeAxis(string guidStr, string axisStr, dynamic handler)
        {
            var guid = new Guid(guidStr);
            if (!subscribedSticks.ContainsKey(guid))
            {
                subscribedSticks[guid] = new StickSubscriptions(guid);
            }
            var joystick = subscribedSticks[guid].joystick;

            subscribedSticks[guid].Add(axisStr, handler);
        }

        // Monitor thread.
        // Watches any axes that have been subscribed to
        // Examines incoming events for that stick to see if any match subscribed inputs
        // If any are found, fires the callback associated with the subscription
        public void MonitorSticks()
        {
            var t = new Thread(new ThreadStart(() =>
            {
                threadRunning = true;
                while (threadRunning)
                {
                    foreach (var subscribedStick in subscribedSticks)
                    {
                        var guid = subscribedStick.Key;
                        var joystick = subscribedStick.Value.joystick;
                        var subscriptions = subscribedStick.Value.subscriptions;
                        if (subscriptions.Count > 0)
                        {
                            joystick.Poll();
                            var data = joystick.GetBufferedData();
                            foreach (var state in data)
                            {
                                if (subscriptions.ContainsKey(state.Offset))
                                {
                                    foreach (var subscribedInput in subscriptions[state.Offset])
                                    {
                                        subscribedInput.Handle(state.Value);
                                    }
                                }
                            }
                        }
                    }
                    Thread.Sleep(1);
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
    }
}