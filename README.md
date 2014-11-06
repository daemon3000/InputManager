## Introduction
*InputManager* is a custom input manager for Unity. It does everything Unity's input manager does and it allows you remap keys at runtime.

The *InputManager* can have one or more input configurations(e.g. one for keyboard and mouse and one for a Xbox controller), each one with its own set of axes(like the ones provided by Unity's input manager).

## Getting Started
### Setup
1. Open the InputManager project, export the folder named "InputManager" as a unitypackage and import it in your project. You don't need to export the "Examples" folder if you don't want to.
2. Create a new *Input Manager* by going to *Team Utility/Input Manager* and selecting *Create Input Manager*.
3. Open the *Advanced Editor* by going to *Team Utility/Input Manager* and selection *Open Advanced Editor*. The first time you open the *Advanced Editor* you will be prompted to overwrite your project's input settings. You can also do it from the *File* menu of the *Advanced Editor* at a later time.
4. Use the *Advanced Editor* to create new input configurations, buttons and axes.
5. If this isn't a new project open it in MonoDevelop(or any IDE you use) and replace all calls to the *Input* class with calls to the *InputManager* class. *InputManager* provides the same public methods and variables so it should be as simple as doing a *Find and Replace*.

**It is recommended that you have only one *Input Manager* in your game. Add it in the first scene and enable *Dont Destroy On Load* in the inspector.** 

### Advanced Editor
Use the editor to create input configurations and axes. 

The *Edit* menu contains options to create, delete and duplicate input configurations or individual axes.

The *File* menu allows you to create snapshots of the *InputManager* and restore it when you exit play-mode.

Note: The names you'll use for the *(Alt)Positive* and *(Alt)Negative* keys are not the same as the ones you use with the default input manager; they are the names of the [KeyCode](https://docs.unity3d.com/Documentation/ScriptReference/KeyCode.html) values. The names are case insensitive.
For example, instead of "left shift" you would use "LeftShift".

### API
The *InputManager* class provides the following methods and variables, in addition to the ones provided by Unity's *Input* class:

- **Instance** - Use it to check if a InputManager instance exists in the scene or to subscribe to the event handlers
- **CurrentConfiguration** - A reference to the current input configuration used by the InputManager
- **InputConfigurations** - A list of all the input configurations
- **AnyInput** - Returns *true* if any button or axis is used
- **Reinitialize**
- **CreateInputConfiguration** - Allows you to create input configurations at runtime
- **DeleteInputConfiguration** - Allows you to delete input configurations at runtime
- **SetConfiguration** - Switch to another input configuration
- **GetConfiguration** - Gets a reference to an input configuration
- **GetConfigurationNames** - Returns an array with the names of all input configurations. A new array is created every time you call this method
- **CreateButton** - Allows you to create axis configurations at runtime
- **CreateDigitalAxis** - Allows you to create axis configurations at runtime
- **CreateMouseAxis** - Allows you to create axis configurations at runtime
- **CreateAnalogAxis** - Allows you to create axis configurations at runtime
- **CreateEmptyAxis** - Allows you to create axis configurations at runtime
- **DeleteAxisConfiguration** - Allows you to delete axis configurations at runtime
- **GetAxisConfiguration** - Gets a reference to an axis configuration
- **StartKeyScan** - Calls the scan handler when a key is pressed. Use this to remap digital axes
- **StartMouseAxisScan** - To change a mouse axis when you get the result from *StartMouseAxisScan* use *AxisConfiguraton.SetMouseAxis*
- **StartJoystickAxisScan** - To change a joystick axis when you get the result from *StartJoystickAxisScan* use *AxisConfiguration.SetAnalogAxis*
- **CancelScan**
- **Save** - Saves the input configurations to a file(XML by default). Create a class that implements *IInputSaver* for custom formats
- **Load** - Loads input configurations from a file(XML by default). Create a class that implements *IInputLoader* for custom formats

**Visit the Wiki for more scripting tutorials.**

## Addons
### Input Adapter
This addon manages keyboard and XBox controller input and features seamless transition between one and the other during play time.

### Joystick Mapping
This addon allows you to map joystick buttons to key codes and axes.

**For more information about the addons visit the Wiki.**

## Platforms
Compatible with Windows 7, Windows 8 Desktop, Windows 8 Store, Linux, Mac OSX and WebPlayer. Only for Unity 4.

## License
This software is released under the [MIT license](http://opensource.org/licenses/MIT). You can find a copy of the license in the LICENSE file included in the *InputManager* source distribution.
