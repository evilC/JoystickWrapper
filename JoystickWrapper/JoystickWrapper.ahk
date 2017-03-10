/*
JoystickWrapper Test script for AHK

Loads the JoystickWrapper DLL via CLR
Allows the user to subscribe to sticks and view the data coming from them
*/

; Include Lexikos' CLR library
#include CLR.ahk
OutputDebug DBGVIEWCLEAR

GoSub, BuildGUI
;~ ; Load the C# DLL
;~ asm := CLR_LoadLibrary("bin\debug\JoystickWrapper.dll")
;~ ; Use CLR to instantiate a class from within the DLL
;~ jw := asm.CreateInstance("JWNameSpace.JoystickWrapper")

jw := new JoystickWrapper("bin\debug\JoystickWrapper.dll")
guid := jw.GetAnyDeviceGuid()
;guid := "da2e2e00-19ea-11e6-8002-444553540000"
jw.Subscribe(guid, jw.InputTypes.Axis, 1, Func("TestFunc").Bind("X"))
jw.Subscribe(guid, jw.InputTypes.Axis, 2, Func("TestFunc").Bind("Y"))
jw.Subscribe(guid, jw.InputTypes.Axis, 3, Func("TestFunc").Bind("Z"))
jw.Subscribe(guid, jw.InputTypes.Axis, 4, Func("TestFunc").Bind("Rx"))
jw.Subscribe(guid, jw.InputTypes.Axis, 5, Func("TestFunc").Bind("Ry"))
jw.Subscribe(guid, jw.InputTypes.Axis, 6, Func("TestFunc").Bind("Rz"))
jw.Subscribe(guid, jw.InputTypes.Axis, 7, Func("TestFunc").Bind("S0"))
jw.Subscribe(guid, jw.InputTypes.Axis, 8, Func("TestFunc").Bind("S1"))
;GoSub, Init
return

TestFunc(axis, value){
	ToolTip % "Axis: " axis ", value: " value
}

class JoystickWrapper {
	__New(dllpath){
		this.DllPath := dllpath
		; Load the C# DLL
		asm := CLR_LoadLibrary(dllpath)
		; Use CLR to instantiate a class from within the DLL
		this.Interface := asm.CreateInstance("JWNameSpace.JoystickWrapper")
		this.InputTypes := this.Interface.inputTypes
	}
	
	Subscribe(guid, type, index, callback){
		this.Interface.Subscribe(guid, type, index, new this.Handler(callback))
	}
	
	GetDevices(){
		device_list := {}
		_device_list := this.Interface.GetDevices()
		ct := _device_list.MaxIndex()+1
		Loop % ct {
			dev := _device_list[A_Index - 1]
			device_list[dev.Guid] := dev.Name
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

;~ ; Called whenever a joystick changes
;~ class Handler
;~ {
	;~ __New(guid, axis){
		;~ this.guid := guid
		;~ this.axis := axis
	;~ }
	
    ;~ Handle(value)
    ;~ {
		;~ msgbox
		global hDeviceReports, device_list
		Gui, ListView, % hDeviceReports
		row := LV_Add(, this.guid, this.axis, value)
		LV_Modify(row, "vis")
    ;~ }
;~ }

; Gui handling code =================================

BuildGUI:
	; Set up GUI for demo
	Gui, Add, ListView, w300 h100 Checked hwndhDeviceSelect gDeviceSelected Altsubmit, Name|Guid
	LV_ModifyCol(1, 200)
	Gui, Add, ListView, w300 h300 hwndhDeviceReports, Device|Input|Value
	LV_ModifyCol(1, 100)
	LV_ModifyCol(2, 130)
	Gui, Show, x0 y0
	return

Init:
	polled_guids := 0
	device_list := {}
	Gui, ListView, % hDeviceSelect
	_device_list := jw.GetDevices()
	ct := _device_list.MaxIndex()+1
	Loop % ct {
		dev := _device_list[A_Index - 1]
		LV_Add(, dev.Name, dev.Guid)
		device_list[dev.Guid] := dev.Name
	}
	return

DeviceSelected:
    if (A_GuiEvent != "I")
        return
    Gui, ListView, % hDeviceSelect
    
    if (ErrorLevel = "c"){
        ; Check / Uncheck
        state := ErrorLevel == "C" ? 1 : 0
        LV_GetText(guid, A_EventInfo, 2)
        if (state){
            if (!IsObject(polled_guids))
                polled_guids := {}
            polled_guids[guid] := 1
			; ToDo: Implement capability detection and axis selection
			;jw.Subscribe(guid, jw.InputType.AXIS, 1, new Handler(guid, "Axis X"))	; Why can I not access the enum?
			;~ jw.SubscribeDev(dev.Axis[1], new Handler(guid, "Axis X"))
			jw.Subscribe(guid, jw.inputTypes.Axis, 8, new Handler(guid, "Axis 8"))
			jw.Subscribe(guid, jw.inputTypes.Button, 128, new Handler(guid, "Button 128"))
			jw.Subscribe(guid, jw.inputTypes.POV, 4, new Handler(guid, "POV 4"))
        } else {
            if (ObjHasKey(polled_guids, guid)){
                polled_guids.Delete(guid)
                if (IsEmptyAssoc(polled_guids)){
                    polled_guids := 0
					jw.Stop()
				}
            }
        }
    }
    return

IsEmptyAssoc(arr){
    for k, v in arr
        return 0
    return 1
}

^Esc::
GuiClose:
    jw.Stop()
	ExitApp