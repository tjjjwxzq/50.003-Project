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

###Story Themes
Each story tree is split into levels, Level 1 being the topmost (Level 0 being the general theme). Every node in each level has an associated set of story fragments (the sublist under it) one of which will be randomly chosen when that node is picked. The story fragments are tagged according to what kind of objects would trigger them (eg. NPC, defeated enemy, general object). Meaning these objects would all be action word trigger points. Ideally the story fragments should be vague enough for them to be able to be combined with any of the fragments at the higher levels.

#### Fantasy

##### Level 1
* Taming/Slaying dragons
  - "Have you heard of the mythical beast in the land far yonder?"(NPC)
  - "We would be rich, if the dragon wasn't always robbing us."(NPC)
  - "You have the look of a young tamer!" (NPC)
  - "A most curious object. You wonder if it could have come from a dragon's lair" (object)
  - "You seem noble enough to be my master, but I'll have to test your strength first!"(NPC/enemy dragon)
  - "It's an egg! Maybe you should find some way of making it hatch?" (object)
  - "It looks like a house that's been ravaged by a dragon..." (object)
  - "It's said that whoever is able to hew this stone into two is destined to slay a dragon..." (object)
  - "There is said to be a small kingdom by the coast, where half-dragons live." (NPC)
  - "A dragon hatchling! It's struggling to breathe..." (object/NPC dragon)
  - "The dragon race is a proud one. But they make powerful allies. Now if only there was a way to bring them over to our side..." (NPC)
* Inverted damsel in distress
  - "It's an extensive dungeon, and you seem to be trapped..."(object)
  - "You're not getting past my watch!" (NPC)
  - "Might this passageway lead somewhere?" (object)
  - "Ha! Now the boss has got you for good!"(NPC)
  - "You are hit by a sudden recollection of past events. You remember chaos suddenly descending on the crowd gathered for the celebration, a brilliant flash...and now it seems you're in a cellar." (object)
  - "Heh! Why are you looking so frantic, little mouse?" (NPC)
  - "Looks like we're in the same boat" (player)
  - "There's gotta be a way out of here...We can do this together!" (player)
* Ancient prophecy/secret of the world
  - "The original words, carved in a stone tablet, have faded into time itself. But there has been a feeling that something is imminent... " (NPC)
  - "This place is heavy with the weight of all things lost but still remembered..." (object)
  - "The intensity of your eyes...look like they could unlock the world's deep secret..." (NPC)
  - "It's a strange curio. A set of incomprehensible glyphs seem to be carved into it..." (object)
  - "Hark travellers! It's said that once strangers enter this land, the very foundations of the world itself will be shaken" (NPC)
  - "I'm filled with the premonition...that the universe is beckoning us to unlock one of it's secrets" (player)
  - "It seems to be an old book of fables...the words on one of the pages seem to glow, but the characters are incomprehensible...Now if only you could find someone who would be able to decipher it?" (object)
  - "The world tree...is speaking. Can you hear its sighs in the wind? This portends something momentous..." (NPC)
* Chosen ones to save a world in peril
  - "A crest half-buried in the sand. It responds to your touch..." (NPC)
  - "Bad things are happening in the world...Very bad things. Will you save us?" (NPC)
  - "This place appears to have been ravaged...Are there still survivors?" (object)
  - "It's over! All over! The Edon clan taken control of everything...The world is not safe in their hands. Will you save us?" (NPC)
  - "You hear the slightest whisper in the wind: 'Chosen ones...chosen ones...At last you've come...'"(object)
  - "There was a reason you were brought to this world...Take this book. Maybe you'll eventually discover why in its pages." (NPC)
  - "So much devastation...I feel like it's our responsibility to do something...save this world." (player)
  - "Ah, young one. You tread the path that is well-worn by those before you. Another cycle is repeating...the evil army gathers" (NPC)
* Pure exploration (building a universe with subplots but no over-arching goal: Aquaria-esque)
  - "This structure seems to be a gate to a long-forgotten shrine..." (object)
  - "A mossy rock covered with glowing inscriptions...Could this be an enchanted forest?"(object)
  - "Beyond this village are the Woods of Yogleth...Brutal trolls live there. You must be careful"(NPC)
  - "You pick up the charm, and it glows softly. Images of dragon-wolves, mighty warriors, suddenly flash into your mind. This could have been the land of a once mighty kingdom..."
  - "A chilly wind bites at your cheek as you examine the landscape. What could be out there in this icy wasteland?"
  - "As dusk descends on the world, creatures of the night begin to stir around you. There is a plaintive howl in the distance. Maybe you should follow it?"
  - "Many young travellers like you have come and gone. Most move on to the Land of the West, where they say the source of magic lies." (NPC)
* Orphans who inherit powerful but dangerous abilities from their long-lost parents and go out in search for their roots while learning to control their powers
* War between two kingdoms is imminent; players must try to prevent it

##### Level 2
* Taming/slaying dragons
  - Slaying dragons
    * "It occurs to you that this is an untameable creature, that you needs must slay it to neutralize its threat to the villagers" (NPC/enemy dragon)
    * "If you would find the dragon's lair and slay it for us, our village will reward you handsomely."(NPC)
    * "As it rises with a newfound strength, it shoots out a jet of fire dangerously close. You realize that it is actually hostile, but it flees before you are able attack it." (NPC/enemy dragon)
