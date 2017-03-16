#persistent
#Include JoystickWrapper.ahk
jw := new JoystickWrapper("JoystickWrapper.dll")

;guid := "da2e2e00-19ea-11e6-8002-444553540000"

if (guid := jw.GetAnyDeviceGuid()){
	jw.SubscribeAxis(guid, 1, Func("TestFunc").Bind("Axis"))
	jw.SubscribeButton(guid, 1, Func("TestFunc").Bind("Button"))
	jw.SubscribePov(guid, 1, Func("TestFunc").Bind("Pov"))
	jw.SubscribePovDirection(guid, 1, 1, Func("TestFunc").Bind("PovDirection"))
} else {
	MsgBox "No sticks found"
	ExitApp
}

TestFunc(type, value){
	Tooltip % type ": " value
}

^Esc::
	ExitApp