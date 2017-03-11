; Include Lexikos' CLR library
#include CLR.ahk

class JoystickWrapper {
	__New(dllpath){
		this.DllPath := dllpath
		; Load the C# DLL
		asm := CLR_LoadLibrary(dllpath)
		; Use CLR to instantiate a class from within the DLL
		this.Interface := asm.CreateInstance("JWNameSpace.JoystickWrapper")
	}
	
	SubscribeAxis(guid, index, callback){
		this.Interface.SubscribeAxis(guid, index, callback)
	}
	
	UnSubscribeAxis(guid, index){
		this.Interface.UnSubscribeAxis(guid, index)
	}
	
	SubscribeButton(guid, index, callback){
		this.Interface.SubscribeButton(guid, index, callback)
	}

	UnSubscribeButton(guid, index){
		this.Interface.UnSubscribeButton(guid, index)
	}
	
	SubscribePov(guid, index, callback){
		this.Interface.SubscribePov(guid, index, callback)
	}

	UnSubscribePov(guid, index){
		this.Interface.UnSubscribePov(guid, index)
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
}