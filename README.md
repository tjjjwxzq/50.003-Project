# 50.003 Project (replace by app name later)

This will be a log for all our discussions and what not, until we finalize the features of the app

## WEEK 1 26/1/2016

### What should our app be about?
####1. At a high level
  * **A Game**
  * A Messaging app
  * A Social Shopping app

####2. A Game
  * Air Hockey/ pinball mod (multiplayer)
    * Ball with different properties 
  * Osmos-like
  * Infinite-runner 1
  * Pacman-like
  * Puzzle-based story-telling platform 1 1 1
  * Coop-survival on a map (individual players can help one another?), adventure game
  * Companion app for SUTD Orientation (ARGish) -Ingress-like 1
  * Point and click adventure 1 1
  * Party game mashup

  Narrowing down:
    * Story! 1
    * Persistence! (player contributions, meta games)
    * Point-and-click puzzles 1
    * Custom obstacle maps
  So:
    * Point-and-click puzzed-based story-telling game/platform

## WEEK 1 27/1/2016
### Milestones
####1. Ideation
  * Design Document TODO 8/2/2016
    * Game mechanics
    * Storyline
    * Wireframing (UI/UX)
    * Concept Art
    * Technologies (which libraries: Unity, libgdx) !!!
    * Testing (think about test cases)
    * Sound assets 

  * Delegation of work TODO 8/2/2016

####2. Learn technologies!!! 
  * Toy app and sharing TODO 20/2/2016

####3. Narrow down design document (more specific)
  * Implementation details, test cases TODO 27/2/2016
 

## WEEK 2 3/2/2016
### Refining the game idea
* Concurrent multiplayer story-telling game
* Players create a narrative as they play
* Narrative is restricted by a set of action words 
  - trigger points? (level blocks)
  - how flexible is this going to be
* Sidescrolling runner/**platformer**/point and click?
* Game starts with a set of actions words that starts the first level block
* *Platformer Mechanics*
  - jumping
  - basic attack (interaction button)
  - basic enemies
  - interactive objects (prompt when player walks over, button to interact)
  - point system (???)
  - puzzle elements
* UI
  - on screen game pad
* User input within level blocks (interactivity)
* Multiplayer mode
 - Competing to reach the triggers
 - Different original story arcs depending on the original action chosen (eg. romantic story)
 - Interaction between players, questions between player
 - Betrayal prompts
 - One player will host a game
* Time-based triggers and object-based triggers
* Action words are deliberately ambiguous, once chosen a dialogue appears elaborating more on the exact scenario chosen

### Use Cases
* Starting the game
* Interact with objects
* Interact with NPCs (dialogues to tell more of the story)
* Interact with other players
* Action word trigger points
* End game

### Use Case 1: Starting the Game
####Objective
* To start playing the game
####Pre-conditions
* Player can connect to a LAN (if playing multiplayer)
####Post-conditions
* Success
  - Single player game starts
  - Players able to join lobby and start the game
* Failure
  - App crashes
  - Player disconnect from the LAN
####Actors
* 1-4 Players
####Triggers
* Player launches the app
####Normal flow 
* Start Screen
  - Choose Single player 
####Alternative flow 1
* Start Screen
  * Choose Multiplayer
    - Join game
      * Exit lobby
      * Starting room: initial action word choice
####Alternative flow 2
* Start Screen 
  - Choose Multiplayer 
    * Host game
      - Exit lobby
      - Starting room: initial action word choice
####Exception flow
* Start Screen
  - Choose Multiplyaer
    * Join game/Host game
      - Disconnect from LAN
      - Return to Start Screen
####Interacts with(???)  
####Open issues(???)

### Use Case 2: Starting the Game
####Objective
* To start playing the game
####Pre-conditions
* Player can connect to a LAN (if playing multiplayer)
####Post-conditions
* Success
  - Single player game starts
  - Players able to join lobby and start the game
* Failure
  - App crashes
  - Player disconnect from the LAN
####Actors
* 1-4 Players
####Triggers
* Player launches the app
####Normal flow 
* Start Screen
  - Choose Single player 
####Alternative flow 1
* Start Screen
  * Choose Multiplayer
    - Join game
      * Exit lobby
      * Starting room: initial action word choice
####Alternative flow 2
* Start Screen 
  - Choose Multiplayer 
    * Host game
      - Exit lobby
      - Starting room: initial action word choice
####Exception flow
* Start Screen
  - Choose Multiplyaer
    * Join game/Host game
      - Disconnect from LAN
      - Return to Start Screen
####Interacts with(???)  
####Open issues(???)

