/*
JoystickWrapper Test script for AHK

Loads the JoystickWrapper DLL via CLR
Allows the user to subscribe to sticks and view the data coming from them
*/
;guid := "da2e2e00-19ea-11e6-8002-444553540000"

; Include Lexikos' CLR library
#include CLR.ahk
OutputDebug DBGVIEWCLEAR

selected_device := 0
AxisList := ["X", "Y", "Z", "Rx", "Ry", "Rz", "S0", "S1"]

GoSub, BuildGUI

jw := new JoystickWrapper("bin\debug\JoystickWrapper.dll")
GoSub, Init
return

TestFunc(device, input, value){
	global hDeviceReports
	Gui, ListView, % hDeviceReports
	row := LV_Add(, device, input, value)
	LV_Modify(row, "Vis")
}

class JoystickWrapper {
	__New(dllpath){
		this.DllPath := dllpath
		; Load the C# DLL
		asm := CLR_LoadLibrary(dllpath)
		; Use CLR to instantiate a class from within the DLL
		this.Interface := asm.CreateInstance("JWNameSpace.JoystickWrapper")
	}
	
	SubscribeAxis(guid, index, callback){
		this.Interface.SubscribeAxis(guid, index, new this.Handler(callback))
	}
	
	SubscribeButton(guid, index, callback){
		this.Interface.SubscribeButton(guid, index, new this.Handler(callback))
	}
	
	SubscribePov(guid, index, callback){
		this.Interface.SubscribePov(guid, index, new this.Handler(callback))
	}
	
	GetDevices(){
		device_list := {}
		_device_list := this.Interface.GetDevices()
		ct := _device_list.MaxIndex()+1
		Loop % ct {
			dev := _device_list[A_Index - 1]
			device_list[dev.Guid] := { Name: dev.Name, Guid: dev.Guid, Axes: dev.Axes, Buttons: dev.Buttons, POVs: dev.POVs}
		}
		return device_list
	}
	
	GetAnyDeviceGuid(){
		return this.Interface.GetDevices()[0].Guid
		devices := this.GetDevices()
		for guid, dev in devices {
			return guid
		}
		return 0
	}
	
	class Handler {
		__New(callback){
			this.callback := callback
		}
		
		Handle(value){
			this.Callback.Call(value)
		}
	}
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
        if (state){
			dev := device_list[selected_device]
			Loop % dev.Axes {
				jw.SubscribeAxis(selected_device, A_Index, Func("TestFunc").Bind(dev.Name, AxisList[A_Index] " Axis"))
			}
			Loop % dev.Buttons {
				jw.SubscribeButton(selected_device, A_Index, Func("TestFunc").Bind(dev.Name, "Button " A_Index))
			}
			Loop % dev.POVs {
				jw.SubscribePov(selected_device, A_Index, Func("TestFunc").Bind(dev.Name, " POV " A_Index))
			}
        }
    }
    return

^Esc::
GuiClose:
    jw.Stop()
	ExitApp