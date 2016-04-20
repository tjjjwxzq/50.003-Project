# 50.003 Final Project Report

## Introduction
Are you feeling hungry? Well, your mouse is!
Enter the world of **Yay A Mouse** and let’s feed your starving mouse! Your mouse, for some reason, has an insatiable appetite, so as the responsible owner, you have to keep feeding it. The twist? Other players are also trying to fatten their pet mice so you got to keep up to make it as fat as possible the fastest you can to win. Harness the special abilities you can use during the game, either to aid yourself or to sabotage other players. There is a catch, however, that different foods have varying benefits or detriments, and when eaten in the right order, you can trigger a food combination and subsequently a frenzy mode that rewards bonus growth rate, accelerating your path to victory. With so many things that can go well (or wrong), this game is bound to keep you engaged challenging your friends and even strangers in this ferocious feeding frenzy!

## System Requirements
The main mechanics of our game are as follows:
1. Swiping Food and Mouse Weight
2. Player Abilities and Mouse Happiness
4. Food Combos and Frenzy Mode

### Swiping Food and Mouse Weight
With your mouse in the center of the screen, random treats and junk food will be moving across the screen. To feed your mouse, you have to swipe the food to your mouse with your finger, and to make sure your mouse doesn't ingest bad food, you have to swipe them away.

When you feed your mouse, it gains weight, which is effectively your score. It will lose weight from ingesting junk food.

### Player Abilities and Mouse Happiness
Every player has some tricks (special abilities) under their sleeves - either sabotaging other players in various ways (eg. sending over a cat to scare off an opponent’s mouse, causing a sudden food drought) or boosting their own pet mouse (eg. immunity to bad food for a certain period of time, generating a deluge of treats).

Player abilities can only be triggered when their mouse has enough happiness (shown in the happiness bar on the left). Happiness can be increased by stroking your mouse, and is depleted when abilities are used.

### Food Combos and Frenzy Mode
Building on top of the core swiping mechanic, there will be a combo food feature. A specific three-food combo (that will change randomly across the duration of the game) will be displayed at the top of the screen. If the player manages to feed food to his/her mouse in that particular order, there will be a score bonus. For every 10 combos achieved (not necessarily consecutively), the player will enter a frenzy feeding mode for a period of time where he/she can feed the mouse by tapping rapidly on the mouse which will shoot food directly at it.

## System Design
### Overview
We used Unity to build our game, and the basic concept in Unity is that of Game Objects and components. Game Objects are given functionality by attaching various components to them. For example, for physics behavior and collision detection one attaches Rigidbody2D and Collider2D components. For custom behavior, one attaches a custom Script component to the Game Object; these scripts inherit from Unity's build in Monobehavior class which have functions that are called at the start of Game Object activation and during every event loop.

Aside from custom Script components, a significant portion of functionality in Unity is done by manual positioning (of the UI, for example, in the Scene view) or assignment in the editor (for example, the binding of buttons to event handler functions).

Unity also compartmentalizes the game into different scenes which have their own set of Game Objects. These scenes are mostly logically separate (eg. Lobby scene versus Main play scene) and so the discussion of our system design will be split according to the different scenes and their Game Objects and Script components.

At a high level, we have the Start/Lobby scene, from which the player can choose to host or join a multiplayer game, an Ability Selection scene, where players get to choose their two starting abilities before the main game begins, and the Main scene, which is where the real game happens.

Here is an overview of the components in our game and the functionalities they implement: (insert fancy diagram here)

Each of these components will be covered in greater detail in the subsequent sections.

### Networking Overview
Shun Yu and Jia Yu can add your stuffs here?
talk about how unity networking works (client/server objects, local player) How is state synchronized (SyncVars, Commands, ClientRPCs etc)


### Start, Lobby, Info, Settings and Name Prompt Scenes
(insert fancy diagrams showing the various scnese and their compoenents, functionality in brief)
The first time a player launches the app, it will enter the Name Prompt scene where it will ask the player to input his/her in-game name which will be saved on local storage. Thus the next time the app is launched the player will no longer be prompted for his/her name, and the locally stored name will be loaded. Instead the game will begin in the Start scene. The name can be changed subsequently through the Settings scene that can be navigated to from the Start scene.

From the Start scene, the player will be able to navigate to the Info scene, which runs the player through a short tutorial of the game. Most of work done for this scene was in the positioning of the UI game objects, as well as a simple controller Script that animates the different tutorial sections. At the end of the tutorial the player will be returned to the Start scene.

