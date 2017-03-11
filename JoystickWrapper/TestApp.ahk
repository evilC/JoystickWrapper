/*
JoystickWrapper Test script for AHK

Loads the JoystickWrapper DLL via CLR
Allows the user to subscribe to sticks and view the data coming from them
*/
;guid := "da2e2e00-19ea-11e6-8002-444553540000"
OutputDebug DBGVIEWCLEAR
#Include JoystickWrapper.ahk
jw := new JoystickWrapper("bin\debug\JoystickWrapper.dll")

selected_device := 0
AxisList := ["X", "Y", "Z", "Rx", "Ry", "Rz", "S0", "S1"]

GoSub, BuildGUI
GoSub, Init
return

TestFunc(device, input, value){
	global hDeviceReports
	Gui, ListView, % hDeviceReports
	row := LV_Add(, device, input, value)
	LV_Modify(row, "Vis")
}

BuildGUI:
	; Set up GUI for demo
	Gui, Add, ListView, w300 h100 Checked hwndhDeviceSelect gDeviceSelected Altsubmit, Name|Guid
	LV_ModifyCol(1, 200)
	Gui, Add, ListView, w300 h300 hwndhDeviceReports, Device|Input|Value
	LV_ModifyCol(1, 175)
	LV_ModifyCol(2, 60)
	Gui, Show, x0 y0
	return

Init:
	device_list := jw.GetDevices()
	Gui, ListView, % hDeviceSelect
	for guid, device in device_list {
		LV_Add(, device.Name, device.Guid)
	}
	return

DeviceSelected:
    if (A_GuiEvent != "I")
        return
    Gui, ListView, % hDeviceSelect
    
    if (ErrorLevel = "c"){
        ; Check / Uncheck
        state := ErrorLevel == "C" ? 1 : 0
        LV_GetText(selected_device, A_EventInfo, 2)
		dev := device_list[selected_device]
        if (state){
			Loop % dev.Axes {
				jw.SubscribeAxis(selected_device, A_Index, Func("TestFunc").Bind(dev.Name, AxisList[A_Index] " Axis"), "LV1")
			}
			Loop % dev.Buttons {
				jw.SubscribeButton(selected_device, A_Index, Func("TestFunc").Bind(dev.Name, "Button " A_Index), "LV1")
			}
			Loop % dev.POVs {
				jw.SubscribePov(selected_device, A_Index, Func("TestFunc").Bind(dev.Name, " POV " A_Index), "LV1")
			}
        } else {
			Loop % dev.Axes {
				jw.UnSubscribeAxis(selected_device, A_Index, "LV1")
			}
			Loop % dev.Buttons {
				jw.UnSubscribeButton(selected_device, A_Index, "LV1")
			}
			Loop % dev.POVs {
				jw.UnSubscribePov(selected_device, A_Index, "LV1")
			}
		}
    }
    return

^Esc::
GuiClose:
    jw.Stop()
	ExitApp