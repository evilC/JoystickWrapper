#persistent
#Include JoystickWrapper.ahk
jw := new JoystickWrapper("JoystickWrapper.dll")

device := jw.GetXInputDevices(1)
jw.SubscribeXboxAxis(1, 3, Func("OnTrigger"))
;~ jw.SubscribeXboxAxis(1, 6, Func("OnTrigger"))

OnTrigger(value){
	global jw
	;~ jw.SetXboxRumble(1, value * 257)
	jw.SetXboxRumble()
}
a := 1
;~ if (guid := jw.GetAnyDeviceGuid()){
	;~ jw.SubscribeAxis(guid, 1, Func("TestFunc").Bind("Axis"))
	;~ jw.SubscribeButton(guid, 1, Func("TestFunc").Bind("Button"))
	;~ jw.SubscribePov(guid, 1, Func("TestFunc").Bind("Pov"))
	;~ jw.SubscribePovDirection(guid, 1, 1, Func("TestFunc").Bind("PovDirection"))
;~ } else {
	;~ MsgBox "No sticks found"
	;~ ExitApp
;~ }

;~ TestFunc(type, value){
	;~ Tooltip % type ": " value
;~ }

^Esc::
	ExitApp