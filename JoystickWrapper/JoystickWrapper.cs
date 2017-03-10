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
            public Dictionary<JoystickOffset, List<dynamic>> subscriptions = new Dictionary<JoystickOffset, List<dynamic>>();

            public StickSubscriptions(Guid guid)
            {
                joystick = new Joystick(directInput, guid);
                var caps = joystick.Capabilities;

                // Build subscription arrays according to capabilities
                for (var i = 1; i <= caps.AxeCount; i++)
                {
                    subscriptions.Add(inputMappings[InputType.AXIS][i - 1], new List<dynamic>());
                }

                for (var i = 1; i <= caps.ButtonCount; i++)
                {
                    subscriptions.Add(inputMappings[InputType.BUTTON][i - 1], new List<dynamic>());
                }

                // Set BufferSize in order to use buffered data.
                joystick.Properties.BufferSize = 128;

                joystick.Acquire();
            }

            public void Add(int index, InputType inputType, dynamic handler)
            {
                var input = inputMappings[inputType][index-1];
                if (!subscriptions.ContainsKey(input))
                {
                    subscriptions[input] = new List<dynamic>();
                }
                subscriptions[input].Add(handler);
            }
        }

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

        //public Dictionary<string, InputType> inputTypes = new Dictionary<string, InputType>(StringComparer.OrdinalIgnoreCase)
        //{
        //    {"AXIS", InputType.AXIS},
        //    {"BUTTON", InputType.BUTTON},
        //    {"POV", InputType.POV}
        //};

        public class InputTypes
        {
            public InputType Axis { get { return InputType.AXIS; } }
            public InputType Button { get { return InputType.BUTTON; } }
            public InputType POV { get { return InputType.POV; } }
        }
        public InputTypes inputTypes = new InputTypes();

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
        /// <param name="index">Name of the axis to subscribe to</param>
        /// <param name="handler">Callback to fire when axis changes</param>
        public void Subscribe(string guidStr, InputType inputType, int index, dynamic handler)
        {
            var guid = new Guid(guidStr);
            if (!subscribedSticks.ContainsKey(guid))
            {
                subscribedSticks[guid] = new StickSubscriptions(guid);
            }
            var joystick = subscribedSticks[guid].joystick;

            subscribedSticks[guid].Add(index, inputType, handler);
        }

        //public void SubscribeDev(JoystickWrapper.JoystickInput input, dynamic handler)
        //{
        //    //var guid = new Guid(guidStr);
        //    //var devinfo = input.parent;
        //    //var guid = new Guid(devinfo.Guid);
        //    var guid = new Guid("83f38eb0-7433-11e6-8007-444553540000");
        //    if (!subscribedSticks.ContainsKey(guid))
        //    {
        //        subscribedSticks[guid] = new StickSubscriptions(guid);
        //    }
        //    var joystick = subscribedSticks[guid].joystick;

        //    subscribedSticks[guid].Add(input.index, input.inputType, handler);
        //}

        // Monitor thread.
        // Watches any sticks that have been subscribed to
        // Examines incoming events for subscribed sticks to see if any match subscribed inputs for that stick
        // If any are found, fires the callback associated with the subscription
        public void MonitorSticks()
        {
            var t = new Thread(new ThreadStart(() =>
            {
                threadRunning = true;
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
                                    subscribedInput.Handle(state.Value);
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

        public DeviceInfo GetDeviceByGuid(string guidStr)
        {
            var guid = new Guid(guidStr);
            var devices = directInput.GetDevices();
            foreach (var deviceInstance in devices)
            {
                if (deviceInstance.Type != DeviceType.Joystick
                    && deviceInstance.Type != DeviceType.Gamepad
                    && deviceInstance.Type != DeviceType.FirstPerson
                    && deviceInstance.Type != DeviceType.Flight
                    && deviceInstance.Type != DeviceType.Driving)
                    continue;
                if (deviceInstance.InstanceGuid == guid)
                {
                    return new DeviceInfo(deviceInstance);
                }
            }
            return null;
        }

        // Reply for GetDevices()
        public class DeviceInfo
        {
            //public SharpDX.DirectInput.Capabilities caps { get; set; }
            //public Dictionary<string, int> caps = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            //public List<JoystickInput> Axis = new List<JoystickInput>();
            public JoystickInput[] Axis;
            public List<JoystickInput> Button = new List<JoystickInput>();
            public List<JoystickInput> POV = new List<JoystickInput>();

            //public JoystickCaps caps = new JoystickCaps();
            public DeviceInfo(DeviceInstance deviceInstance)
            {
                var joystick = new Joystick(directInput, deviceInstance.InstanceGuid);
                var axes = new List<JoystickInput>();
                for (var i = 0; i <= joystick.Capabilities.AxeCount; i++)
                {
                    axes.Add(new JoystickInput(this, InputType.AXIS, i));
                    Axis = axes.ToArray();
                }
                for (var i = 0; i <= joystick.Capabilities.ButtonCount; i++)
                {
                    Button.Add(new JoystickInput(this, InputType.BUTTON, i));
                }
                for (var i = 0; i <= joystick.Capabilities.PovCount; i++)
                {
                    POV.Add(new JoystickInput(this, InputType.POV, i));
                }

                Name = deviceInstance.InstanceName;
                Guid = deviceInstance.InstanceGuid.ToString();
            }

            public string Name { get; set; }
            public string Guid { get; set; }
        }

        public class JoystickInput
        {
            public DeviceInfo parent { get; set; }
            public InputType inputType { get; set; }
            public int index { get; set; }

            public JoystickInput(DeviceInfo parentDevice, InputType iType, int i)
            {
                index = i;
                parent = parentDevice;
                inputType = iType;
            }
        }

        //public class JoystickCaps
        //{
        //    public int Axes { get; set; }
        //    public int Buttons { get; set; }
        //    public int POVs { get; set; }
        //}

    }
}