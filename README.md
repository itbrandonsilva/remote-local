# RLocal

So what is it...

RLocal provides a way to play classic games (or modern games that don't support multiplayer) over a network in real time. A host will run a RLocal server to broadcast video/audio to clients. Clients would use RLocal to connect to the RLocal host to subscribe to the video/audio feed, while also forwarding Gamepad/Keyboard inputs to the host.

Many more necessary features/functionality in the works:

* Gamepad/Keyboard configuration
* Volume Control
* Wasapi Audio Capture
* Fullscreen Game Capture
* Performance Improvements
* Graceful Error Handling

### Requirements:

* .NET Framework 4.5.2
* Windows 7 SP1 (and beyond)
* DirectX 11
* VC++ 2015 Redistributable

### The Goods:

 * [NAudio](https://github.com/naudio/NAudio)
 * [SharpDX](https://github.com/sharpdx/SharpDX)
 * [vJoy](https://github.com/shauleiz/vJoy)
 * [ffmpeg](http://ffmpeg.org/)
