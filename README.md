# VRUE
Names: 
- Selina Breuer
	- Matrklnr: 12019862 
- Ye-Ryun Kim 
	- Matrklnr: 01604987



Hardware:
- Oculus (via Android Built)
- Meta Quest 3 (via Windows Built)

Software:
- Unity: 2021.3.44f1
- Multiplayer Framework: PUN


# Sources
Listed are sources we used and every chatGPT prompt associated with Assignment3

- https://www.youtube.com/watch?v=Eipi6rNPz9U
- https://www.youtube.com/watch?v=-6vw7W7titM
- https://chatgpt.com/share/67391f38-d1b0-8009-9074-bf74bb02339e
- https://chatgpt.com/share/67391f75-0ea8-8009-93dd-3e7bd69e6b73
- https://chatgpt.com/share/67391fbd-4490-8009-9a19-8ee402119686


# Texture Resources 
- https://www.freepik.com/free-vector/simple-wood-texture_1016488.htm
- https://www.sharetextures.com


# Notes regarding implementation of Tasks

2) 
- Locomotion is a simple running and jumping gesture.
- Swing arms close to and infront of your body in opposite directions to run in the direction you face/look
- Horizontal swings as well as arm movement outside of your immediate front will not move you forward.
- Jumping is implemented by moving your arms in the same up/down direction within you immediate front.
- Speed and jumping force is determined by your hand movement
- Right controller primary button will recalibrate to face the goal
- Right controller secondary button will turn you 180° backwards
- Movement is physics based with smooth acceleration and controlled stop if both hands have stopped moving

4) 



# Changelog

- Moved Canvas lookup from update() loop to start() in Character.cs

- Added Goal script to XR origin that implements ontriggerEnter while leaving character.cs intact
	- issue was, character.cs does not have a rigid body, thus ontriggerenter would not work. instead implemnt ontriggerenter (collider goal) in XROrigin, which is synched to character transform anyways

- Added Rayinteractor to right hand for UI

- Disabled ui after countdown (otherwise Rayinteractor will keep having an annoying ray)
- Reenabled UI after goal

- Fixed XROrigin Spawnpoint not sychronized with Spawned Player location on roomjoin

- Start with free movement before countdown triggered, invisible startline collider disabled after countdown instead

- Fixed Goal Synchronized with XROrigin onTrigger collision

- fixed player movement & speed, allowing for stopping and faster acceleration

- playtested finish screen on both players

- narrowed down movement allowance. Only moving if : 
	- Hands Swinging right in front of body
	- Hands swingin vertically and in opposite directions# RowTogether_VR
