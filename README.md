# ROW TOGETHER - Group 2
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
- https://www.youtube.com/watch?v=_QilKZ1f5Vo
- https://www.youtube.com/watch?v=xRXOnuFji-Q
- https://www.youtube.com/watch?v=RGb0_o691jo
	- photon ipunobservable and sendrate + serializable rate
- https://www.youtube.com/watch?v=vbeAuIH-4WA
	- photon animation view
- https://www.youtube.com/watch?v=Pp0UMchtFyo
	- photon voice2
- https://chatgpt.com/share/6744feed-f73c-8009-a953-46b2c6b72a67
- https://assetstore.unity.com/packages/tools/lexic-a-procedural-name-generator-33221

# Notes regarding implementation of Tasks

- Boat setup : 
	- The boat is a networked scene object initialized and controlled only by the master client. Having its own photon view, it syncs all child objects in relation to the boats movement, which is inherent to unity's parent/child hirarchy
	- All networked players with their view transform will, onJoinRoom, become a child of that boat, both in local game as well as in remote instances \(through rpc calls\)
	- Networked players local transforms will be synched, resulting in robust and absolute correct synch of players in every local and remote instance, when moving the boat. Meaning no rubber banding \(checkout issue described under Character.cs MapPosition() function\) regardless of master or non master client.
	- The boat can only be controlled by the master client and is also properly synced considereing water waves, etc...

- Paddle sync : <br>
	- Due to how the boat is setup, paddles are also a child of the boat, allowing them to easily move along the boat with 0 network issues.
	- However, to sync the paddles on grab and rotation, the paddles would also need their own Photon View, which results in a nested networked object.
	- However, with the default XRGrabInteractable component, when grabbing an object, the parent-child hirarchy will be temporarly destroyed with the object becoming a child of the root hirarchy. This issue rersults in the network sync being unreliable as photon view transforms would need to be applied on the global world transform of the object, instead of the local transforms, resulting in rubber banding and teleporting.
	- The solution is to overwrite the grab function of XRGrabInteractable to prevent unparenting, such that the paddles remain a child of the boat with local transforms applied on the photon view transform for robust transform sync.
	- Despite Photon being able to effortlessly sync even nested networked objects, there seems to be a bug if the grabable's movement type is set to anything but instanteneous. The object still remains grabable with correct ownership transfer, however, in the local instance of non master clients only, grabbing such objects results in some kind of network synch overload of that object with movement in local and remote instances seemingly waiting on responses between master & non master clients network synch. This results in extremely 1fps like choppiness in movement even on local instances.
	- This issue only occurs in nested networked objects, as unlinking the paddle from the parent boat on grab, the described scenario will not occur.
	- The only solution found for keeping the parent child hirarchy for robust transform synch while allowing grabbing and synching with paddles own photon view \(meaning nested networked object\) is to change the grabable movement type from kinematic/velocity to instanteneous
		- The implications of that change means: no collision of the paddle with other game objects possible

- Different Methods implemented for onNewPlayer joined room synchronization:
	- RpcTarget.Others
		- Sync triggered by something.
		- Good for sync with all players currently in room.
		- Bad for new players joining
		- Issue: timing related if rpc call on PlayerEnteredRoom, as new players still missing photonNetwork initialization
	- RpcTarget.AllBuffered
		- Good to replay all buffered rpc calls to new players.
		- Issue: timing related, same as above, if master client dced, loses all buffers
	- CustomRoomProperties:
		- Define gamestate in room properties. On player join, manually fetch room properties and manually set settings accordingly
		- Good agains timing issues, as self managed
		- Issue: A lot of hassle and delay in room prop update
	- RPC Request to master for sync
		- After joining and Network initialization finished, make RPC call RpcTarget.MasterClient with params Player PhotonNetwork.LocalPlayer, to issue master client rpc to new target player
		- Master client then syncs his game state directly with the new player.

# Changelog
13.12.24
- refactored UIManager to only manage ui
	- Ui manager no longer has access to gameplaymanager. 
	- on button listeners are added via gameplaymanager passing correct methods
	- on button click will trigger the correct gameplay features
- refactored Gameplaymanager to manage game state, game play, required UI change triggers
	- gameplay manager has access to uimanager. UI changes can be triggered. 
	- reason for this: triggers and colliders all implemented in gameplay. On triggers need to change ui
- Moved most/all PunRPC calls to boatmanager (prob should be called boatnetworkedmanager)
	- network related all aggregated for easier overview and access
	- has access to gameplay manager and therefore access to uimanager if needed.
- sync boat constraints to existing and new players upon join.
	- Having constraints on all non-master clients is very benefitial, despite movement of boat sync regardless.
	- Fixed edgecase of master leaving, but new master having no boat constraints before game starts.
- Fixed Gamereset not resetting correctly when masterswitched.
	- Use existing ship start location gameobject in scene instead of setting initial pos/rot of ship. Edgecase of switched master resetting boat into middle of track prevented.	
	- Fixed sync of game reset not sync lives, livesUI, boat constraints constraints
- Fixed canvas UI desync for new players upon join
	- the start ui is implemented via routine and countdown.
	- If the countdown starts and a new player/observer joins, the UI will never update.
	- Fixed by implementing master client UI request upon new player join. If the UI text displaying anything but default master client text, start routine to fetch current master client canvas text until hitting ""
- Fixed Trees becoming grey on pc build
	- changed tree bard material to required Nature/soft occlusion
	- Increased trees billboard start distance
- Added OpenXR HTC Vive controller support
13.12.24
- fixed heartItems synced via photon views
	- multiple heartItems can be spawned by dragging prefab into scene.
	- simply give photonview an id between 200 - 210
	- all calculation and triggers for potential added heartItems have been implemented already. see heartPhotonViewIDs list in gameplaymanager
- fixed gameover sync
	- applied implementations considering various edgecases for masterclient switch, late join or in room game over triggers.
- fixed goal sync
	- applied implementations considering various edgeaces for masterclient siwtch, late join or in room game over triggers.
- added support for grabbing paddle out of hand of other player