## Introduction
*InputManager* is a custom input manager for Unity that allows you to rebind keys at runtime and abstract input devices(through input configurations) for cross platform input.

### Features
- Very simple to implement. It has the same public methods and variables as Unity's **Input** class
- Allows you to customize key bindings at runtime
- Allows you to convert touch input to axes and buttons on mobile devices
- Allows you to bind script methods to various input events(e.g. when the user presses a button or key) through the inspector
- Save the key bindings to a file, to PlayerPrefs or anywhere else by implementing a simple interface
- Seamless transition from keyboard to gamepad with the InputAdapter addon
- Works with the new UI system introduced in Unity 4.6

## Platforms
Compatible with Windows 7, Windows 8 Desktop, Windows 8 Store, Linux, Mac OSX, WebPlayer and Android(not tested on iOS but it might work). Requires the latest version of Unity.

## Getting Started
For detailed information on how to get started with this plugin visit the [Wiki](https://github.com/daemon3000/InputManager/wiki/Getting-Started).

## API
The *InputManager* class provides the following methods and variables, in addition to the ones provided by Unity's *Input* class:

- **Instance** - Use it to check if a InputManager instance exists in the game or to subscribe to the event handlers
- **CurrentConfiguration** - A reference to the current input configuration used by the InputManager
- **IsScanning** - Use this to check if a scan is already running before starting a new one
- **AnyInput** - Returns *true* if any button or axis is used
- **SetRemoteAxisValue** - Allows you to manually set the value of an axis, if the respective axis is of type *RemoteAxis*
- **SetRemoteButtonValue** - Allows you to manually set the state of a button, if the respective button is of type *RemoteButton*
- **Reinitialize**
- **SetInputConfiguration** - Switch to another input configuration
- **GetInputConfiguration** - Gets a reference to an input configuration
- **CreateInputConfiguration** - Allows you to create input configurations at runtime
- **DeleteInputConfiguration** - Allows you to delete input configurations at runtime
- **GetAxisConfiguration** - Gets a reference to an axis configuration
- **CreateButton** - Allows you to create axis configurations at runtime
- **CreateDigitalAxis** - Allows you to create axis configurations at runtime
- **CreateMouseAxis** - Allows you to create axis configurations at runtime
- **CreateAnalogAxis** - Allows you to create axis configurations at runtime
- **CreateAnalogButton** - Allows you to create axis configurations at runtime
- **CreateRemoteAxis** - Allows you to create axis configurations at runtime
- **CreateRemoteButton** - Allows you to create axis configurations at runtime
- **CreateEmptyAxis** - Allows you to create axis configurations at runtime
- **DeleteAxisConfiguration** - Allows you to delete axis configurations at runtime
- **StartKeyScan** - Scans for keyboard keys, mouse buttons and joystick buttons
- **StartMouseAxisScan** - Scans for mouse axes. To change a mouse axis when you get the result from *StartMouseAxisScan* use *AxisConfiguraton.SetMouseAxis*
- **StartJoystickAxisScan** - Scans for joystick axes. To change a joystick axis when you get the result from *StartJoystickAxisScan* use *AxisConfiguration.SetAnalogAxis*
- **StartScan** - Scans for multiple input types(keyboard keys, joystick buttons, joystick axes, etc) at the same time
- **CancelScan**
- **Save** - Saves the input configurations to a file(XML by default). Create a class that implements *IInputSaver* for custom formats
- **Load** - Loads input configurations from a file(XML by default). Create a class that implements *IInputLoader* for custom formats

**Visit the Wiki for more scripting tutorials.**

## Addons
### Input Adapter
This addon manages keyboard and XBox controller input and features seamless transition between one and the other during play time.

### Joystick Mapping
This addon allows you to map joystick buttons to key codes and axes.

### UI Input Modules
Custom standalone input module for the new UI system introduced in Unity 4.6.

### Input Events
This addon allows you to bind script methods to various input events(e.g. when the user presses a button or key) through the inspector.

**For more information about the addons visit the [Wiki](https://github.com/daemon3000/InputManager/wiki ).**

## License
This software is released under the [MIT license](http://opensource.org/licenses/MIT). You can find a copy of the license in the LICENSE file included in the *InputManager* source distribution.
