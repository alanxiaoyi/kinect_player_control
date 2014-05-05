kinect_player_control
=====================
Ok, here is my little Kinect program based on Kinect SDK 1.8. I also include kinect toolbox and the WPF solutions into the project for reference convenience. 

Kinect toolbox is a library having gesture recognition (among many other features). The kinect WPF viewer comes with the SDK.

Basically, my program can be used to control a vedio player by gestures. For example, swipe hands right is to fast forward the video, to left is to backward. Vertically swipe hand up is to pause/play. The basic step is like below:

1. Kinect toolbox helps me recognize the gesture (I add my own vertical gesture and modified the library)
2. Search all opened windows to find the Player's window by its name.
3. Send a virtual Key stroke to control it. For example, right arrow to fast forward, left for backward, whitespace for pause/play


Note, I am not proficient in Visial C# and WPF prgramming. The reference stuff may look kinda messy ( I must reference several things from different locations to make it work)
