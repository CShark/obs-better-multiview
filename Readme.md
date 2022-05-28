# Better Multiview
An enhanced version of the OBS-Multiview with support for controlling more than just OBS. It allows arbitrary layouting your scenes in a custom grid and adding custom actions that will trigger when activating that scene.

Actions can also be grouped into sets and will only be triggered when entering or exiting that group of slots.

It is a standalone app that interfaces with OBS via the OBS-WebSocket Plugin.

![StreamView](https://github.com/CShark/obs-stream-deck/blob/master/Assets/StreamView.jpg)

## Requirements
OBS >= 27.0.0

OBS-WebSocket >= 4.9.1

## Setup
Currently there is no config UI for the OBS settings, so you have to enter the password, port & host of obs manually in the settings.json

## Removing the app
This multiview adds two scenes to your OBS: multiview and preview. Just delete them. The app will recreate them if necessary.

## Available Plugins
The available plugins come from personal use. Any ideas can be either suggested or added as a pull request.

### Keyboard
Supports Keyboard shortcuts with key suppression, numbers and optionally multiple Keyboards.

- Key supression: Allows you to cancel key propagation when the specified key is mapped, meaning mapped keys become unusable to avoid accidentally entering text in other apps when switching scenes
- Numbers: You can map scenes to numbers instead of keys. That way you can enter a number on your numpad and the corresponding scene will be activated. Numbers take precedence over normal shortcuts
- Multiple keyboards: You can map shortcuts to specific keys on a specific keyboard. Requires the [Oblitum Interception-Driver](https://github.com/oblitum/Interception) to be installed. Keyboards are assigned a guid by default which can be changed to a human readable label by clicking on it.

### QLC+ (DMX)
Supports manipulating DMX Devices through QLC+. Requires QLC+ to be launched with the `-w` parameter to enable the web-API.

This Plugin allows you to associate a scene with one or multiple functions or widgets from the virtual console which will be activated when the scene is activated and optionally reset when leaving the scene. For functions, any value other than zero will activate the function. For widgets it depends on the type of widget; buttons only support on/off, while sliders allow to set a value between 0 and 255.

### PELCO-D
A popular PTZ-protocol for cameras via RS485. Requires a RS485 dongle.

This plugin allows you to recall presets programmed in cameras that support the PelcoD-Protocol. Usually requires a PelcoD-Capable camera console to program the presets first as well as an RS485-dongle.

### KNX
Supports sending messages to a KNX/IP interface. Requires a KNX/IP interface in the local network.

This Plugin allows you to send arbitrary messages to a device on the KNX bus system. You have to configure available groups and their datapoint types by hand before using them, currently Datapoint 1.* (boolean) and 5.* (relative) are supported. You can then define actions for the entry and exit of a scene slot.