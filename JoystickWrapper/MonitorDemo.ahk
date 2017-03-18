#SingleInstance force
#Persistent

Gui, +HwndhMain
Monitors := {}

;OutputDebug DBGVIEWCLEAR
#Include JoystickWrapper.ahk
global jw := new JoystickWrapper("JoystickWrapper.dll")

; DirectInput
global DeviceList := jw.GetDevices()
Gui, Add, Text, w300 Center, DirectInput Devices
DevicesLV := new WrappedLV(hMain, "w300", "Name|Guid")
DevicesLV.SetColumnWidth(1, 150)
for i, dev in DeviceList {
	DevicesLV.Add("", dev.Name, dev.Guid)
}
Gui, Add, Button, w300 gMonitorDevice, Monitor Selected Device (Can be repeated)

; XInput
Gui, Add, Text, w300 y+20 Center, XInput Devices
global XinputDeviceList := jw.GetXInputDevices()
XinputDevicesLV := new WrappedLV(hMain, "w300", "Name|ID")
XinputDevicesLV.SetColumnWidth(1, 150)
for i, dev in XinputDeviceList {
	XinputDevicesLV.Add("", dev.Name, dev.Guid)
}
Gui, Add, Button, w300 gMonitorXinputDevice, Monitor Selected Device (Can be repeated)

Gui, % hMain ":Show"
return


MonitorDevice:
	selected_device := DevicesLV.GetCurrentText(2)
	if (!selected_device)
		return
	Monitors[guid] := new Monitor(DeviceList[selected_device])
	return

MonitorXinputDevice:
	selected_device := XinputDevicesLV.GetCurrentText(2)
	if (!selected_device)
		return
	Monitors[guid] := new Monitor(XinputDeviceList[selected_device], "Xbox")
	return

GuiClose(hwnd){
	global hMain
	if (hwnd == hMain){
		ExitApp
	} else {
		Monitors.Delete(hwnd)
	}
}

^Esc::
	ExitApp

class Monitor {
	static AxisList := ["X", "Y", "Z", "Rx", "Ry", "Rz", "S0", "S1"]
	static PovDirections := ["Up", "Right", "Down", "Left"]
	
	__New(dev, input_type := ""){
		this.InputType := input_type	; For now, input type for DirectInput is "", because only xinput api calls have xinput in name
		this.DisplayType := (input_type = "" ? "DirectInput" : "XInput")
		this.Device := dev
		Gui, New, hwndhGui
		this.hGui := hGui
		
		this.AxisLV := new WrappedLV(hGui, "w50 R8 AltSubmit Checked", "Axis", this.AxisLVEvent.Bind(this))
		this.ButtonLV := new WrappedLV(hGui, "x+5 yp w60 R8 AltSubmit Checked", "Button", this.ButtonLVEvent.Bind(this))
		if (this.InputType == "")
			this.PovLV := new WrappedLV(hGui, "x+5 w50 R4 AltSubmit Checked", "Pov", this.PovLVEvent.Bind(this))
		this.PovDirectionLVs := []
		
		Loop % dev.POVs {
			pov := A_Index
			this.PovDirectionLVs.Push(new WrappedLV(hGui, "x+5 yp w60 R4 AltSubmit Checked", "Pov #" pov, this.PovDirectionLVEvent.Bind(this, pov)))
			Loop 4 {
				this.PovDirectionLVs[pov].Add(, this.PovDirections[A_Index])
			}
		}
				
		this.ReportLV := new WrappedLV(hGui, "xm w" 170 + (dev.Povs * 65), "Input|Value")
		this.ReportLV.SetColumnWidth(1, 150)
		for i, sa in dev.SupportedAxes{
			ax := this.AxisList[sa]
			this.AxisLV.Add("", ax)
		}
		
		Loop % dev.Buttons {
			this.ButtonLV.Add(, A_Index)
		}
		
		Loop % dev.POVs {
			this.PovLV.Add(, A_Index)
		}
		title := this.DisplayType == "DirectInput" ? dev.name : "ID " dev.Guid
		Gui, Show, , % "(" this.DisplayType ") " title
	}
	
