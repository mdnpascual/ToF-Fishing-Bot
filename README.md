

# ToF Fishing Bot

## Version 2.0

A FIshing bot for Tower of Fantasy written in C# using OpenCVSharp and SharpDX for fast frame capture and analysis to determine how to move the fishing cursor and also automate the fishing process.

Fishing bot in action in youtube: https://youtu.be/MiCFCidkEVg
[![Fishing bot in action](https://i.ytimg.com/vi/MiCFCidkEVg/maxresdefault.jpg)](http://www.youtube.com/watch?v=MiCFCidkEVg "Fishing bot in action in youtube")

Quick image tutorial:
[![Settings up the Fishing bot](https://raw.githubusercontent.com/mdnpascual/ToF-Fishing-Bot/master/ToF_Fishing_Bot/img/tut.jpg)](https://raw.githubusercontent.com/mdnpascual/ToF-Fishing-Bot/master/ToF_Fishing_Bot/img/tut.jpg "Settings up the Fishing bot (TODO: CHANGE ME)")

# New Features in 2.0

- Keyboard Commands are sent on background. It means you can now do other things on the other monitor as long as the Fishing UI In-game is not blocked
- Zoom Feature when setting up points
- UI Overhaul to include Dark mode
- Lots of Settings exposed in `settings.json` (See Advanced Customization Below)

# Troubleshooting

### It does not run. Nothing is showing up

Download and Install: https://dotnet.microsoft.com/en-us/download/dotnet/6.0 (.NET Desktop Runtime >> x64)

### Clicking Start Fishing does nothing

- After clicking start fishing, you need to click the reel in button once yourself to start fishing. After that, the tool should automate by itself until you run out of bait.

### After clicking any of the buttons to setup the color/position, Clicking on the game won't lock/select the position color

- Run the tool as Administrator

### It says Game not found.

- Run the game first OR again, run the tool as Administrator

### Status stuck at NotFishing after setup and clicking Start Fishing. I can see it moving the cursor but doesn't catch the fish

- Make sure the lowest point of the Fish and Player stamina are properly selected (See tutorial).

### After setup and clicking start fishing, it does nothing, it doesn't show the 2 black lines at the bottom of the program as show in the video.

- Usually, this means that the middle bar rectangle bounds is setup wrong, seeing bad data. Re-do the middle bar setup

### It moves the cursor properly but doesn't catch the fish. It just stop the fishing process.

- Usually, this means you have changed the default keys. The default key to catch the fish is (1). If there's a different key below the reel-in icon, look for Advanced Customization; Button for Fish Capture section below on how you can customize your settings.

### The mouse just lags after opening the tool.

- Some other programs are interfering with the mouse/keyboard detection. I cannot help with this one unfortunately as I'm just using an external package to send/detect the mouse position.

# Advanced Customization

### This section explains how you can customize certain aspectos or behaviour of the tool.

After setting up the tool for the first time, you should see a `settings.json` file on the same folder of the executable. Open it, and add and entry if needed

## Button for Fish Capture
#### `"KeyCode_FishCapture": <keycode_intval>`
Example: ```  "KeyCode_FishCapture": "49"``` (default)

Where `keycode_intval` is the value of the keycode in integer. To set your custom hotkey to capture the fish, Please refer to: http://www.foreui.com/articles/Key_Code_Table.htm
In the example above, the value of 49 represent the Keyboard Input "1".

## Button to dismiss Fish Capture Dialogue
#### `"KeyCode_DismissFishDialogue": <keycode_intval>`
Example: ```  "KeyCode_DismissFishDialogue": "27"``` (default)

Where `keycode_intval` is the value of the keycode in integer. To set your custom hotkey to dismiss the fish capture dialogue, Please refer to: http://www.foreui.com/articles/Key_Code_Table.htm
In the example above, the value of 27 represent the Keyboard Input "Escape".

## Delay for Lag Compensation at start of fishing
#### `"Delay_LagCompensation": <delay_in_ms>`
Example: ```  "Delay_LagCompensation": "5000"``` (default)

Where `delay_in_ms` is the delay in millisecond to prevent the tool by reeling in immediatly after starting.
After starting to fish, there is a delay/lag associated before the fish stamina is filled. This is the delay to compensate for that.

## Delay before capturing fish after fish stamina detected has been depleted

#### `"Delay_FishCapture": <delay_in_ms>`
Example: ```  "Delay_FishCapture": "2000"``` (default)

Where `delay_in_ms` is the delay in millisecond before the tool presses the reel-in hotkey.
When the tool detects that the fish stamina is depleted, it will wait for this delay before capturing the fish.
If you notice that the tool sometimes pre-emptively catches a fish on higher stamina fish or alphas, then increasing this delay will force the tool to wait longer before capturing the fish.

## Delay before dismissing the Fish Capture screen
#### `"Delay_DismissFishCaptureDialogue": <delay_in_ms>`
Example: ```  "Delay_DismissFishCaptureDialogue": "2000"``` (default)

Where `delay_in_ms` is the delay in millisecond before the tool presses the ESC Key after capturing the fish.

## Delay before restarting the process again
#### `"Delay_Restart": <delay_in_ms>`
Example: ```  "Delay_Restart": "2000"``` (default)

Where `delay_in_ms` is the delay in millisecond before the tool restarts the fishing process again after pressing the ESC Key.

## Minimum height on middle bar
#### `"MinimumMiddleBarHeight": <pixel_difference>`
Example: ```  "MinimumMiddleBarHeight": "5"``` (default)

Where `pixel_difference` is the Y difference that the program will allow before it tries to detect the middle bar.
This is people who run the game at lower resolutions who wants to attempt and see if the tool will work for them.

Note: The tool is only tested to work at the minimum of 5 pixel height. Setting it lower might make it work but doesn't guarantee it.

## Zoom window size
#### `"ZoomSize_X": <size_horizontal>`
#### `"ZoomSize_Y": <size_vertical>`
Example: ```  "ZoomSize_X": "300"``` (default)
Example: ```  "ZoomSize_Y": "300"``` (default)

Where `size_horizontal` and `size_vertical` is the windows size in pixels of the zoom when clicking a buttong to setup the points.

## Zoom Factor
#### `"ZoomFactor": <zoom_strength>`
Example: ```  "ZoomFactor": "4"``` (default)

Where higher values of `zoom_strength` increases the amount of zoom that you see below the cursor

## Game Process name
#### `"GameProcessName": <exe_name>`
Example: ```  "GameProcessName": "QRSL"``` (default)

Where `exe_name` is the main executable for the game
This is where if you want to use this tool to other games or region.
For example, if you want to use this on the CN version, you might want to replace `exe_name` to `WmgpMobileGame`

## Stamina Color Threshold
#### `"StaminaColorDetectionThreshold": <double_val>`
Example: ```  "StaminaColorDetectionThreshold": "40.0"``` (default)

Where `double_val` is a the amount of variance that the game will accept as the correct color for the fish or player stamina.
This value are the 2 changing values at the top of the "Start/Stop Fishing" Button.
Lower values means a stricter check but more false negatives. Higher values means less strict but more false positive.

## Middle Bar Color Threshold
#### `"MiddlebarColorDetectionThreshold": <double_val>`
Example: ```  "MiddlebarColorDetectionThreshold": "10.0"``` (default)

Where `double_val` is a the amount of variance that the game will accept as the correct color for the middle bar.
Lower values means a stricter check but more false negatives. Higher values means less strict but more false positive.