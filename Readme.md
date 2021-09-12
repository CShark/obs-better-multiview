# Stream Deck
An enhanced version of the OBS-Multiview with support for controlling more than just OBS. It allows arbitrary layouting your scenes in a custom grid and adding custom actions that will trigger when activating that scene.

It is a standalone app that interfaces with OBS via the OBS-WebSocket Plugin.

## Requirements
OBS >= 27.0.0

OBS-WebSocket >= 4.9.1

## Setup
Currently there is no config UI for the OBS settings, so you have to enter the password, port & host of obs manually in the settings.json

## Removing the app
StreamDeck adds two scenes to your OBS: multiview and preview. Just delete them. StreamDeck will recreate them if necessary.

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

## Planned Plugins
### KNX
A popular home-automation system. Requires a KNX/IP interface.

### PELCO-D
A popular PTZ-protocol for cameras via RS485. Requires a RS485 dongle.