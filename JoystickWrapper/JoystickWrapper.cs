﻿using System;
using System.Collections.Generic;
using System.Threading;

using SharpDX.DirectInput;
using System.Diagnostics;

namespace JWNameSpace
{
    public class JoystickWrapper
    {
        static private DirectInput directInput;
        private SubscribedSticks stickSubscriptions = new SubscribedSticks();

        // ================================ Publicly Exposed Methods and Classes =====================================
        // Note! Be WARY of overloading any method expected to be hit by non C code
        // To insulate end-users from unforseen behaviour, avoid overloading API endpoints

        // Constructor
        public JoystickWrapper()
        {
            directInput = new DirectInput();
        }


        // --------------------------- Subscribe / Unsubscribe methods ----------------------------------------
        public bool SubscribeAxis(string guid, int index, dynamic handler, string id = "0")
        {
            if (index < 1 || index > 8)
                return false;
            var offset = inputMappings[InputType.AXIS][index - 1];
            return Subscribe(guid, offset, handler, id);
        }

        public bool UnSubscribeAxis(string guid, int index, string id = "0")
        {
            if (index < 1 || index > 8)
                return false;
            var offset = inputMappings[InputType.AXIS][index - 1];
            return UnSubscribe(guid, offset, id);
        }

        public bool SubscribeButton(string guid, int index, dynamic handler, string id = "0")
        {
            if (index < 1 || index > 128)
                return false;
            var offset = inputMappings[InputType.BUTTON][index - 1];
            return Subscribe(guid, offset, handler, id);
        }

        public bool UnSubscribeButton(string guid, int index, string id = "0")
        {
            if (index < 1 || index > 128)
                return false;
            var offset = inputMappings[InputType.BUTTON][index - 1];
            return UnSubscribe(guid, offset, id);
        }

        public bool SubscribePov(string guid, int index, dynamic handler, string id = "0")
        {
            if (index < 1 || index > 4)
                return false;
            var offset = inputMappings[InputType.POV][index - 1];
            return Subscribe(guid, offset, handler, id);
        }

        public bool SubscribePovDirection(string guid, int index, int povDirection, dynamic handler, string id = "0")
        {
            if (index < 1 || index > 4 || povDirection < 1 || povDirection > 4)
                return false;
            var offset = inputMappings[InputType.POV][index - 1];
            return Subscribe(guid, offset, handler, id, povDirection);
        }

        public bool UnSubscribePov(string guid, int index, string id = "0")
        {
            if (index < 1 || index > 4)
                return false;
            var offset = inputMappings[InputType.POV][index - 1];
            return UnSubscribe(guid, offset, id);
        }

        public bool UnSubscribePovDirection(string guid, int index, int povDirection, string id = "0")
        {
            if (index < 1 || index > 4 || povDirection < 1 || povDirection > 4)
                return false;
            var offset = inputMappings[InputType.POV][index - 1];
            return UnSubscribe(guid, offset, id, povDirection);
        }

        // ------------------------------ Querying Methods ------------------------------------
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

        public string GetDeviceGuidByName(string name)
        {
            var devices = directInput.GetDevices();
            foreach (var deviceInstance in devices)
            {
                if (!IsStickType(deviceInstance))
                    continue;
                if (String.Equals(deviceInstance.InstanceName.TrimEnd('\0'), name, StringComparison.OrdinalIgnoreCase))
                {
                    return deviceInstance.InstanceGuid.ToString();
                }
            }
            return "";
        }

        // ---------------------------- Publicly visible datatypes ---------------------------------
        // Allows categorization of input types
        public enum InputType { AXIS, BUTTON, POV };

        // Reply for GetDevices()
        public class DeviceInfo
        {
            public int Axes { get; set; }
            public int Buttons { get; set; }
            public int POVs { get; set; }
            public int[] SupportedAxes { get; set; }

