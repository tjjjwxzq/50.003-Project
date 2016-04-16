
# Use Cases
## Hosting a Game
Player hosts game and moves from the start screen to the lobby.
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

## Joining a Game
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

## Starting the ability selection when players are ready

## Starting the game when players have selected their abilities

## Swiping Food

## Gaining Happiness

## Generating and tracking combos

## Entering and exiting frenzy mode

## Using abilities

## End game
