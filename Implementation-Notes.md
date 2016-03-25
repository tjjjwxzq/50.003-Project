# Implementation Notes

## Level
There will be 10 weight levels that the mouse will hit. Each level up allows the player to 1) updgrade and existing ability 2) add a new ability (if they have less than 4 abilities). Each time the mouse levels up its sprite will also change, and each level there will be a dashed outline around to mouse to indicate the it must attain to reach the next level.

## Happiness
Happiness acts as mana for player abilities. Each ability uses a certain amount of happiness, and happiness can be recharged by stroking the mouse. Currently the range is set from 0 to 100.

Players can track the happiness by the happiness bar on the left of the screen

Possible mechanic: Each time your mouse happiness reaches the max, the rate at which stroking increases happiness increases by 1.

## Food
All food objects will carry a `Food` script componenet which handles touch detection and movement. The nutritional value or food points, as well as the type(eg. Cheese, carrot), are stored in public variables `Value` and `Type` which can be modified via the inspector panel. All food objects will be tagged with 'Food' for appropriate behaviour upon collision. The different food types will be fixed as different prefabs with different value and type fields.

### Food Types
| Food types | Points|
|:-----------|:-----:|
| Good food  |       |
| 1. Normal  |  5pt  |
| 2. Cheese  |  10pt |
| 3. Carrot  |  7pt  |
| 4. Oat     |  15pt |
| 5. Apple   |  8pt  |
| 6. Anchovy |  12pt |
| 7. Bread   |  18pt |
| 8. Seed    |  20pt |
|            |       |
| Bad food   |       |
| 1. Bad     |  -5pt |
| 2. Peanut  |  -7pt |
| 3. Orange  |  -10pt|
| 4. Garlic  |  -15pt|
| 5. Chocolate| -20pt|
| 6. Poison  |  -50pt|

### Food Spawning and Movement
This is controlled by a FoodController object with a FoodController Script component. The FoodController tracks a maxFoodCount and a duration range within which it will randomly spawn food. Player abilities may affect these two parameters to boost themselves or sabotage other players.

Food spawning position is weighted with higher probability further from the center (from the mouse).

Each time the spawning coroutine runs it will randomly pick one type of food to spawn. The spawning probability is weighted:

Spawning Probability Weights:

| Food types | Weight|
|:-----------|:-----:|
| Good food  |       |
| 1. Normal  |  6    |
| 2. Cheese  |  2.5  |
| 3. Carrot  |  3.5  |
| 4. Oat     |  2    |
| 5. Apple   |  3    |
| 6. Anchovy |  1.5  |
| 7. Bread   |  1    |
| 8. Seed    |  0.8  |
|            |       |
| Bad food   |       |
| 1. Bad     |  4    |
| 2. Peanut  |  2    |
| 3. Orange  |  1.5  |
| 4. Garlic  |  1.2  |
| 5. Chocolate| 0.8  |
| 6. Poison  |  0.2  |
|
| TOTAL      |  30.0 |



1. Static  
Food just stays at their spawn location.

2. Horizontal  
Food moves horizontally. Direction changes within a random time interval.

3. Vertical  
Food moves vertically. Direction changes within a random time interval.

4. Random  
Food moves randomly. Direction changes within a random time interval.


## Abilities
Player abilities come in two types: those that boost said player and those that sabotage other players. Players start with two abilities (chosen or random?) and can choose to unlock more abilities or level up current abilities (so how many mouse levels should we have? 4 seems a bit too little then)

### Self-boosting
1. Immunity to bad food for a duration (Immunity)

  * Happiness: 30
  * Level 1: duration of 10sec
  * Level 2: duration of 30sec

2. Increase probability/number of higher point foods (Treats Galore)

  * Happiness: 80
  * Level 1: increases probability of spawning a randomly chosen treat for 15sec, as well as max treat count (max number of that type of food that can be on the screen at once)
  * Level 2: increases probability of spawning a randomly chosen treat whose value >= 10pt, for 20sec, as well as max treat count
  * Level 3: generate a wave of a randomly chosen treat for 15sec (all food spawned will be of that type during that duration)

