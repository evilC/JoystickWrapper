#persistent
#Include JoystickWrapper.ahk
jw := new JoystickWrapper("JoystickWrapper.dll")

device := jw.GetXInputDevices(1)
jw.SubscribeXboxAxis(1, 3, Func("OnTrigger").Bind(1))
jw.SubscribeXboxAxis(1, 6, Func("OnTrigger").Bind(2))

OnTrigger(whichMotor, value){
	global jw
	; Trigger axis reports 0..256, Rumble speed is 0...65535, so multiply axis value by 257
	jw.SetXboxRumble(1, whichMotor, value * 257)
}

^Esc::
	ExitApp