            private static List<JoystickOffset> joystickAxisOffsets = new List<JoystickOffset>() { JoystickOffset.X, JoystickOffset.Y, JoystickOffset.Z, JoystickOffset.RotationX, JoystickOffset.RotationY, JoystickOffset.RotationZ, JoystickOffset.Sliders0, JoystickOffset.Sliders1 };

            public DeviceInfo(DeviceInstance deviceInstance)
            {
                var joystick = new Joystick(directInput, deviceInstance.InstanceGuid);
                joystick.Acquire();
                JoystickState state = joystick.GetCurrentState();
                Axes = joystick.Capabilities.AxeCount;
                Buttons = joystick.Capabilities.ButtonCount;
                POVs = joystick.Capabilities.PovCount;

                Name = deviceInstance.InstanceName;
                Guid = deviceInstance.InstanceGuid.ToString();


                var sa = new List<int>();
                for (int i = 0; i < joystickAxisOffsets.Count; i++)
                {
                    try
                    {
                        var mightGoBoom = joystick.GetObjectInfoByName(joystickAxisOffsets[i].ToString());
                        sa.Add(i+1);
                    }
                    catch { }
                }
                SupportedAxes = sa.ToArray();
                joystick.Unacquire();
            }

            public string Name { get; set; }
            public string Guid { get; set; }
        }

        // =============================================== Private datatypes ==========================================

