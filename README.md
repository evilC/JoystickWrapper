# JoystickWrapper

##What?
A C# DLL wrapper using SharpDX, to replace AHK's joystick functionality

##Why?
I *love* AutoHotkey, but frankly it's joystick support sucks, for the following reasons:
  * Up events for Buttons fire immediately after the down event.  
  In order to detect held buttons, you need a `GetKeyState()` loop
  * Axes are not event-based.
  You need to use a `GetKeyState()` loop
  * POVs are not event-based.
  You guessed it, yet another `GetKeyState()` loop  
  * The axis scale is 0..100.
  At first this seems sensible and useful. Until you realize that centered sticks do not report as 50  
  Because the underlying range is 0..65535, with the mid-point being 32767, the conversion causes the mid-point to end up as 49.997  
  So deciding if you are at the mid-point (or, say, within a deadzone) becomes a case of floating-point comparisons and all their ambiguities.  
  * AHK joystick support is implemented using the WinMM API.
  So you only have access to 6 axes, 32 buttons, 1 POV  
  DirectX supports 8 axes, 128 buttons, 4 POVs.  
  So, for example, some of the axes on my Thrustmaster T.16000M / TWCS combo are unreadable using AHK.  
  Also, this means that all sticks must be referred to by ID number, and these IDs have a habit of changing.
  * Lexikos, the author of AutoHotkey_L, has expressed no interest in updating AHK's joystick support to something better.  

Lexikos has released some really great stuff recently, one of which being his [CLR Library](https://autohotkey.com/boards/viewtopic.php?f=6&t=4633) which allows AHK to interface with C# code.  
At the time of writing, I am starting a career in automated test using C#, so I thought it would be a good idea to work with it in my spare time.  
There is also a DirectX wrapper for C# called [SharpDX ](http://sharpdx.org/) which allows easy reading of sticks in C#, so over the course of a weeked I sat down and knocked up a POC for reading sticks in AHK via DirectX.  

##How?
  1. You load the C# JoystickWrapper DLL from AutoHotkey, using Lexikos' CLR library.  
  ```asm := CLR_LoadLibrary("JoystickWrapper.dll")```
  2. You then instantiate the `JoystickWrapper` class from the DLL, again using CLR  
  ```jw := asm.CreateInstance("JWNameSpace.JoystickWrapper")```

You can then call methods from the class as if `jw` were a regular AHK class.  
eg to call `JoystickWrapper.GetDevices()` from the DLL, just do `jw.GetDevices()`  
Return objects are COM objects, so be aware that:  
  * Arrays are zero-based.
  * To get the max index of an array, use `var.MaxIndex()` not the regular AHK `var.Length()`  

For more info, see the demo AHK script in the JoystickWrapper folder.  

##Goals
###Must Have
  * 100% Event-driven reporting of stick states (From the perspective of the AHK code)  
  ie you pass the wrapper a callback and a stick GUID, and it fires the callback only when it changes
  * Full 8 axis, 128 button, 4 POV support
  * Either native 0..65535 reporting, or a rescaled range that does not sit at an odd value  
  * Ability to get names of sticks  

###Should Have
  * Ability to subscribe to individual axes
  * Ability to subscribe to individual buttons
  * Ability to subscribe to individual POVs
  * Ability to subscribe to individual POV *directions* like UCR currently does

###Could Have
  * Integration with Nefarius' upcoming [HidGuardian / HidCerberus](https://github.com/nefarius/ViGEm) system
    This would allow hiding of the phyical stick from any other application than JoystickWrapper  
    In this manner, true "remapping" of sticks could be achieved, in combination with Shaul's vJoy or Nefarius' [ViGEm](https://github.com/nefarius/ViGEm)
    