3. Fearlessness of cats/ants/scary things (Fearless!)

  * Happiness: 40
  * Level 1: doesn't run off-screen, but happiness and weight still drop, duration of 30sec
  * Level 2: doesn't run off-screen, but happiness still drops, duration of 30sec
  * Level 3: completely fearless, duration of 30sec

4. Increased ability to gain weight (score multiplier on all foods eaten) (Fat Mouse)

  * Happiness: 70
  * Level 1: gains weight twice as easily (2x multipler) for 15sec
  * Level 2: gains weight four times as easily (4x multiplier) for 20sec

### Sabotage
1. Send over a scary animal to a chosen player (Scary Cat)

  * Happiness: 60
  * Level 1: scares mouse off screen for 5sec, reduces weight by 50 and happiness by 10
  * Level 2: scares mouse off screen for 10sec, reduces weight by 100 and happiness by 20

2. Send a wave of bad food to a chosen player (Beastly Buffet)

  * Happiness: 50
  *  Level 1: increases probability of spawning a randomly chosen bad food for 15sec, as well as max food count (max number of that type of food that can be on the screen at once)
  * Level 2: increases probability of spawning a randomly chosen treat whose value <= -10pt, for 20sec, as well as max food count
  * Level 3: generate a wave of a randomly chosen bad food for 15sec (all food spawned will be of that type during that duration)

3. Steal food from a chosen player (Thief!)

  * Happiness: 70
  * Level 1: increase spawn interval of chosen player by 1.5 times and decrease own spawn interval by 1.5 times. Removes up to 10 good food from chosen player's screen and adds them to yours. Lasts for 15sec.
  * Level 2: increase spawn interval of chosen player by 2 times and decrease own spawn interval by 2 times. Removes up to 15 good food from chosen player's screen and adds them to yours.


## Networking
### Saving Player Name
When the player launches the app for the first time, a scene is entered where the players is prompted for his/her name. This name is saved locally and is used by default next time, but the player may change it through player settings (through the main menu). This can be saved simply using `PlayerPrefs.SetString("Name", "PlayerNameHere")` and got using `PlayerPrefs.GetString("Name")`

### Local Network Discovery
From the start screen a player can choose to host or join a game on the local network. This is done by using the `NetworkDiscovery` component which allows one to start a server that will broadcast UDP messages and clients that will listen for them.

The `NetworkDiscovery` class is extended and its `OnReceivedBroadcast` message is overridden to enable clients to be started by the `NetworkLobbyManager`.

### Lobby
In the Lobby scene, on initialization of the Lobby Player game object(`OnStartLocalPlayer()`), the local player should set its name using the saved `PlayerPrefs`, then call a command right after to inform the server that the name was changed. The name variable should also be a `SyncVar` so the change is reflected on all clients.

When a player enters the lobby (`OnClientEnterLobby()`), the UI avatar should be set up with the correct name (?? is this called before `OnStartLocalPlayer()`?)

Ideally each player should be able to tap on their avatar/icon to change their color. The colors should be unique, so if any of the colors are used by other players, the tap skips to the next unused color in the list. So given a list of all the possible colors, maintain a list of used colours.
When a player first enters the lobby, go through the list of possible colors till the first unused one is found, then set that as the default color.
When the player taps on the avatar to change color, look through the list to find the next unused color. Set that as the new color, and remove the old color from the list of used colors.
The function to change the color should be a command (sent to the server) wrapped in another callback function attached to the UI button.

Under each player avatar there should be a ready button, which should be bound to a method that will call either `SendReadyToBeginMessage()` or `SendNotReadyToBeginMessage()` depending on the current `readyToBegin` flag.

Next to each player avatar there should be an exit button, which hould be bound to a method that will call `RemovePlayer()` and close the connection to the server (`Network.CloseConnection(Network.connections[0])` is clicked on the client local player (`isLocalPlayer` flag), and then reset the UI to the start screen UI.

There will be a start button in the lobby that will only be enabled when all the players in the lobby are ready. This can happen even when there are less than 4 players (min players is 2 and max is 4).

To pass the player colors from the lobby scene to the main scene, use `OnLobbyServerSceneLoadedForPlayer()`.

### Main Game
