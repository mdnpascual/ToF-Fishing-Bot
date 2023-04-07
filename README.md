# Version 2.0 In-Progress

## TODOS:

- Removal of Mouse outputs (Use default Hotkey(1) to catch fish and Esc to dismiss Success Dialog)
- Change UI to remove Reel in button and close button
- ~~Customize hotkey button~~ (future 2.X update)
- Add more sanity checks (Check Minimum Height)
- ~~Lock setup buttons~~ (future 2.X update)
- Send all outputs in the background (PostMessage instead of SendInput)

# ToF Fishing Bot

A FIshing bot for Tower of Fantasy written in C# using OpenCVSharp and SharpDX for fast frame capture and analysis to determine how to move the fishing cursor and also automate the fishing process.

Fishing bot in action in youtube: https://youtu.be/67-FUUfhDqY
[![Fishing bot in action](https://img.youtube.com/vi/67-FUUfhDqY/maxresdefault.jpg)](http://www.youtube.com/watch?v=67-FUUfhDqY "Fishing bot in action in youtube")

Tutorial on how to setup in youtube: https://youtu.be/cnyKoxRe9cA
[![Settings up the Fishing bot](https://img.youtube.com/vi/cnyKoxRe9cA/maxresdefault.jpg)](http://www.youtube.com/watch?v=cnyKoxRe9cA "Settings up the Fishing bot")

# Troubleshooting

### Clicking Start Fishing does nothing

- After clicking start fishing, you need to click the reel in button once yourself to start fishing. After that, the tool should automate by itself until you run out of bait.

### After clicking any of the buttons to setup the color/position, Clicking on the game won't lock/select the position color

- Run the tool as Administrator

### After clicking "Start Fishing" it seems to detect the fishing UI and trying to move the fishing cursor but doesn't do the action in game.

- Make sure that after clicking Start Fishing, click back on the game. Also again, run the tool as Administrator

### Status stuck at NotFishing after setup and clicking Start Fishing. I can see it moving the cursor but doesn't catch the fish

- Make sure the Fish stamina and Player stamina are properly selected. They are important to be properly setup because those are the things the program needs to know when to click the reel in button and resetting the cycle. To check, the button for selecting fish and player stamina has a border color. If the game is showing the fish and player stamina UI, check if the border is showing green or red. Fix the one where it showing the red border

### After setup and clicking start fishing, it does nothing, it doesn't show the 2 black lines at the bottom of the program as show in the video.

- Usually, this means that the middle bar rectangle bounds is setup wrong, seeing bad data

### The mouse just lags after opening the tool.

- Some other programs are interfering with the mouse/keyboard detection. I cannot help with this one unfortunately as I'm just using an external package to send/detect the mouse position.