A player can choose to either host or join a game on the local network. The networking functionality is handled by a Game Object with a LobbyManager Script component. The LobbyManager extends the built-in NetworkLobbyManager class of the Unity High-level API (HLAPI), which provides methods for starting  host and client processes as well as hooks for one to implement custom behavior on certain events (eg. when a new client joins a lobby). When new clients join a lobby, the LobbyManager script causes Lobby Player Game Objects to be created on the server and spawned on the clients (these are the visible avatars that appear on the screen when new players join the lobby). 
These Lobby Player Game Objects each have a LobbyPlayer Script component attached to them. The LobbyPlayer script inherits from the HLAPI class NetworkLobbyPlayer, which provides functions that can be overridden to control what happens when a client enters a lobby. 

Once there are enough players in the lobby, (minimum of two players), and each player has indicated that they are ready to begin the game, a button will appear on the host player's screen for him/her to start the game. Once the game is started, all players move on to the Ability Selection scene.

The following sequence diagrams illustrate in greater detail what happens when a player hosts or joins a game:

#### Hosting a Game
Player chooses to host a game and moves from the start screen to the lobby scene.
Hosting initiates both a server and client process on the same machine, and the server starts broadcasting UDP messages for local discovery.

```sequence
Title: Hosting a Game
Note over LobbyManager: Extends Unity.Networking.NetworkLobbyManager\n which handles networking in the lobby\n and the main game
Note over MyNetworkDiscovery: Extends Unity.Networking.NetworkDiscovery\n which handles local discovery
Player->StartController:OnHostGame()
StartController->LobbyManager: StartHost()
LobbyManager->MyNetworkDiscovery: Initialize(),StartAsServer()
LobbyManager->LobbyPlayer: create new LobbyPlayer
LobbyManager-->LobbyManager: OnLobbyClientEnter()
LobbyManager->StartController: ToggleStartUI(false), ToggleWaitingUI(false)
Note left of LobbyManager: Changes UI to lobby UI
LobbyPlayer->LobbyPlayer: OnClientEnterLobby() [sets position and name, color of lobby player on screen], OnStartLocalPlayer() [sets name and color for lobby player local to machine]
```

#### Joining a Game
Player chooses to join a game and waits to receive a UDP message broadcasted from a server (host) on the local network. The UI changes from the start screen to the lobby scene which displays the number of players in the lobby when a client manages to connect to the host.

```sequence
Title: Joining a Game


Player->StartController: OnJoinGame()
StartController->MyNetworkDiscovery: Initialize(), StartAsClient()
StartController->StartController: ToggleStartUI(false), ToggleWaitingUI(true)
LobbyManager->MyNetworkDiscovery: OnReceivedBroadcast()
Note right of MyNetworkDiscovery: This method is called\n on the client when\n its receives UDP broadcast\n message from the server
MyNetworkDiscovery->LobbyManager: StartClient()
MyNetworkDiscovery->MyNetworkDiscovery: StopBroadcast()
LobbyManager->LobbyPlayer: create new LobbyPlayer
LobbyManager->LobbyManager: OnLobbyClientEnter()
LobbyManager->StartController: ToggleStartUI(false), ToggleWaitingUI(false)
Note left of LobbyManager: Changes UI to lobby UI
LobbyPlayer->LobbyPlayer: OnClientEnterLobby() [sets position and name, color of lobby player on screen], OnStartLocalPlayer() [sets name and color for lobby player local to machine]
```

#### Starting the ability selection when players are ready
When there are more than two players in the lobby, and all the players specify that they are ready to start the game, the lobby scene will change to the ability selection scene where the players will get to choose their two starting abilities.

```sequence
Title: Starting the Ability Selection Scene (on Host Player)


LobbyManager->LobbyManager: checkAllReady()
Note left of LobbyManager: LobbyManager constantly checks\n to see if players\n are all ready
LobbyManager->StartController: ToggleHostReadyUI(true)
Note right of LobbyManager: Shows a start game button\n on the host's screen
HostPlayer->LobbyManager: OnStartGame()
LobbyManager->LobbyManager: ServerChangeScene("SelectAbilities")
```

```sequence
Title: Starting the Ability Selection Scene (on Client Player)


LobbyManager->LobbyManager: checkAllReady()
Note left of LobbyManager: LobbyManager constantly checks\n to see if players\n are all ready
LobbyManager->StartController: ToggleClientReadyUI(true)
Note right of LobbyManager: Shows text "waiting\n for host to start the game"
HostPlayer->LobbyManager: OnStartGame()
LobbyManager->LobbyManager: ServerChangeScene("SelectAbilities")
```

### Ability Selection Scene
(insert fancy diagram with player object. Talk about how player abilities are saved)

This is the scene where the players get to choose their two starting abilities before the actual game begins. The bulk of the functionality here pertains to the UI, for which there is a controller Game Object with an AbilitySelectionController Script component attached. The most important part of this scene is the saving of player abilities.


*Jia Yu can talk about how abilities are implemented, and how they are added to the player objects*