	AxisLVEvent(){
		if (A_GuiEvent != "I" || !InStr(ErrorLevel, "c"))
			return
		state := InStr(ErrorLevel, "C", true) ? 1 : 0
		row := A_EventInfo
		ax := this.Device.SupportedAxes[row]
		if (state)
			jw["Subscribe" this.InputType "Axis"](this.Device.Guid, ax, this.InputEvent.Bind(this, "Axis " this.AxisList[ax]), this.hGui)
		else
			jw["UnSubscribe" this.InputType "Axis"](this.Device.Guid, ax, this.hGui)
	}
	
	ButtonLVEvent(){
		if (A_GuiEvent != "I" || !InStr(ErrorLevel, "c"))
			return
		state := InStr(ErrorLevel, "C", true) ? 1 : 0
		row := A_EventInfo
		if (state)
			jw["Subscribe" this.InputType "Button"](this.Device.Guid, row, this.InputEvent.Bind(this, "Button " row), this.hGui)
		else
			jw["Subscribe" this.InputType "Button"](this.Device.Guid, row, this.hGui)
	}
	
	PovLVEvent(){
		if (A_GuiEvent != "I" || !InStr(ErrorLevel, "c"))
			return
		state := InStr(ErrorLevel, "C", true) ? 1 : 0
		row := A_EventInfo
		if (state)
			jw["Subscribe" this.InputType "Pov"](this.Device.Guid, row, this.InputEvent.Bind(this, "POV " row), this.hGui)
		else
			jw["Subscribe" this.InputType "Pov"](this.Device.Guid, row, this.hGui)
	}
	
	PovDirectionLVEvent(pov_id){
		if (A_GuiEvent != "I" || !InStr(ErrorLevel, "c"))
			return
		state := InStr(ErrorLevel, "C", true) ? 1 : 0
		row := A_EventInfo
		if (state)
			ret := jw["Subscribe" this.InputType "PovDirection"](this.Device.Guid+0, pov_id, row, this.InputEvent.Bind(this, "POV " pov_id ", Direction " this.PovDirections[row]), this.hGui)
		else
			jw["Subscribe" this.InputType "PovDirection"](this.Device.Guid+0, pov_id, row, this.hGui)
	}
	
	InputEvent(input, value){
		this.ReportLV.Add(, input, value)
		this.ReportLV.ScrollToEnd()
	}
}

class WrappedLV {
	LastAddedRow := 0

	__New(hGui, options := "", columns := "", callback := 0){
		this.hGui := hGui
		this.Callback := callbacl
		Gui, % hGui ":Add", ListView, % "hwndhwnd " options, % columns
		this.hLV := hwnd
		this.SelectLV()
		if (callback != 0)
			GuiControl, +g, % this.hLV, % callback
	}
	
	Add(options := "", aParams*){
		this.SelectLV()
		this.LastAddedRow := LV_Add(options, aParams*)
	}
	
	SelectLV(){
		Gui, % this.hGui ":Default"
		Gui, ListView, % this.hLV
	}
	
	SetColumnWidth(col, width){
		this.SelectLV()
		LV_ModifyCol(col, width)
	}
	
	AutoWidth(){
		this.SelectLV()
		LV_ModifyCol()
	}
	
	ScrollToEnd(){
		if (this.LastAddedRow){
			this.SelectLV()
			LV_Modify(this.LastAddedRow, "Vis")
		}
	}
	
	GetCurrentRow(){
		this.SelectLV()
		return LV_GetNext()
	}
	
	GetCurrentText(col := ""){
		this.SelectLV()
		LV_GetText(ret, this.GetCurrentRow(), col)
		return ret
	}
}