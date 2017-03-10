/*
JoystickWrapper Test script for AHK

Loads the JoystickWrapper DLL via CLR
Allows the user to subscribe to sticks and view the data coming from them
*/

; Include Lexikos' CLR library
#include CLR.ahk
OutputDebug DBGVIEWCLEAR

GoSub, BuildGUI

; Load the C# DLL
asm := CLR_LoadLibrary("bin\debug\JoystickWrapper.dll")
; Use CLR to instantiate a class from within the DLL
jw := asm.CreateInstance("JWNameSpace.JoystickWrapper")

GoSub, Init
return

; Called whenever a joystick changes
class Handler
{
	__New(guid, axis){
		this.guid := guid
		this.axis := axis
	}
	
    Handle(value)
    {
		global hDeviceReports, device_list
		Gui, ListView, % hDeviceReports
		row := LV_Add(, this.guid, this.axis, value)
		LV_Modify(row, "vis")
    }
}

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
			jw.SubscribeAxis(guid, "x", new Handler(guid, "x"))
			jw.SubscribeAxis(guid, "y", new Handler(guid, "y"))
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