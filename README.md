## Introduction
*InputManager* is a custom input manager for Unity that allows you to rebind keys at runtime and abstract input devices(through input configurations) for cross platform input.

### Features
- Very simple to implement. It has the same public methods and variables as Unity's **Input** class
- Allows you to customize key bindings at runtime
- Allows you to convert touch input to axes and buttons on mobile devices
- Allows you to bind script methods to various input events(e.g. when the user presses a button or key) through the inspector
- Run up to four input configurations at the same time for easy local co-op input handling
- Save the key bindings to a file, to PlayerPrefs or anywhere else by implementing a simple interface
- Seamless transition from keyboard to gamepad with the InputAdapter addon
- Works with the new UI system introduced in Unity 4.6

## Platforms
Compatible with Windows 7, Windows 8 Desktop, Windows 8 Store, Linux, Mac OSX, WebPlayer and Android(not tested on iOS but it might work). Requires the latest version of Unity.

## Getting Started
For detailed information on how to get started with this plugin visit the [Wiki](https://github.com/daemon3000/InputManager/wiki).

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

## Contributors

- [TheSniperFan](https://github.com/TheSniperFan)
- [reberzon](https://github.com/reberzon)
- [Ikillnukes](https://github.com/Ikillnukes)
