/*
JoystickWrapper Test script for AHK

Loads the JoystickWrapper DLL via CLR
Allows the user to subscribe to sticks and view the data coming from them
*/
;OutputDebug DBGVIEWCLEAR
#Include JoystickWrapper.ahk
jw := new JoystickWrapper("bin\debug\JoystickWrapper.dll")

GoSub, BuildGUI
GoSub, Init
return

TestFunc(lv_id, device, input, value){
	global ReportLVs
	Gui, ListView, % ReportLVs[lv_id]
	row := LV_Add(, device, input, value)
	LV_Modify(row, "Vis")
}

BuildGUI:
	fn := Func("DeviceSelected").Bind(1)
	; Set up GUI for demo
	Gui, Add, ListView, xm w300 h100 Checked hwndhDeviceSelect1 Altsubmit, Name|Guid
	GuiControl, +g, % hDeviceSelect1, % fn
	LV_ModifyCol(1, 200)
	
	fn := Func("DeviceSelected").Bind(2)
	Gui, Add, ListView, x+5 w300 h100 Checked hwndhDeviceSelect2 gDeviceSelected Altsubmit, Name|Guid
	GuiControl, +g, % hDeviceSelect2, % fn
	LV_ModifyCol(1, 200)
	
	DeviceLVs := [hDeviceSelect1, hDeviceSelect2]
	
	Gui, Add, ListView, xm w300 h300 hwndhDeviceReports1, Device|Input|Value
	LV_ModifyCol(1, 175)
	LV_ModifyCol(2, 60)
	LV_ModifyCol(1, 200)
	
	Gui, Add, ListView, x+5 w300 h300 hwndhDeviceReports2, Device|Input|Value
	LV_ModifyCol(1, 175)
	LV_ModifyCol(2, 60)
	Gui, Show, x0 y0
	
	ReportLVs := [hDeviceReports1, hDeviceReports2]
	return

Init:
	DeviceList := jw.GetDevices()
	for guid, device in DeviceList {
		Gui, ListView, % hDeviceSelect1
		LV_Add(, device.Name, device.Guid)
		Gui, ListView, % hDeviceSelect2
		LV_Add(, device.Name, device.Guid)
	}
	return

;DeviceSelected:
DeviceSelected(lv_id){
	global jw, DeviceLVs, DeviceList
	static AxisList := ["X", "Y", "Z", "Rx", "Ry", "Rz", "S0", "S1"]
	
    if (A_GuiEvent != "I")
        return
    Gui, ListView, % DeviceLVs[lv_id]
    
    if (ErrorLevel = "c"){
        ; Check / Uncheck
        state := ErrorLevel == "C" ? 1 : 0
        LV_GetText(selected_device, A_EventInfo, 2)
		dev := DeviceList[selected_device]
        if (state){
			Loop % dev.Axes {
				jw.SubscribeAxis(selected_device, A_Index, Func("TestFunc").Bind(lv_id, dev.Name, AxisList[A_Index] " Axis"), lv_id)
			}
			Loop % dev.Buttons {
				jw.SubscribeButton(selected_device, A_Index, Func("TestFunc").Bind(lv_id, dev.Name, "Button " A_Index), lv_id)
			}
			Loop % dev.POVs {
				jw.SubscribePov(selected_device, A_Index, Func("TestFunc").Bind(lv_id, dev.Name, " POV " A_Index), lv_id)
			}
        } else {
			Loop % dev.Axes {
				jw.UnSubscribeAxis(selected_device, A_Index, lv_id)
			}
			Loop % dev.Buttons {
				jw.UnSubscribeButton(selected_device, A_Index, lv_id)
			}
			Loop % dev.POVs {
				jw.UnSubscribePov(selected_device, A_Index, lv_id)
			}
		}
    }
}

^Esc::
GuiClose:
    jw.Stop()
	ExitApp