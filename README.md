# JoystickWrapper

###[AHK Forum Thread](https://autohotkey.com/boards/viewtopic.php?f=19&t=28889) (Code contributions, design input etc all very much welcome)

---

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

##How do I use it in AutoHotkey?
  1. Copy `JoystickWrapper.ahk` and all DLL files from the `JoystickWrapper` folder  
  Place them in the same folder as your script
  2. Include the JoystickWrapper library  
  ```#include JoystickWrapper.ahk```
  3. Instantiate AHK `JoystickWrapper` class, passing it the path to the DLL  
  ```jw := new JoystickWrapper("JoystickWrapper.dll")```
  4. You may now subscribe to inputs, eg using one of the API commands, eg:
  ```jw.SubscribeAxis(<some stick guid>, 1, Func("SomeFunc"))```
  
##What functions does it provide?
###Input Subscriptions
The subscription commands share some common parameters:  
guid - A GUID for a stick.  
This uniquely identifies a stick on your system, and should be constant across runs.  
You can find GUIDs using one of the Query Commands below.  

subscriber id - (Optional) Allows multiple subscriptions to the same input  
If you wish to have multiple subscriptions on the same input, you can call subscribe commands multiple times...  
... But each time, pass a different Subscriber ID. It can be any string.  
If omitted, "0" is used.

####Subscribe / Unsubscribe Axis  
    jw.SubscribeAxis(<stick guid>, <axis id>, <callback>[ ,<subscriber id>])
    jw.UnSubscribeAxis(<stick guid>, <axis id>, [ ,<subscriber id>])
axis id = 1 - 8  

####Subscribe / Unsubscribe Button  
    jw.SubscribeButton(<stick guid>, <button id>, <callback>[ ,<subscriber id>])
    jw.UnSubscribeButton(<stick guid>, <button id>, [ ,<subscriber id>])
button id = 1 - 128  

####Subscribe / Unsubscribe POV (D-Pad)  
    jw.SubscribePov(<stick guid>, <pov id>, <callback>[ ,<subscriber id>])
    jw.UnSubscribePov(<stick guid>, <pov id>, [ ,<subscriber id>])
pov id = 1 - 4  

####Subscribe / Unsubscribe POV Direction (Up / Down / Left / Right)  
    jw.SubscribePovDirection(<stick guid>, <pov id>, <pov direction>,<callback>[ ,<subscriber id>])
    jw.UnSubscribePovDirection(<stick guid>, <pov id>, <pov direction> ,[ ,<subscriber id>])

pov id = 1 - 4 
pov direction =  
1 : Up / North  
2: Right / East  
3: Down / South  
4: Left / Up  
Each direction has a tolerance of 90 degrees, so eg up + right will trigger both up and right.  

###Query Methods
####GetDevices
    jw.GetDevices()
Returns a list of devices, their names, guids, and capabilities.

####GetAnyDeviceGuid
Gets any guid that it can find. Useful to make demo scripts simple.
    guid := jw.GetAnyDeviceGuid()
    jw.SuscribeAxis(guid, ...

##Goals
~~striked items~~ are done
###Must Have
  * ~~100% Event-driven reporting of stick states (From the perspective of the AHK code)  
  ie you pass the wrapper a callback and a stick GUID, and it fires the callback only when it changes~~  
  * ~~Full 8 axis, 128 button, 4 POV support~~  
  * ~~Either native 0..65535 reporting, or a rescaled range that does not sit at an odd value~~  
  * ~~Ability to get names of sticks~~  

###Should Have
  * ~~Ability to subscribe to individual axes~~
  * ~~Ability to subscribe to individual buttons~~
  * ~~Ability to subscribe to individual POVs~~
  * ~~Ability to subscribe to individual POV *directions* like UCR currently does~~

###Could Have
  * Integration with Nefarius' upcoming [HidGuardian / HidCerberus](https://github.com/nefarius/ViGEm) system
    This would allow hiding of the phyical stick from any other application than JoystickWrapper  
    In this manner, true "remapping" of sticks could be achieved, in combination with Shaul's vJoy or Nefarius' [ViGEm](https://github.com/nefarius/ViGEm)
    