        // Maps SharpDX "Offsets" (Input Identifiers) to both iinput type and input index (eg x axis to axis 1)
        private static Dictionary<InputType, List<JoystickOffset>> inputMappings = new Dictionary<InputType, List<JoystickOffset>>(){
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

        // ================================================== Private Methods ==============================================================
        private bool Subscribe(string guid, JoystickOffset offset, dynamic handler, string id, int povDirection = 0)
        {
            // Block the Monitor thread from polling while we update the data structures
            lock (stickSubscriptions.Sticks)
            {
                return stickSubscriptions.Add(guid, offset, handler, id, povDirection);
            }
        }

        private bool UnSubscribe(string guid, JoystickOffset offset, string id, int povDirection = 0)
        {
            // Block the Monitor thread from polling while we update the data structures
            lock (stickSubscriptions.Sticks)
            {
                return stickSubscriptions.Remove(guid, offset, id, povDirection);
            }
        }

        private bool IsStickType(DeviceInstance deviceInstance)
        {
            return deviceInstance.Type == DeviceType.Joystick
                    || deviceInstance.Type == DeviceType.Gamepad
                    || deviceInstance.Type == DeviceType.FirstPerson
                    || deviceInstance.Type == DeviceType.Flight
                    || deviceInstance.Type == DeviceType.Driving
                    || deviceInstance.Type == DeviceType.Supplemental;
        }

        // ------------------------------------------ Subscription Handling -------------------------------------------------

        // Handles storing subscriptions for (and processing input of) a collection of joysticks
        private class SubscribedSticks
        {
            public Dictionary<Guid, SubscribedStick> Sticks { get; set; }
            public bool threadRunning = false;

            public SubscribedSticks()
            {
                Sticks = new Dictionary<Guid, SubscribedStick>();
            }

            public bool RegisterStick(string guidStr)
            {
                Guid guid = new Guid(guidStr);
                Joystick joystick;
                if (!Sticks.ContainsKey(guid))
                {
                    try
                    {
                        joystick = new Joystick(directInput, guid);
                    }
                    catch
                    {
                        return false;
                    }
                    var stick = new SubscribedStick(joystick);
                    Sticks.Add(guid, stick);
                }
                return true;
            }

            public bool Add(string guidStr, JoystickOffset offset, dynamic handler, string id, int povDirection = 0)
            {
                if (!RegisterStick(guidStr))
                {
                    return false;
                }
                Guid guid = new Guid(guidStr);
                if (!Sticks[guid].Add(offset, handler, id, povDirection))
                {
                    RemoveStickIfEmpty(guid);   // If the stick was valid, but non-existent axis or button, then remove the stick again if it is empty
                    return false;
                }
                SetMonitorState();
                return true;
            }

            public bool Remove(string guidStr, JoystickOffset offset, string id, int povDirection = 0)
            {
                Guid guid = new Guid(guidStr);
                if (!Sticks.ContainsKey(guid))
                {
                    return false;
                }
                var ret = Sticks[guid].Remove(offset, id, povDirection);
                RemoveStickIfEmpty(guid);
                return ret;
            }

            // If a stick has no bindings associated with it, remove it from the Dictionary, so the Monitor thread does not poll it
            private bool RemoveStickIfEmpty(Guid guid)
            {
                if (Sticks[guid].IsEmpty())
                {
                    Sticks.Remove(guid);
                    SetMonitorState();
                    return true;
                }
                return false;
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
                    Debug.WriteLine("JoystickWrapper| MonitorSticks starting");
                    while (threadRunning)
                    {
                        // Iterate subscribed sticks
                        if (Sticks.Count > 0)
                        {
                            foreach (var subscribedStick in Sticks.Values)
                            {
                                // Obtain a lock, so that the data structures are not modified mid-poll
                                lock (Sticks)
                                {
                                    subscribedStick.Poll();
                                }
                            }
                        }
                        Thread.Sleep(1);
                    }
                    Debug.WriteLine("JoystickWrapper| MonitorSticks stopping");
                }));
                t.Start();
            }
        }

        // Handles storing subscriptions for (and processing input of) a specific joystick
        private class SubscribedStick
        {
            public Dictionary<JoystickOffset, bool> SupportedInputs { get; set; }
            public Dictionary<JoystickOffset, SubscribedInput> Inputs { get; set; }
            public Joystick joystick;

            public SubscribedStick(Joystick passedStick)
            {
                joystick = passedStick;
                Inputs = new Dictionary<JoystickOffset, SubscribedInput>();
                SupportedInputs = new Dictionary<JoystickOffset, bool>();
                
                // Set BufferSize in order to use buffered data.
                joystick.Properties.BufferSize = 128;

                joystick.Acquire();

                foreach (JoystickOffset offset in Enum.GetValues(typeof(JoystickOffset)))
                {
                    var present = true;
                    try
                    {
                        var test = joystick.GetObjectInfoByName(offset.ToString());
                    }
                    catch
                    {
                        present = false;
                    }
                    SupportedInputs[offset] = present;
                }
            }

            ~SubscribedStick()
            {
                try
                {
                    joystick.Unacquire();
                }
                catch { }
            }

            public bool Add(JoystickOffset offset, dynamic handler, string id, int povDirection = 0)
            {
                if (!SupportedInputs[offset])
                    return false;
                if (!Inputs.ContainsKey(offset))
                {
                    Inputs.Add(offset, new SubscribedInput());
                }
                return Inputs[offset].Add(handler, id, povDirection);
            }

            public bool Remove(JoystickOffset offset, string id, int povDirection = 0)
            {
                if (!Inputs.ContainsKey(offset))
                {
                    return false;
                }
                var ret = Inputs[offset].Remove(id, povDirection);
                if (ret)
                {
                    if (Inputs[offset].Subscriptions.Count == 0)
                    {
                        Inputs.Remove(offset);
                    }
                }
                return ret;
            }

            public bool IsEmpty()
            {
                return Inputs.Count == 0;
            }

            public void Poll()
            {
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

        // Handles storing subscriptions for (and processing input of) a specific input on a specific joystick
        private class SubscribedInput
        {
            public Dictionary<string, Subscription> Subscriptions { get; set; }
            public Dictionary<int, SubscribedPovDirection> PovDirectionSubscriptions { get; set; }

            public SubscribedInput()
            {
                Subscriptions = new Dictionary<string, Subscription>(StringComparer.OrdinalIgnoreCase);
                PovDirectionSubscriptions = new Dictionary<int, SubscribedPovDirection>();
            }

            public bool Add(dynamic handler, string subscriberID, int povDirection = 0)
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
                    return true;
                }
                else
                {
                    //Pov Direction Mapping
                    if (povDirection < 1 || povDirection > 4)
                    {
                        return false;
                    }
                    if (!PovDirectionSubscriptions.ContainsKey(povDirection))
                    {
                        PovDirectionSubscriptions.Add(povDirection, new SubscribedPovDirection(povDirection));
                    }
                    return PovDirectionSubscriptions[povDirection].Add(subscriberID, handler);
                }
            }

            public bool Remove(string subscriberID, int povDirection = 0)
            {
                if (povDirection == 0)
                {
                    // Regular mapping
                    if (!Subscriptions.ContainsKey(subscriberID))
                    {
                        return false;
                    }
                    Subscriptions.Remove(subscriberID);
                    return true;
                }
                else
                {
                    // POV Direction Mapping
                    if (PovDirectionSubscriptions.ContainsKey(povDirection))
                    {

                        var ret = PovDirectionSubscriptions[povDirection].Remove(subscriberID);
                        if (ret && PovDirectionSubscriptions[povDirection].IsEmpty())
                        {
                            PovDirectionSubscriptions.Remove(povDirection);
                        }
                        return ret;
                    }
                    return false;
                }
            }

            public bool IsEmpty()
            {
                return Subscriptions.Count > 0;
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

        // Handles storing subscriptions for (and processing input of) a specific direction of a POV on a specific joystick
        private class SubscribedPovDirection
        {
            public int Direction { get; set; }      // Setting: The Direction Mapped to. A number from 1-4 N/E/S/W
            public int Tolerance { get; set; }      // Setting: How many degrees either side of angle to consider a match
            private int Angle { get; set; }         // PreCalculation: The angle as reported by DirectX (0..36000) that the Direction maps to
            private bool State { get; set; }         // The current state of the direction. Used so we can decide whether to send press or release events
            public Dictionary<string, Subscription> Subscriptions { get; set; }

            public SubscribedPovDirection(int povDirection)
            {
                Direction = povDirection;
                State = false;
                // Pre-calculate values, so we do less work each tick of the Monitor thread
                Tolerance = 4500; // Hard-code tolerance for now - allow configuring at some point. Tolerance setting is same for all bindings though.
                Angle = (povDirection - 1) * 9000;    // convert 4-way to degrees
                Subscriptions = new Dictionary<string, Subscription>(StringComparer.OrdinalIgnoreCase);
            }

            public bool Add(string subscriberID, dynamic handler)
            {
                Subscriptions.Add(subscriberID, new Subscription(handler));
                return true;
            }

            public bool Remove(string subscriberID)
            {
                if (Subscriptions.ContainsKey(subscriberID))
                {
                    Subscriptions.Remove(subscriberID);
                    return true;
                }
                return false;
            }

            public bool IsEmpty()
            {
                return Subscriptions.Count == 0;
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
                if (value == -1)
                    return false;
                var diff = AngleDiff(value, Angle);
                Debug.WriteLine(String.Format("JoystickWrapper| Angle = {0}, Value = {1}, Diff = {2}", Angle, value, diff));
                return value != -1 && AngleDiff(value, Angle) <= Tolerance;
            }

            private int AngleDiff(int a, int b)
            {
                var result1 = a - b;
                if (result1 < 0)
                    result1 += 36000;

                var result2 = b - a;
                if (result2 < 0)
                    result2 += 36000;

                return Math.Min(result1, result2);
            }
        }

        // Holds information on a given subscription
        private class Subscription
        {
            public dynamic Callback { get; set; }

            public Subscription(dynamic callback)
            {
                Callback = callback;
            }
        }

    }
}