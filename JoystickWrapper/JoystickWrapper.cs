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
            public Dictionary<JoystickOffset, Dictionary<string, dynamic>> subscriptions = new Dictionary<JoystickOffset, Dictionary<string, dynamic>>();

            public StickSubscriptions(Guid guid)
            {
                joystick = new Joystick(directInput, guid);
                var caps = joystick.Capabilities;

                // Build subscription arrays according to capabilities
                for (var i = 1; i <= caps.AxeCount; i++)
                {
                    subscriptions.Add(inputMappings[InputType.AXIS][i - 1], new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase));
                }

                for (var i = 1; i <= caps.ButtonCount; i++)
                {
                    subscriptions.Add(inputMappings[InputType.BUTTON][i - 1], new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase));
                }

                // Set BufferSize in order to use buffered data.
                joystick.Properties.BufferSize = 128;

                joystick.Acquire();
            }

            public bool Add(int index, InputType inputType, dynamic handler, string id = "0")
            {
                var input = inputMappings[inputType][index-1];
                if (!subscriptions.ContainsKey(input))
                {
                    subscriptions[input] = new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
                }
                if (subscriptions[input].ContainsKey(id))
                    subscriptions[input][id] = handler;
                else
                    subscriptions[input].Add(id, handler);
                return true;
            }

            public bool Remove(int index, InputType inputType, string id = "0")
            {
                var input = inputMappings[inputType][index - 1];
                if (subscriptions.ContainsKey(input))
                {
                    subscriptions[input].Remove(id);
                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.Value.Count != 0)
                            return true;
                    }
                    subscriptions.Clear();
                    return true;
                }
                return false;
            }
        }

        // Maps Axis / Button / POV numbers to Offset identidfiers
        public static Dictionary<InputType, List<JoystickOffset>> inputMappings = new Dictionary<InputType, List<JoystickOffset>>(){
            {
                InputType.AXIS, new List<JoystickOffset>()
                {
                    JoystickOffset.X,
                    JoystickOffset.Y,
                    JoystickOffset.Z,
                    JoystickOffset.RotationX,
                    JoystickOffset.RotationY,
                    JoystickOffset.RotationZ,
                    JoystickOffset.Sliders0,
                    JoystickOffset.Sliders1
                }
            },
            {
                InputType.BUTTON, new List<JoystickOffset>()
                {
                    JoystickOffset.Buttons0, JoystickOffset.Buttons1, JoystickOffset.Buttons2, JoystickOffset.Buttons3, JoystickOffset.Buttons4, 
                    JoystickOffset.Buttons5, JoystickOffset.Buttons6, JoystickOffset.Buttons7, JoystickOffset.Buttons8, JoystickOffset.Buttons9, JoystickOffset.Buttons10, 
                    JoystickOffset.Buttons11, JoystickOffset.Buttons12, JoystickOffset.Buttons13, JoystickOffset.Buttons14, JoystickOffset.Buttons15, JoystickOffset.Buttons16, 
                    JoystickOffset.Buttons17, JoystickOffset.Buttons18, JoystickOffset.Buttons19, JoystickOffset.Buttons20, JoystickOffset.Buttons21, JoystickOffset.Buttons22, 
                    JoystickOffset.Buttons23, JoystickOffset.Buttons24, JoystickOffset.Buttons25, JoystickOffset.Buttons26, JoystickOffset.Buttons27, JoystickOffset.Buttons28, 
                    JoystickOffset.Buttons29, JoystickOffset.Buttons30, JoystickOffset.Buttons31, JoystickOffset.Buttons32, JoystickOffset.Buttons33, JoystickOffset.Buttons34, 
                    JoystickOffset.Buttons35, JoystickOffset.Buttons36, JoystickOffset.Buttons37, JoystickOffset.Buttons38, JoystickOffset.Buttons39, JoystickOffset.Buttons40, 
                    JoystickOffset.Buttons41, JoystickOffset.Buttons42, JoystickOffset.Buttons43, JoystickOffset.Buttons44, JoystickOffset.Buttons45, JoystickOffset.Buttons46, 
                    JoystickOffset.Buttons47, JoystickOffset.Buttons48, JoystickOffset.Buttons49, JoystickOffset.Buttons50, JoystickOffset.Buttons51, JoystickOffset.Buttons52, 
                    JoystickOffset.Buttons53, JoystickOffset.Buttons54, JoystickOffset.Buttons55, JoystickOffset.Buttons56, JoystickOffset.Buttons57, JoystickOffset.Buttons58, 
                    JoystickOffset.Buttons59, JoystickOffset.Buttons60, JoystickOffset.Buttons61, JoystickOffset.Buttons62, JoystickOffset.Buttons63, JoystickOffset.Buttons64, 
                    JoystickOffset.Buttons65, JoystickOffset.Buttons66, JoystickOffset.Buttons67, JoystickOffset.Buttons68, JoystickOffset.Buttons69, JoystickOffset.Buttons70, 
                    JoystickOffset.Buttons71, JoystickOffset.Buttons72, JoystickOffset.Buttons73, JoystickOffset.Buttons74, JoystickOffset.Buttons75, JoystickOffset.Buttons76, 
                    JoystickOffset.Buttons77, JoystickOffset.Buttons78, JoystickOffset.Buttons79, JoystickOffset.Buttons80, JoystickOffset.Buttons81, JoystickOffset.Buttons82, 
                    JoystickOffset.Buttons83, JoystickOffset.Buttons84, JoystickOffset.Buttons85, JoystickOffset.Buttons86, JoystickOffset.Buttons87, JoystickOffset.Buttons88, 
                    JoystickOffset.Buttons89, JoystickOffset.Buttons90, JoystickOffset.Buttons91, JoystickOffset.Buttons92, JoystickOffset.Buttons93, JoystickOffset.Buttons94, 
                    JoystickOffset.Buttons95, JoystickOffset.Buttons96, JoystickOffset.Buttons97, JoystickOffset.Buttons98, JoystickOffset.Buttons99, JoystickOffset.Buttons100, 
                    JoystickOffset.Buttons101, JoystickOffset.Buttons102, JoystickOffset.Buttons103, JoystickOffset.Buttons104, JoystickOffset.Buttons105, JoystickOffset.Buttons106, 
                    JoystickOffset.Buttons107, JoystickOffset.Buttons108, JoystickOffset.Buttons109, JoystickOffset.Buttons110, JoystickOffset.Buttons111, JoystickOffset.Buttons112, 
                    JoystickOffset.Buttons113, JoystickOffset.Buttons114, JoystickOffset.Buttons115, JoystickOffset.Buttons116, JoystickOffset.Buttons117, JoystickOffset.Buttons118, 
                    JoystickOffset.Buttons119, JoystickOffset.Buttons120, JoystickOffset.Buttons121, JoystickOffset.Buttons122, JoystickOffset.Buttons123, JoystickOffset.Buttons124, 
                    JoystickOffset.Buttons125, JoystickOffset.Buttons126, JoystickOffset.Buttons127
                }
            },
            {
                InputType.POV, new List<JoystickOffset>()
                {
                    JoystickOffset.PointOfViewControllers0,
                    JoystickOffset.PointOfViewControllers1,
                    JoystickOffset.PointOfViewControllers2,
                    JoystickOffset.PointOfViewControllers3
                }

            }
        };

        // For some reason the AHK code cannot access this Enum
        public enum InputType {AXIS, BUTTON, POV };

        // Reply for GetDevices()
        public class DeviceInfo
        {
            public int Axes { get; set; }
            public int Buttons { get; set; }
            public int POVs { get; set; }

            public DeviceInfo(DeviceInstance deviceInstance)
            {
                var joystick = new Joystick(directInput, deviceInstance.InstanceGuid);
                Axes = joystick.Capabilities.AxeCount;
                Buttons = joystick.Capabilities.ButtonCount;
                POVs = joystick.Capabilities.PovCount;

                Name = deviceInstance.InstanceName;
                Guid = deviceInstance.InstanceGuid.ToString();
            }

            public string Name { get; set; }
            public string Guid { get; set; }
        }

        // ======================================= METHODS ==========================================

        public JoystickWrapper()
        {
            directInput = new DirectInput();
        }

        // ============================ Endpoints ========================================

        /// <summary>
        /// Adds a Subscription to an Axis
        /// </summary>
        /// <param name="guidStr">Guid of the stick to subscribe to</param>
        /// <param name="index">Number of the axis to subscribe to</param>
        /// <param name="handler">Callback to fire when input changes</param>
        /// <param name="id">ID of the subscriber. Can be left blank if you do not wish to allow multiple subscriptions to the same input</param>
        public bool SubscribeAxis(string guidStr, int index, dynamic handler, string id = "0")
        {
            return Subscribe(guidStr, InputType.AXIS, index, handler, id);
        }

        public bool UnSubscribeAxis(string guidStr, int index, string id = "0")
        {
            return UnSubscribe(guidStr, InputType.AXIS, index, id);
        }

        /// <summary>
        /// Adds a Subscription to a Button
        /// </summary>
        /// <param name="guidStr">Guid of the stick to subscribe to</param>
        /// <param name="index">Button to subscribe to</param>
        /// <param name="handler">Callback to fire when input changes</param>
        /// <param name="id">ID of the subscriber. Can be left blank if you do not wish to allow multiple subscriptions to the same input</param>
        public bool SubscribeButton(string guidStr, int index, dynamic handler, string id = "0")
        {
            return Subscribe(guidStr, InputType.BUTTON, index, handler, id);
        }

        public bool UnSubscribeButton(string guidStr, int index, string id = "0")
        {
            return UnSubscribe(guidStr, InputType.BUTTON, index, id);
        }

        /// <summary>
        /// Adds a Subscription to a POV
        /// </summary>
        /// <param name="guidStr">Guid of the stick to subscribe to</param>
        /// <param name="index">Number of the POV to subscribe to</param>
        /// <param name="handler">Callback to fire when input changes</param>
        /// <param name="id">ID of the subscriber. Can be left blank if you do not wish to allow multiple subscriptions to the same input</param>
        public bool SubscribePov(string guidStr, int index, dynamic handler, string id = "0")
        {
            return Subscribe(guidStr, InputType.POV, index, handler, id);
        }

        public bool UnSubscribePov(string guidStr, int index, string id = "0")
        {
            return UnSubscribe(guidStr, InputType.POV, index, id);
        }

        // Gets a list of available devices and their caps
        public DeviceInfo[] GetDevices()
        {
            var devices = directInput.GetDevices();
            List<DeviceInfo> deviceList = new List<DeviceInfo>();
            foreach (var deviceInstance in devices)
            {
                if (!IsStickType(deviceInstance))
                    continue;
                deviceList.Add(new DeviceInfo(deviceInstance));
            }
            return deviceList.ToArray();
        }

        public DeviceInfo GetDeviceByGuid(string guidStr)
        {
            var guid = new Guid(guidStr);
            var devices = directInput.GetDevices();
            foreach (var deviceInstance in devices)
            {
                if (!IsStickType(deviceInstance))
                    continue;
                if (deviceInstance.InstanceGuid == guid)
                {
                    return new DeviceInfo(deviceInstance);
                }
            }
            return null;
        }

        // =============================== Internal ==================================

        // Monitor thread.
        // Watches any sticks that have been subscribed to
        // Examines incoming events for subscribed sticks to see if any match subscribed inputs for that stick
        // If any are found, fires the callback associated with the subscription

        private bool Subscribe(string guidStr, InputType inputType, int index, dynamic handler, string id = "0")
        {
            var guid = new Guid(guidStr);
            if (!subscribedSticks.ContainsKey(guid))
            {
                subscribedSticks[guid] = new StickSubscriptions(guid);
            }
            if (subscribedSticks[guid].Add(index, inputType, handler, id))
            {
                SetMonitorState();
                return true;
            }
            return false;
        }

        private bool UnSubscribe(string guidStr, InputType inputType, int index, string id = "0")
        {
            var guid = new Guid(guidStr);
            if (subscribedSticks.ContainsKey(guid))
            {
                if (subscribedSticks[guid].Remove(index, inputType, id))
                {
                    if (subscribedSticks[guid].subscriptions.Count == 0)
                    {
                        subscribedSticks.Remove(guid);
                    }
                    SetMonitorState();
                    return true;
                }
            }
            return false;
        }

        private void SetMonitorState()
        {
            if (threadRunning && subscribedSticks.Count == 0)
                threadRunning = false;
            else if (!threadRunning && subscribedSticks.Count > 0)
                MonitorSticks();
        }

        private void MonitorSticks()
        {
            var t = new Thread(new ThreadStart(() =>
            {
                threadRunning = true;
                //Debug.WriteLine("JoystickWrapper| MonitorSticks starting");
                while (threadRunning)
                {
                    // Iterate subscribed sticks
                    foreach (var subscribedStick in subscribedSticks)
                    {
                        var guid = subscribedStick.Key;
                        var joystick = subscribedStick.Value.joystick;
                        var subscriptions = subscribedStick.Value.subscriptions;
                        // Poll the stick
                        joystick.Poll();
                        var data = joystick.GetBufferedData();
                        // Iterate each report
                        foreach (var state in data)
                        {
                            // Do we have any subscriptions for that input?
                            if (subscriptions.ContainsKey(state.Offset))
                            {
                                // Fire all callbacks for that input
                                foreach (var subscribedInput in subscriptions[state.Offset])
                                {
                                    //Debug.WriteLine(String.Format("JoystickWrapper| Firing callback for id {0}", subscribedInput.Key));
                                    subscribedInput.Value(state.Value);
                                }
                            }
                        }
                    }
                    Thread.Sleep(1);
                }
                //Debug.WriteLine("JoystickWrapper| MonitorSticks stopping");
            }));
            t.Start();
        }

        private bool IsStickType(DeviceInstance deviceInstance)
        {
            return deviceInstance.Type == DeviceType.Joystick
                    || deviceInstance.Type == DeviceType.Gamepad
                    || deviceInstance.Type == DeviceType.FirstPerson
                    || deviceInstance.Type == DeviceType.Flight
                    || deviceInstance.Type == DeviceType.Driving;
        }
    }
}