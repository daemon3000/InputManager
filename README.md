## Introduction
*InputManager* is a custom input manager for Unity that allows you to rebind keys at runtime and abstract input devices for cross platform input.

### Features
- Very simple to implement. It has the same public methods and variables as Unity's **Input** class.
- Allows you to customize key bindings at runtime.
- Allows you to use XInput for better controller support.
- Allows you to convert touch input to axes and buttons on mobile devices.
- Allows you to bind script methods to various input events(e.g. when the user presses a button or key) through the inspector.
- Run up to four input configurations at the same time for easy local co-op input handling.
- Save the key bindings to a file, to PlayerPrefs or anywhere else by implementing a simple interface.
- Seamless transition from keyboard to gamepad with multiple bindings per input action.
- Standardized gamepad input. Gamepad profiles map various controllers to a standard set of buttons and axes.

## Platforms
Compatible with Windows Desktop, Windows Store, Linux, Mac OSX and Android(not tested on iOS but it probably works). Requires the latest version of Unity.

## Getting Started
For detailed information on how to get started with this plugin visit the [Wiki](https://github.com/daemon3000/InputManager/wiki/Getting-Started) or watch the video tutorial linked below.

[![Unity - Custom Input Manager Setup Tutorial](https://i.imgur.com/8axnwkG.png)](https://www.youtube.com/watch?v=F_ZIxBPU3Vs)

## Addons
### XInputDotNet
This addon allows you to use [XInput](https://github.com/speps/XInputDotNet) for controller support instead of the Unity input system. **Only available on Windows platforms.**

### UI Input Module
Custom standalone input module for the UI system introduced in Unity 4.6.

### Input Events
This addon allows you to bind script methods to various input events(e.g. when the user presses a button or key) through the inspector.

**For more information about the addons visit the [Wiki](https://github.com/daemon3000/InputManager/wiki).**

## License
This software is released under the [MIT license](http://opensource.org/licenses/MIT). You can find a copy of the license in the LICENSE file included in the *InputManager* source distribution.

## Contributors

- [TheSniperFan](https://github.com/TheSniperFan)
- [reberzon](https://github.com/reberzon)
- [Ikillnukes](https://github.com/Ikillnukes)
- Felipe Mioto
- Zhialus