The following sequence diagram illustrates in more detail what happens and a player chooses the two starting abilities and indicates that they are ready to start the main game.

#### Starting the game when players have selected their abilities
After every player has selected their two starting abilities and confirmed their selection, there will be short countdown before the game starts.

```sequence
Title: Ability Selection

participant HumanPlayer as Player
participant AbilitySelectionController as ASC
participant Player as P

Player->ASC: OnAbilityDetail()
Note left of ASC: Changes the UI to show\n details for the\n selected ability
Player->ASC: OnAbilityChosen()
ASC->P: addAbility(selectedAbility)
ASC->ASC: backToSelectionUI()

Player->ASC: OnAbilityDetail()
Player->ASC: OnAbilityChosen()
ASC->P: addAbility(selectedAbility)
ASC->ASC: backToSelectionUI()
Note left of ASC: Now that the player\n has chosen two abilities\n a ready button appears

Player->ASC: OnReadyButton()
ASC->Player: CmdReadyToPlay(true)
Note left of Player: sets a ready to play\n flag on the player object\n on the server which is\n synchronized with all the player\n objects on the client

LobbyManager->LobbyManager: checkAbilitiesReady()
Note right of LobbyManager: constantly checks if all\n players have chosen their\n abilities and are ready to start
LobbyManager->ASC: StartCountdown()
Note left of LobbyManager: starts a countdown screen\n before the main game starts
LobbyManager->LobbyManager: ServerChangeScene("Main")
```

### Main Scene
(insert fancy diagram with different components: food controller, mouse, level controller)
This is where the main gameplay occurs. There are three main components: the Mouse, the Food Controller, and the Level Controller.

The Mouse Game Object sits in the center of the screen waiting to be fed. The 'feeding' is basically implemented as collision detection between Game Objects with Collider2D components, namely between the Mouse Game Object and the Food Game Objects that the player swipes towards the Mouse. The Mouse behaviour is implemented by the attached Mouse Script component, which controls such things as the rotation of the mouse, the behaviour on collision detection with food, the detection of a player stroking the mouse to increase happiness,

implemented as a coroutine that increments or decrements the rotation of the  Mouse Game Object's Transform component by a linearly interpolated amount everytime

### Sequence diagrams
MOVE SEQUENCE DIAGRAMS TO WITHIN THE SCENE SECTIONS
#### Swiping Food
The core mechanic of the game is having the player swipe treats towards the mouse and junk food away from it. 

```sequence
Title: Swiping food

participant Food

Player->Food: detectTouchSwipe()
Food->Food.RigidBody2D: AddForce(swipeforce)
Note right of Food: A RigidBody2D is a\n Unity component that is\n used for adding 2D physics
Food->Mouse: OnCollisionEnter2D(collisionObj)
Note left of Mouse: The weight of the Mouse\n is modified depending\n on the type of Food
Mouse->Food.PoolMember: Deactivate()
Note right of Mouse: The PoolMember script\n is attached to game objects\n which are members of an object pool.\n Deactivate returns the game object to its pool
```

#### Gaining Happiness
Mouse happiness is required for players to use their abilities, and is gained when players stroke their mouse.

```sequence
Title: Gaining Happiness

participant Player
participant Mouse

Player->Mouse: updateHappiness()
Note right of Player: This method detects\n whether the player's finger\n has moved long enough\n to be counted as a stroke
```


## Generating and tracking combos
A controller object updates the combo sequence every so often and tracks whether the sequence of food fed to the mouse matches the combo sequence.

```sequence
Title: Generating Combos

participant LevelController

LevelController->LevelController: updateFoodCombo()
Note left of LevelController: updateFoodCombo() is a coroutine\n that runs at random intervals\n to update the food combo
```

```sequence
Title: Tracking Combos

participant LevelController

Mouse->LevelController: UpdateSequence()
Note right of Mouse: called when the Mouse is fed\n and updates a sequence count\n in LevelController that tracks\n whether the food fed\n matches the combo sequence
LevelController->LevelController: checkComboStreak()
Note left of LevelController: checks if the sequence count\n is 3, meaning a combo\n has been attained\n If so the player gets a\n score bonus and the combo\n count is incremented
```

## Entering and exiting frenzy mode
Frenzy mode is entered once the player has attained 10 combos (not necessarily consecutively). It lasts for 10 seconds then the game returns to normal mode.

```sequence
Title: Entering and Exiting Frenzy Mode

LevelController->LevelController: checkGameMode()
LevelController-->LevelController: enterFrenzy()
LevelController->LevelController: checkGameMode()
Note left of LevelController: after 10secs have elapsed\n the mode is set back to normal
LevelController-->LevelController: enterNormal()
```

#### Using abilities

#### End game

### Thread Safety
Unity API methods are not thread-safe. However, since none of our scripts are multi-threaded, there are not concurrency issues.
