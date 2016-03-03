# 50.003 Project (replace by app name later)

This will be a log for all our discussions and what not, until we finalize the features of the app

## WEEK 6 3/3/2016
TOTAL CHANGE OF IDEA!!!
Let's feed a mouse and make it as fat as possible as fast as possible! Other people are trying to fatten their pet mice too so you've got to get your game up! Mouse food and treats will be moving across the screen(how will they move? maybe three different sets of movements) and you have to swipe the treats to your mouse to feed it. The first player to get their mouse fully fattened wins!

But it's not just as simple as a ferocious feeding frenzy. Every player has some tricks(special abilities) under their sleeves: they can sabotage other players in various ways(sending over a cat or a swarm of ants to scare off their mouse, causing a sudden food drought, depriving other players of therir special abilities, stealing food from them, send more bad food over(laced with rat poison!)), or to boost their own pet (immunity to bad food for a certain time period, generate a deluge of treats, )

Player abilities can only be called when the player has enough mana (measured in terms of mouse happiness). Happiness can be increased by stroking your mouse or feeding it certain kinds of treats, and is exhausted when abilities are used.

Building on top of the core swiping mechanic, there will be a combo feature: there will be a specific 3-food combo (that will change randomly across the duration of the game), and if the player manages to feed food to his/her mouse in that order, there will be a score multiplier (2x the total score of the three foods), and if a combo streak (5 combos in a row?) is achieved the player will enter a frenzy feeding mode for a period of time (10sec?) where he/she can feed his mouth by shooting food directly at it (tapping rapidly on the mouse)

### Splitting of Work
1) Jun Qi: basic mechanics, swiping food, how the food moves on the screen, tracking mouse weight and happiness; UI and art assets
2) Hetty: Food combo implmentation, food types, frenzy feeding mode, happiness bar controller code
3) Jia Yu: Implementing player abilities, two classes:
  1. Player-boosting
    - Immunity to bad food for some time period
    - Generate a wave of treats
    - Increase mouse's ability to gain weight (score multipler for all food effectively)
    - Immunity to being scared by cats/ants/scary things
  2. Sabotage other players:
    - Send cat/ants to scare another player's mouse off the screen (player has to swipe screen to make mouse run back to center) (can't feed mouse during that period and maybe causes a decrease in happiness)
    - Send a wave of bad food
    - Steal food from them (less food for them and more for you!)
    - I can't of the last one fill something up here
4) Shun Yu: Networking (from start screen to host/join a game, updating clients about the status of other clients, updating clients when a player uses a sabotaging ability etc)



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
    * Point-and-click puzzle-based story-telling game/platform

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
* Interacting with objects
* Interacting with enemies
* Interacting with other players
* Action word trigger points
* Ending of a level block

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
  - Choose Multiplayer
    * Join game/Host game
      - Disconnect from LAN
      - Return to Start Screen

####Interacts with
* Triggering action word prompts 

####Open issues
* Do all players have to give assent before the game starts or can the host choose to start whenever it wishes?

### Use Case 2: Interacting with in-game objects
####Objective
* To interact with an in-game object (immersive experience; player collaboration)

####Pre-conditions
* Players have started the game

####Post-conditions
* Success
  - Object responds if player chooses to interact
  - Nothing changes if player chooses not to interact
* Failure
  - App crashes
  - Player disconnect from the LAN (for multiplayer)

####Actors
* 1-4 Players

####Triggers
* Player walks over in-game object

####Normal flow 
* Dialogue pops up, prompts player 
  - Player chooses to interact further
    * Object responds 
     - Aesthetic changes to environment
     - Trigger action word prompts
     - Affect global mechanics (eg. increased gravity) 
     - Reveal more storyline

####Alternative flow 
* Dialogue pops up, prompts player 
  - Player chooses not to interact further

####Exception flow
* Multiple players try to interact with the object at the same time 

####Interacts with

####Open issues
* How to prevent multiple players from interacting with the same object simultaneously?

###Use Case 3: Interacting with enemies 
####Objective
* Kill/avoid enemy or get killed by enemies

####Pre-conditions
* Players have started the game
* There are enemies on the map

