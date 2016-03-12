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




