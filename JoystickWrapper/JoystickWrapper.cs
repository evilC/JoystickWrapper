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
        public SubscribedSticks stickSubscriptions = new SubscribedSticks();

        // Keeps a record of which sticks are subscribed to
        public class SubscribedSticks
        {
            public Dictionary<Guid, SubscribedStick> Sticks { get; set; }
            public bool threadRunning = false;

            public SubscribedSticks()
            {
                Sticks = new Dictionary<Guid, SubscribedStick>();
            }

            public void Add(string guidStr, JoystickOffset offset, dynamic handler, string id, int povDirection = 0)
            {
                Guid guid = new Guid(guidStr);
                if (!Sticks.ContainsKey(guid))
                {
                    Sticks.Add(guid, new SubscribedStick(guid));
                }
                Sticks[guid].Add(offset, handler, id, povDirection);
                SetMonitorState();
            }

            public bool Remove(string guidStr, JoystickOffset offset, string id)
            {
                Guid guid = new Guid(guidStr);
                if (!Sticks.ContainsKey(guid))
                {
                    return false;
                }
                Sticks[guid].Remove(offset, id);
                if (Sticks[guid].Inputs.Count == 0)
                {
                    Sticks.Remove(guid);
                    SetMonitorState();
                }
                return true;
            }

            private void SetMonitorState()
            {
                if (threadRunning && Sticks.Count == 0)
                    threadRunning = false;
                else if (!threadRunning && Sticks.Count > 0)
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
                        foreach (var subscribedStick in Sticks.Values)
                        {
                            subscribedStick.Poll();
                        }
                        Thread.Sleep(1);
                    }
                    //Debug.WriteLine("JoystickWrapper| MonitorSticks stopping");
                }));
                t.Start();
            }
        }

        // For a given stick, keeps a record of which inputs are subscribed to
        public class SubscribedStick
        {
            public Dictionary<JoystickOffset, SubscribedInput> Inputs { get; set; }
            private Joystick joystick;

            public SubscribedStick(Guid guid)
            {
                Inputs = new Dictionary<JoystickOffset, SubscribedInput>();
                joystick = new Joystick(directInput, guid);
                // Set BufferSize in order to use buffered data.
                joystick.Properties.BufferSize = 128;

                joystick.Acquire();
            }

            public void Add(JoystickOffset offset, dynamic handler, string id, int povDirection = 0)
            {
                if (!Inputs.ContainsKey(offset))
                {
                    Inputs.Add(offset, new SubscribedInput());
                }
                Inputs[offset].Add(handler, id, povDirection);
            }

            public bool Remove(JoystickOffset offset, string id)
            {
                if (!Inputs.ContainsKey(offset))
                {
                    return false;
                }
                Inputs[offset].Remove(id);
                if (Inputs[offset].Subscriptions.Count == 0)
                {
                    Inputs.Remove(offset);
                }
                return true;
            }

            public void Poll(){
                joystick.Poll();
                var data = joystick.GetBufferedData();
                // Iterate each report
                foreach (var state in data)
                {
                    if (Inputs.ContainsKey(state.Offset))
                    {
                        Inputs[state.Offset].ProcessPollRecord(state.Value);
                    }
                }
            }

        }

        // For a given input, maintains a list of Subscribers that wish to be notified when the input changes
        public class SubscribedInput
        {
            public Dictionary<string, Subscription> Subscriptions { get; set; }
            public Dictionary<int, PovDirectionSubscriptions> PovDirectionSubscriptions { get; set; }

            public SubscribedInput()
            {
                Subscriptions = new Dictionary<string, Subscription>(StringComparer.OrdinalIgnoreCase);
                PovDirectionSubscriptions = new Dictionary<int, PovDirectionSubscriptions>();
            }

            public void Add(dynamic handler, string subscriberID, int povDirection = 0)
            {
                if (povDirection == 0)
                {
                    // Regular mapping
                    if (Subscriptions.ContainsKey(subscriberID))
                    {
                        Subscriptions[subscriberID].Callback = handler;
                    }
                    else
                    {
                        Subscriptions.Add(subscriberID, new Subscription(handler));
                    }
                }
                else
                {
                    // Pov Direction Mapping
                    //if (povDirection < 1 || povDirection > 8)
                    //{
                    //    return /*false*/;
                    //}
                    PovDirectionSubscriptions.Add(povDirection, new PovDirectionSubscriptions(subscriberID, povDirection, handler));
                }
            }

            public bool Remove(string subscriberID)
            {
                if (!Subscriptions.ContainsKey(subscriberID))
                {
                    return false;
                }
                Subscriptions.Remove(subscriberID);
                return true;
            }

            public void ProcessPollRecord(int value)
            {
                foreach (var sub in Subscriptions)
                {
                    sub.Value.Callback(value);
                }

                foreach (var dir in PovDirectionSubscriptions)
                {
                    dir.Value.ProcessPollRecord(value);
                }
            }
        }

        // Holds information on a given subscription
        public class Subscription
        {
            public dynamic Callback { get; set; }

            public Subscription(dynamic callback)
            {
                Callback = callback;
            }
        }

        // Holds information on a given POV Direction subscription
        public class PovDirectionSubscriptions
        {
            public int Direction { get; set; }      // The Direction Mapped to. A number from 1-8
            private int Angle { get; set; }         // The pure angle (in degrees) that the Direction maps to
            public int Tolerance { get; set; }      //  How many degrees either side of angle to consider a match
            private int Min { get; set; }           // The minimum angle to consider "pressed" (May contain negative value if Angle is close to 0)
            private int Max { get; set; }           // The minimum angle to consider "pressed" (May contain value over 360 is Angle is close to 360)
            private bool State { get; set; }         // The current state of the direction. Used so we can decide whether to send press or release events
            public Dictionary<string, Subscription> Subscriptions { get; set; }

            public PovDirectionSubscriptions(string subscriberID, int povDirection, dynamic callback)
            {
                Direction = povDirection;
                State = false;
                // Pre-calculate values, so we do less work each tick of the Monitor thread
                Tolerance = 45; // Hard-code tolerance for now - allow configuring at some point. Tolerance setting is same for all bindings though.
                Angle = (povDirection - 1) * 45;    // convert 8-way to degrees
                Min = Angle - Tolerance;
                Max = Angle + Tolerance;

                Subscriptions = new Dictionary<string, Subscription>(StringComparer.OrdinalIgnoreCase);
                Subscriptions.Add(subscriberID, new Subscription(callback));
            }

            public void ProcessPollRecord(int value)
            {
                bool newState = ValueMatchesAngle(value);
                if (newState != State)
                {
                    State = newState;
                    var ret = Convert.ToInt32(State);
                    foreach (var sub in Subscriptions)
                    {
                        sub.Value.Callback(ret);
                    }
                }
            }

            private bool ValueMatchesAngle(int value)
            {
                return (value != -1 && value >= Min && value <= Max);
            }
        }

        // For some reason the AHK code cannot access this Enum
        public enum InputType { AXIS, BUTTON, POV };

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

        // ================================ Publicly Exposed Methods =====================================
        // Note! Be WARY of overloading any method expected to be hit by non C code
        // To insulate end-users from unforseen behaviour, avoid overloading API endpoints

        // Constructor
        public JoystickWrapper()
        {
            directInput = new DirectInput();
        }


        // Subscribe / Unsubscribe methods
        public void SubscribeAxis(string guid, int index, dynamic handler, string id = "0")
        {
            var offset = inputMappings[InputType.AXIS][index - 1];
            Subscribe(guid, offset, handler, id);
        }

        public void UnSubscribeAxis(string guid, int index, string id = "0")
        {
            var offset = inputMappings[InputType.AXIS][index - 1];
            UnSubscribe(guid, offset, id);
        }

        public void SubscribeButton(string guid, int index, dynamic handler, string id = "0")
        {
            var offset = inputMappings[InputType.BUTTON][index - 1];
            Subscribe(guid, offset, handler, id);
        }

        public void UnSubscribeButton(string guid, int index, string id = "0")
        {
            var offset = inputMappings[InputType.BUTTON][index - 1];
            UnSubscribe(guid, offset, id);
        }

        public void SubscribePov(string guid, int index, dynamic handler, string id = "0")
        {
            var offset = inputMappings[InputType.POV][index - 1];
            Subscribe(guid, offset, handler, id);
        }

        public void SubscribePovDirection(string guid, int index, int povDirection, dynamic handler, string id = "0")
        {
            var offset = inputMappings[InputType.POV][index - 1];
            _SubscribePovDirection(guid, offset, povDirection, handler, id);
        }

        public void UnSubscribePov(string guid, int index, string id = "0")
        {
            var offset = inputMappings[InputType.POV][index - 1];
            UnSubscribe(guid, offset, id);
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

        public string GetAnyDeviceGuid()
        {
            var devices = directInput.GetDevices();
            foreach (var deviceInstance in devices)
            {
                if (!IsStickType(deviceInstance))
                    continue;
                return deviceInstance.InstanceGuid.ToString();
            }
            return "";
        }

        // =========================== Internal Methods ==========================================
        private void Subscribe(string guid, JoystickOffset offset, dynamic handler, string id = "0")
        {
            stickSubscriptions.Add(guid, offset, handler, id);
        }

        private void _SubscribePovDirection(string guid, JoystickOffset offset, int povDirection, dynamic handler, string id = "0")
        {
            stickSubscriptions.Add(guid, offset, handler, id, povDirection);
        }

        private void UnSubscribe(string guid, JoystickOffset offset, string id = "0")
        {
            stickSubscriptions.Remove(guid, offset, id);
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