####Post-conditions
* Player kills enemy 
  - enemy disappears
  - enemy respawns after a while/ is permanently gone
* Enemy kills player
  - player respawns at original spawn point

####Actors
* 1- 4 players

####Triggers
* Attack and collide into enemy
* Collide into enemy without attacking

####Normal flow
* Press A button
 - Collide with enemy
  * Enemy dies

####Alternative flow
* Collide with enemy
  - Player dies (haha)

####Interacts with
* Trigger action words prompt

####Open issues

###User Case 4: Interacting with other players
####Objective
* To interact with other players

####Pre-conditions
* Players have started the game
* Multiplayer game

####Post-conditions
* Players gain new information

####Actors
* 2-4 Players

####Triggers
* Players collide (overlap) with each other and press the A button

####Normal flow (aesthetic interaction)
* One player moves over the other, interaction prompt appears (change in colour, or dialogue pop-up)  
  -  Any one player presses the A button to interact
    - Triggers aesthetic change (sound or animation)

####Alternative flow (information passing)
* One player moves over the other, interaction prompt appears (change in colour, or dialogue pop-up)  
  - Any one player presses the A button to interact
    - Dialogue appears with information

####Exception flow (multiple players trying to interact together)
* More than two players overlap/collide, try to interact
  - The first pair that starts to interact is no longer interactable with other players for the duration of their interaction
    * Interacting pairs interact as normal

####Interacts with

####Open issues


###User Case 5: Triggering action word prompts
####Objective
* To allow players to choose story elements that will affect gameplay

####Pre-conditions
* Players have started the game

####Post-conditions
* Map is altered
* Story is added to
* Triggering next level block

####Actors
1-4 Players

####Triggers
* Interact with some key object
* Unlocked post boss-fight
* End of level-block 

####Normal flow
* Action word bubbles appear on screen 
 - Player jumps to touch a bubble and presses A button to choose word
  * Dialogue appears to elaborate on the chosen scenario
    - Map is altered in some way
      * Aesthetic changes
      * New objects appear on the map
      * Unlock NPC dialogue 

####Interacts with
* Interacting with objects
* Ending of level block
* Interacting with enemies

####Open issues


###User Case 6: Ending of level block
####Objective
* Allow user to progress to the next level/end the game

####Pre-conditions
* Player have started the game
* Players have finished the main quest for each level

####Post-conditions
* Players choose action words to set the next level
 - Players advance to next level block

####Actors
1-4 Players

####Triggers
* Players finish main level quest

####Normal flow
* Main level quest is completed
  - Dialogue appears on each player's screen to notify them on quest completion and story progression
    * Direction prompt appears on players screens to prompt them to move to gathering point (denoted by some structure?)
      - All players reach gathering point
       * Action word selection for the next level is triggered
        - Players advance to next level

####Interacts with
* Interacting with objects
* Ending of level block
* Interacting with enemies

####Open issues
* How to categorize and track story elements in the game? (to determine how next level is generated based on the chosen action word)

## Week 4 15/2/2016

###Procedural Story and Map generation
* Tile-based side-scrolling maps with different tile sets (triggered by player choice)
* Three general atmospheric themes coupled with play styles:
 - Action and Adventure (normal)
 - Espionage (cone of light)
 - Horror (small halo of light around players)
* Plot styles (determined by first action word choice, probably na adjective):
  - ABC plot
  - Accumulation of elements
  - Series of villains
  - Geographic progression
  - Event-driven
* Same action words in different contexts can have different associations
* Each action word is associated with a set of story paragraphs, one of which will be randomly chosen when that action word is chosen, and will appear to the player. At the end of the game the story paragraphs strung together will a retelling of the story of sorts.
* Story themes (a tree of stories with subtrees at different levels of specifity. If a player choice is made at that level of the tree, the corresponding set of nodes are chosen from )
  - Romance
  - Fantasy (Jun Qi)
  - Horror (Shun Yu)
  - Tragedy (Jia Yu)
  - Espionage (Hetty)
* Puzzle (tag-based system)
  - 5 puzzles a person
  - sidescrolling platformer
* Try to implement basic physics and UI by the end of this week


