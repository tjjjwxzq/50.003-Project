# Implementation Notes

## Food
All food objects will carry a `Food` script componenet which handles touch detection and movement. The nutritional value or food points, as well as the type(eg. Cheese, carrot), are stored in public variables `Value` and `Type` which can be modified via the inspector panel. All food objects will be tagged with 'Food' for appropriate behaviour upon collision. The different food types will be fixed as different prefabs with different value and type fields.

### Food Types
List of food types:
1. Normal     5pt
2. Cheese     10pt
3. Carrot     7pt
4. Oat        15pt
5. Apple      8pt
6. Anchovy    12pt
7. Bread      18pt
8. Seed       20pt

1. Bad        -5pt
2. Peanut     -7pt
3. Orange     -10pt
4. Garlic     -15pt
5. Chocolate  -20pt
6. Poison     -50pt

### Food Spawning and Movement
This is controlled by a FoodController object with a FoodController Script component. The FoodController tracks a maxFoodCount and a duration range within which it will randomly spawn food. Player abilities may affect these two parameters to boost themselves or sabotage other players.

Food spawning position is weighted with higher probability further from the center (from the mouse).

Each time the spawning coroutine runs it will randomly pick one type of food to spawn. The spawning probability is weighted:

Spawning Probability Weights:
1. Normal     6
2. Cheese     2.5
3. Carrot     3.5
4. Oat        2
5. Apple      3
6. Anchovy    1.5
7. Bread      1
8. Seed       0.8

1. Bad        4
2. Peanut     2
3. Orange     1.5
4. Garlic     1.2
5. Chocolate  0.8
6. Poison     0.2

TOTAL         30.0



1. Static  
Food just stays at their spawn location.

2. Horizontal  
Food moves horizontally. Direction changes within a random time interval.

3. Vertical  
Food moves vertically. Direction changes within a random time interval.

4. Random  
Food moves randomly. Direction changes within a random time interval.

