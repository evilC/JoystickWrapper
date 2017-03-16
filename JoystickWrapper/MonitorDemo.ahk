#SingleInstance force
#Persistent

Gui, +HwndhMain
Monitors := {}

;OutputDebug DBGVIEWCLEAR
#Include JoystickWrapper.ahk
global jw := new JoystickWrapper("JoystickWrapper.dll")

global DeviceList := jw.GetDevices()
DevicesLV := new WrappedLV(hMain, "w300", "Name|Guid")
DevicesLV.SetColumnWidth(1, 150)
for i, dev in DeviceList {
	DevicesLV.Add("", dev.Name, dev.Guid)
}
Gui, Add, Button, w300 gMonitorDevice, Monitor Selected Device (Can be repeated)
Gui, % hMain ":Show", x0 y0
return


MonitorDevice:
	selected_device := DevicesLV.GetCurrentText(2)
	if (!selected_device)
		return
	Monitors[guid] := new Monitor(DeviceList[selected_device])
	return

GuiClose(hwnd){
	global hMain
	if (hwnd == hMain){
		ExitApp
	}
}

class Monitor {
	static AxisList := ["X", "Y", "Z", "Rx", "Ry", "Rz", "S0", "S1"]
	static PovDirections := ["Up", "Right", "Down", "Left"]
	
	__New(dev){
		this.Device := dev
		Gui, New, hwndhGui
		this.hGui := hGui
		
		this.AxisLV := new WrappedLV(hGui, "w50 R8 AltSubmit Checked", "Axis", this.AxisLVEvent.Bind(this))
		this.ButtonLV := new WrappedLV(hGui, "x+5 yp w60 R8 AltSubmit Checked", "Button", this.ButtonLVEvent.Bind(this))
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
		Gui, Show, x0 y0, % dev.Name
	}
	
	AxisLVEvent(){
		if (A_GuiEvent != "I" || !InStr(ErrorLevel, "c"))
			return
		state := InStr(ErrorLevel, "C", true) ? 1 : 0
		row := A_EventInfo
		ax := this.Device.SupportedAxes[row]
		if (state)
			jw.SubscribeAxis(this.Device.Guid, ax, this.InputEvent.Bind(this, "Axis " this.AxisList[ax]), this.hGui)
		else
			jw.UnSubscribeAxis(this.Device.Guid, ax, this.hGui)
	}
	
	ButtonLVEvent(){
		if (A_GuiEvent != "I" || !InStr(ErrorLevel, "c"))
			return
		state := InStr(ErrorLevel, "C", true) ? 1 : 0
		row := A_EventInfo
		if (state)
			jw.SubscribeButton(this.Device.Guid, row, this.InputEvent.Bind(this, "Button " row), this.hGui)
		else
			jw.UnSubscribeButton(this.Device.Guid, row, this.hGui)
	}
	
	PovLVEvent(){
		if (A_GuiEvent != "I" || !InStr(ErrorLevel, "c"))
			return
		state := InStr(ErrorLevel, "C", true) ? 1 : 0
		row := A_EventInfo
		if (state)
			jw.SubscribePov(this.Device.Guid, row, this.InputEvent.Bind(this, "POV " row), this.hGui)
		else
			jw.UnSubscribePov(this.Device.Guid, row, this.hGui)
	}
	
	PovDirectionLVEvent(pov_id){
		if (A_GuiEvent != "I" || !InStr(ErrorLevel, "c"))
			return
		state := InStr(ErrorLevel, "C", true) ? 1 : 0
		row := A_EventInfo
		if (state)
			jw.SubscribePovDirection(this.Device.Guid, pov_id, row, this.InputEvent.Bind(this, "POV " pov_id ", Direction " this.PovDirections[row]), this.hGui)
		else
			jw.UnSubscribePovDirection(this.Device.Guid, pov_id, row, this.hGui)
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