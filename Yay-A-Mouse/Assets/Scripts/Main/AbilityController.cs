using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Random = System.Random;

/// <summary>
/// Script class to encapsulate mouse ability logic.
/// Implements functions to be called when abilities are activated by a player
/// or when a player is affected by an ability activated by another player.
/// Should be attached to a Player object, but only if it is the local player
/// This component is attached to the local player object when the scene is changed to Main
/// </summary>
public class AbilityController : NetworkBehaviour
{
    // Other GameObjects
    private Mouse mouse;
    public Player player;
    private FoodController foodController;
    private LevelController levelController;
    private AbilityUI abilityUi;
    private Text messageBox;

    // Game messages
    private List<string> activeStatuses;
    private List<GameMessage> gameMessages;

    private struct GameMessage
    {
        public DateTime time;
        public string message;

        public GameMessage(DateTime time, string message) : this()
        {
            this.time = time;
            this.message = message;
        }
    }

    // Simple status flags
    private bool mouseIsImmune;
    private bool mouseIsFearless;
    private bool mouseIsFat;

    // Mouse level
    private int maxLevelAchieved;
    private int prevLevel;
    public int abilityPoints;

    // Treats Galore status
    private bool treatsGaloreIsActive;
    private string treatsGaloreBoostedFood;
    private float tgOriginalWeight;
    private int tgOriginalCount;

    // Scary Cat status
    private bool mouseIsOffscreen;
    private int scaryCatDuration;

    // Beastly Buffet status
    private bool beastlyBuffetIsActive;
    private string beastlyBuffetBoostedFood;
    private int beastlyBuffetDuration;
    private float bbOriginalWeight;
    private int bbOriginalCount;

    // Thief status
    private bool mouseIsThief;
    private bool mouseIsThiefVictim;
    private DateTime lastStolenFrom;
    private int thiefVictimDuration;

    // targetedplayer for abilities
    public string targetedPlayer = "";

    // ability cooldown
    private Dictionary<AbilityName, Text> countdowns;

    private Dictionary<string, float> defaultFoodSpawnWeights;

    private Dictionary<string, int> defaultMaxFoodCounts;

    private IDictionary<AbilityName, DateTime> abilityLastActivatedTimes;

    void Start()
    {
        Debug.Log(String.Format("AbilityController for {0} starting.", gameObject.GetComponent<Player>().Name));
        player = gameObject.GetComponent<Player>();
        abilityLastActivatedTimes = new Dictionary<AbilityName, DateTime>(7);
        gameMessages = new List<GameMessage>(3);
        activeStatuses = new List<string>();
    }

    /// <summary>
    /// Function that will look for a local player mouse object and get a reference to it.
    /// 
    /// Since the ability controller starts before the main scene is initialised, we can only get references
    /// to main scene objects after the main scene has loaded. These methods are called to attach the 
    /// ability controller to the respective main scene objects.
    /// </summary>
    /// <returns>This ability controller, so that calls can be chained.</returns>
    public AbilityController AttachToMouse()
    {
        mouse = GameObject.Find("Mouse").GetComponent<Mouse>();
        if (mouse == null)
        {
            Debug.LogError(player.Name + " AbilityController failed to attach to Mouse.");
            return this;
        }
        Debug.Log(player.Name + " AbilityController component attached to mouse.");
        maxLevelAchieved = mouse.Level;
        return this;
    }

    /// <summary>
    /// Function that will look for a local player food controller object and get a reference to it.
    /// 
    /// Since the ability controller starts before the main scene is initialised, we can only get references
    /// to main scene objects after the main scene has loaded. These methods are called to attach the 
    /// ability controller to the respective main scene objects.
    /// </summary>
    /// <returns>This ability controller, so that calls can be chained.</returns>
    public AbilityController AttachToFoodController()
    {
        foodController = GameObject.Find("FoodController").GetComponent<FoodController>();
        if (foodController == null)
        {
            Debug.LogError(player.Name + " AbilityController failed to attach to FoodController.");
            return this;
        }
        Debug.Log(player.Name + " AbilityController component attached to food controller.");
        return this;
    }

    /// <summary>
    /// The Update method is called once per frame. The ability controller 
    /// checks the state of various game variables and updates them if necessary.
    /// 
    /// Variables checked:
    /// 
    /// Immunity ability last used and duration
    /// Mouse level
    /// Mouse Immunity status
    /// Treats galore ability last used and duration
    /// FoodController treats galore active
    /// Mouse Fearless ability last used and duration
    /// Mouse Fearless active
    /// Fat Mouse ability last used and duration
    /// Thief spawn boost last activated and duration
    /// Mouse Fat active
    /// Mouse last chased offscreen by Scary Cat and duration
    /// Mouse Offscreen
    /// Mouse last affected by Beastly Buffet and duration
    /// Mouse last affected by Thief and duration
    /// </summary>
    void Update()
    {
        if (!isLocalPlayer) return;

        if (mouse == null || foodController == null || levelController == null || abilityUi == null) return;

        if (mouse.Level > maxLevelAchieved)
        {
            Debug.Log(player.Name + "levelled up!");
            maxLevelAchieved = mouse.Level;
            abilityPoints += 2;
        }

        if (mouseIsImmune)
        {
            var timeLeft = TimeLeft(AbilityName.Immunity);
            abilityUi.Countdowns[AbilityName.Immunity].text = timeLeft.ToString();
            activeStatuses.Add(string.Format("{0} seconds of Immunity left!", timeLeft));
            if (!IsStillActive(AbilityName.Immunity))
            {
                mouseIsImmune = false;
                mouse.Immunity = false;
                abilityUi.Countdowns[AbilityName.Immunity].gameObject.SetActive(false);
            }
        }

        if (treatsGaloreIsActive)
        {
            var timeLeft = TimeLeft(AbilityName.TreatsGalore);
            abilityUi.Countdowns[AbilityName.TreatsGalore].text = timeLeft.ToString();
            activeStatuses.Add(string.Format("{0} seconds of Treats Galore left!", timeLeft));
            if (!IsStillActive(AbilityName.TreatsGalore))
            {
                foodController.setMaxFoodCount(treatsGaloreBoostedFood, tgOriginalCount);
                foodController.setFoodSpawnWeight(treatsGaloreBoostedFood, tgOriginalWeight);
                treatsGaloreIsActive = false;
                abilityUi.Countdowns[AbilityName.TreatsGalore].gameObject.SetActive(false);
            }
        }

        if (mouseIsFearless)
        {
            var timeLeft = TimeLeft(AbilityName.Fearless);
            abilityUi.Countdowns[AbilityName.Fearless].text = timeLeft.ToString();
            activeStatuses.Add(string.Format("{0} seconds of Fearless left!", timeLeft));
            if (!IsStillActive(AbilityName.Fearless))
            {
                mouseIsFearless = false;
                mouse.Fearless = false;
                abilityUi.Countdowns[AbilityName.Fearless].gameObject.SetActive(false);
            }
        }

        if (mouseIsFat)
        {
            var timeLeft = TimeLeft(AbilityName.FatMouse);
            abilityUi.Countdowns[AbilityName.FatMouse].text = timeLeft.ToString();
            activeStatuses.Add(string.Format("{0} seconds of Fat Mouse left!", timeLeft));
            if (!IsStillActive(AbilityName.FatMouse))
            {
                mouse.GrowthAbility = 1;
                mouseIsFat = false;
                abilityUi.Countdowns[AbilityName.FatMouse].gameObject.SetActive(false);
            }
        }

        if (mouseIsOffscreen)
        {
            var timeLeft = scaryCatDuration - DateTime.Now.Subtract(abilityLastActivatedTimes[AbilityName.ScaryCat]).Seconds;
            activeStatuses.Add(string.Format("{0} seconds until mouse returns!", timeLeft));
            if (!IsStillActive(AbilityName.ScaryCat, scaryCatDuration))
            {
                mouse.Offscreen = false;
                mouseIsOffscreen = false;
            }
        }

        if (beastlyBuffetIsActive)
        {
            var timeLeft = beastlyBuffetDuration - DateTime.Now.Subtract(abilityLastActivatedTimes[AbilityName.BeastlyBuffet]).Seconds;
            activeStatuses.Add(string.Format("{0} seconds of Beastly Buffet remaining!", timeLeft));
            if (!IsStillActive(AbilityName.BeastlyBuffet, beastlyBuffetDuration))
            {
                foodController.setMaxFoodCount(beastlyBuffetBoostedFood, bbOriginalCount);
                foodController.setFoodSpawnWeight(beastlyBuffetBoostedFood,
                    bbOriginalWeight);
                beastlyBuffetIsActive = false;
            }
        }

        if (mouseIsThief)
        {
            var timeLeft = TimeLeft(AbilityName.Thief);
            abilityUi.Countdowns[AbilityName.Thief].text = timeLeft.ToString();
            activeStatuses.Add(string.Format("{0} seconds until Thief buff runs out!", timeLeft));
            if (!IsStillActive(AbilityName.Thief))
            {
                foodController.SpawnRate = 1f;
                mouseIsThief = false;
                abilityUi.Countdowns[AbilityName.Thief].gameObject.SetActive(false);
            }
        }

        if (mouseIsThiefVictim)
        {
            var timeLeft = thiefVictimDuration - DateTime.Now.Subtract(lastStolenFrom).Seconds;
            activeStatuses.Add(string.Format("{0} seconds until Thief debuff runs out!", timeLeft));
            if (DateTime.Now.Subtract(lastStolenFrom).Seconds > thiefVictimDuration)
            {
                foodController.SpawnRate = 1f;
                mouseIsThiefVictim = false;
            }
        }

        //        var statuses = string.Join("\n", activeStatuses.ToArray());

        gameMessages.RemoveAll(o => DateTime.Now.Subtract(o.time).Seconds > 5);
        var gameMsgs = string.Join("\n", gameMessages.Select(o => o.message).ToArray());
        messageBox.text = gameMsgs;
        activeStatuses.Clear();
    }

    /// <summary>
    /// Function that will look for a local player level controller object and get a reference to it.
    /// 
    /// Since the ability controller starts before the main scene is initialised, we can only get references
    /// to main scene objects after the main scene has loaded. These methods are called to attach the 
    /// ability controller to the respective main scene objects.
    /// </summary>
    /// <returns>This ability controller, so that calls can be chained.</returns>
    public AbilityController AttachToLevelController()
    {
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
        if (levelController == null)
        {
            Debug.LogError(player.Name + " AbilityController failed to attach to LevelController.");
            return this;
        }
        Debug.Log(player.Name + " AbilityController component attached to level controller.");
        return this;
    }

    /// <summary>
    /// Function that will look for a local player abilityUI object and get a reference to it.
    /// 
    /// Since the ability controller starts before the main scene is initialised, we can only get references
    /// to main scene objects after the main scene has loaded. These methods are called to attach the 
    /// ability controller to the respective main scene objects.
    /// </summary>
    /// <returns>This ability controller, so that calls can be chained.</returns>
    public AbilityController AttachToAbilityUi()
    {
        abilityUi = GameObject.Find("Canvas").transform.Find("Abilities").gameObject.GetComponent<AbilityUI>();
        if (abilityUi == null)
        {
            Debug.LogError("AbilityController failed to attach to AbilityUi.");
            return this;
        }
        Debug.Log(player.Name + " AbilityController attached to AbilityUi.");
        return this;
    }

    public AbilityController AttachToMessageBox()
    {
        messageBox = GameObject.Find("GameMessages").GetComponent<Text>();
        if (messageBox == null)
        {
            Debug.LogError("AbilityController failed to attach to MessageBox.");
            return this;
        }
        Debug.Log(player.Name + " AbilityController attached to MessageBox");
        return this;
    }

    public bool IsActive(AbilityName ability)
    {
        switch (ability)
        {
            case AbilityName.Immunity:
                return mouseIsImmune;
            case AbilityName.TreatsGalore:
                return treatsGaloreIsActive;
            case AbilityName.Fearless:
                return mouseIsFearless;
            case AbilityName.FatMouse:
                return mouseIsFat;
            case AbilityName.ScaryCat:
                return false;
            case AbilityName.BeastlyBuffet:
                return beastlyBuffetIsActive;
            case AbilityName.Thief:
                return mouseIsThief;
            default:
                throw new ArgumentOutOfRangeException("ability", ability, null);
        }
    }

    // Function to check if an ability is still active using the duration specified in the player's instance of the ability.
    private bool IsStillActive(AbilityName ability)
    {
        if (abilityLastActivatedTimes.ContainsKey(ability))
        {
            var timeSinceLastActivation = DateTime.Now.Subtract(abilityLastActivatedTimes[ability]).Seconds;
            return timeSinceLastActivation < player.PAbilities[ability].Duration;
        }
        else return false;
    }

    private int TimeLeft(AbilityName ability)
    {
        if (abilityLastActivatedTimes.ContainsKey(ability))
        {
            var timeSinceLastActivation = DateTime.Now.Subtract(abilityLastActivatedTimes[ability]).Seconds;
            return player.PAbilities[ability].Duration - timeSinceLastActivation;
        }
        else return -1;
    }

    // Function to check if an ability is still active using the specified duration.
    private bool IsStillActive(AbilityName ability, int duration)
    {
        if (abilityLastActivatedTimes.ContainsKey(ability))
        {
            var timeSinceLastActivation = DateTime.Now.Subtract(abilityLastActivatedTimes[ability]).Seconds;
            return timeSinceLastActivation < duration;
        }
        else return false;
    }

    /// <summary>
    /// To be called by the Player script
    /// to activate the ability based on 
    /// the tapped icon
    /// </summary>
    /// <param name="ability"></param>
    public void ActivateAbility(AbilityName ability)
    {
        switch (ability)
        {
            case AbilityName.Immunity:
                ActivateImmunity();
                abilityUi.Countdowns[ability].text = player.PAbilities[AbilityName.Immunity]
                    .Duration.ToString();
                abilityUi.Countdowns[ability].gameObject.SetActive(true);
                break;
            case AbilityName.TreatsGalore:
                ActivateTreatsGalore();
                abilityUi.Countdowns[ability].text = player.PAbilities[AbilityName.TreatsGalore]
    .Duration.ToString();
                abilityUi.Countdowns[ability].gameObject.SetActive(true);
                break;
            case AbilityName.Fearless:
                ActivateFearless();
                abilityUi.Countdowns[ability].text = player.PAbilities[AbilityName.Fearless]
    .Duration.ToString();
                abilityUi.Countdowns[ability].gameObject.SetActive(true);
                break;
            case AbilityName.FatMouse:
                ActivateFatMouse();
                abilityUi.Countdowns[ability].text = player.PAbilities[AbilityName.FatMouse]
    .Duration.ToString();
                abilityUi.Countdowns[ability].gameObject.SetActive(true);
                break;
            case AbilityName.ScaryCat:
                ActivateScaryCat();
                break;
            case AbilityName.BeastlyBuffet:
                ActivateBeastlyBuffet();
                break;
            case AbilityName.Thief:
                ActivateThief();
                abilityUi.Countdowns[ability].text = player.PAbilities[AbilityName.Thief]
    .Duration.ToString();
                abilityUi.Countdowns[ability].gameObject.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException("ability", ability, null);
        }
    }

    /// <summary>
    /// Improves one of the player's abilities. Does nothing if the ability level is already at maximum.
    /// </summary>
    /// <param name="ability"></param>
    public void ImproveAbility(AbilityName ability)
    {
        if (player.PAbilities[ability].Level >= player.PAbilities[ability].MaxLevel) return;
        player.PAbilities.SetAbility(ability, player.PAbilities[ability].Level + 1);
        abilityUi.UpdateAbilityLevelOnButton(ability);
        abilityPoints--;

        NewGameMessage("Improved " + player.PAbilities[ability].Name + "!");
    }

    public void NewGameMessage(string message)
    {
        gameMessages.Add(new GameMessage(DateTime.Now, message));
        //        if (gameMessages.Count > 3) gameMessages.Dequeue();
    }

    /// <summary>
    /// Function to be called when a player activates the Immunity ability.
    /// Checks that Immunity is not already activated and the mouse has 
    /// sufficient Happiness and sets the mouse Immunity state.
    /// </summary>
    public void ActivateImmunity()
    {
        if (mouse.Immunity) return;
#if UNITY_EDITOR
#else
        if (mouse.Happiness < player.PAbilities.Immunity.Cost) return;
#endif
        mouseIsImmune = true;
        mouse.Immunity = true;
        abilityLastActivatedTimes[AbilityName.Immunity] = DateTime.Now;
        mouse.Happiness -= player.PAbilities.Immunity.Cost;

        NewGameMessage("Immunity activated!");
    }

    /// <summary>
    /// Function to be called when a player activates the Fearless ability.
    /// Checks that Fearless is not already activated and the mouse
    /// has sufficient Happiness and sets the mouse Fearless state.
    /// </summary>
    public void ActivateFearless()
    {
        if (mouse.Fearless) return;
#if UNITY_EDITOR
#else 
        if (mouse.Happiness < player.PAbilities.Fearless.Cost) return; 
#endif
        mouseIsFearless = true;
        mouse.Fearless = true;
        abilityLastActivatedTimes[AbilityName.Fearless] = DateTime.Now;
        mouse.Happiness -= player.PAbilities.Fearless.Cost;

        NewGameMessage("Fearless activated!");
    }

    /// <summary>
    /// Function to be called when the player activates the Treats Galore ability.
    /// Checks that Treats Galore is not already activated and the mouse has
    /// sufficient Happiness. Chooses a random food above a certain point threshold
    /// specified in the ability level and boosts its max food count and spawn weight 
    /// through the associated FoodController instance.
    /// </summary>
    public void ActivateTreatsGalore()
    {
        Debug.LogWarning("Activating treats galore.");
        if (treatsGaloreIsActive) return;

#if UNITY_EDITOR
#else
        if (mouse.Happiness < player.PAbilities.TreatsGalore.Cost) return; 
#endif
        var goodFoods = foodController.FoodValues.Where(food => food.Value > player.PAbilities.TreatsGalore.PointThreshold).ToList();
        var boostedFood = goodFoods[new Random().Next(goodFoods.Count)].Key;
        tgOriginalCount = foodController.getMaxFoodCount(boostedFood);
        foodController.setMaxFoodCount(boostedFood, tgOriginalCount * player.PAbilities.TreatsGalore.SpawnLimitMultiplier);
        tgOriginalWeight = foodController.getFoodSpawnWeight(boostedFood);
        foodController.setFoodSpawnWeight(boostedFood, tgOriginalWeight * player.PAbilities.TreatsGalore.SpawnWeightMultiplier);
        treatsGaloreBoostedFood = boostedFood;
        treatsGaloreIsActive = true;
        abilityLastActivatedTimes[AbilityName.TreatsGalore] = DateTime.Now;
        mouse.Happiness -= player.PAbilities.TreatsGalore.Cost;

        NewGameMessage("Treats Galore activated!");
        Debug.Log(string.Format("{0} activated Treats Galore and boosted {1}", player.Name, boostedFood));
    }

    /// <summary>
    /// Function to be called when the player activates the Fat Mouse ability.
    /// Checks that Fat Mouse is not already activated and the mouse has 
    /// sufficient Happiness. Sets the mouse GrowthAbility to the multiplier
    /// specified by the ability level.
    /// </summary>
    public void ActivateFatMouse()
    {
        if (mouseIsFat) return;
#if UNITY_EDITOR
#else
        if (mouse.Happiness < player.PAbilities.FatMouse.Cost) return; 
#endif
        mouseIsFat = true;
        mouse.GrowthAbility = player.PAbilities.FatMouse.WeightMultiplier;
        abilityLastActivatedTimes[AbilityName.FatMouse] = DateTime.Now;
        mouse.Happiness -= player.PAbilities.FatMouse.Cost;

        NewGameMessage("Fat Mouse activated!");
        Debug.Log(string.Format("{0} activated Fat Mouse. Weight gain multiplier: {1}", player.Name, mouse.GrowthAbility));
    }

    /// <summary>
    /// Function to be called when a player targets another player with
    /// the Scary Cat ability. Checks that the mouse has enough Happiness and 
    /// calls the RpcReceiveScaryCat function on the targeted player.
    /// </summary>
    public void ActivateScaryCat()
    {
        if (targetedPlayer.Length == 0) return;
#if UNITY_EDITOR
#else
        if (mouse.Happiness < player.PAbilities.ScaryCat.Cost) return; 
#endif
        CmdDispatchScaryCat(player.Name, targetedPlayer, player.PAbilities.ScaryCat.Duration, player.PAbilities.ScaryCat.HappinessReduction, player.PAbilities.ScaryCat.WeightReduction);
        mouse.Happiness -= player.PAbilities.ScaryCat.Cost;

        NewGameMessage("Sent Scary Cat to " + targetedPlayer + "!");
    }

    [Command]
    public void CmdDispatchScaryCat(string caller, string target, int duration, int happinessReduction, int weightReduction)
    {
        if (!isServer) return;
        var victim = GameObject.FindGameObjectsWithTag("Player").First(o => o.GetComponent<Player>().Name.Equals(target));
        victim.GetComponent<AbilityController>().RpcReceiveScaryCat(caller, duration, happinessReduction, weightReduction);
    }

    /// <summary>
    /// Function to be called when a player is affected by another player's
    /// Scary Cat ability. May send the player's mouse offscreen and drop its
    /// Weight and Happiness depending on whether the player's Fearless ability
    /// is activated and its level.
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="happinessReduction"></param>
    /// <param name="weightReduction"></param>
    [ClientRpc]
    public void RpcReceiveScaryCat(string caller, int duration, int happinessReduction, int weightReduction)
    {
        if (!isLocalPlayer) return;
        levelController.ScaryCatAnimation();

        if (mouseIsOffscreen)
        {
            // "A scary cat came over, but your mouse wasn't around!"
        }
        else
        {
            if (mouseIsFearless)
            {
                if (player.PAbilities.Fearless.DropHappiness && player.PAbilities.Fearless.DropWeight)
                {
                    // "A scary cat came over, but your mouse stood its ground. However, its happiness and weight still dropped
                    mouse.Happiness -= happinessReduction;
                    mouse.Weight -= weightReduction;
                }
                else if (player.PAbilities.Fearless.DropHappiness)
                {
                    // "A scary cat came over, but your mouse bravely stood its ground. However, its happiness still dropped."
                    mouse.Happiness -= happinessReduction;
                }
                else
                {
                    // "A scary cat came over, but your mouse was completely nonchalant."
                }
            }
            else
            {
                // "A scary cat came over, and your mouse ran away! Its happiness and weight also dropped."
                mouse.Happiness -= happinessReduction;
                mouse.Weight -= weightReduction;
                mouse.Offscreen = true;
                mouseIsOffscreen = true;
                abilityLastActivatedTimes[AbilityName.ScaryCat] = DateTime.Now;
                scaryCatDuration = duration;
            }
        }

        NewGameMessage(caller + " sent a Scary Cat over!");
    }

    /// <summary>
    /// Function to be called when a player targets another player with the Beastly Buffet ability. 
    /// Checks that the mouse has sufficient Happiness and calls the RpcReceiveBeastlyBuffet function
    /// on the targeted player.
    /// </summary>
    public void ActivateBeastlyBuffet()
    {
        if (targetedPlayer.Length == 0) return;
#if UNITY_EDITOR
#else
        if (mouse.Happiness < player.PAbilities.BeastlyBuffet.Cost) return; 
#endif
        CmdDispatchBeastlyBuffet(player.Name, targetedPlayer, player.PAbilities.BeastlyBuffet.Duration, player.PAbilities.BeastlyBuffet.PointThreshold, player.PAbilities.BeastlyBuffet.SpawnLimitMultiplier, player.PAbilities.BeastlyBuffet.SpawnWeightMultiplier);
        mouse.Happiness -= player.PAbilities.BeastlyBuffet.Cost;

        NewGameMessage("Sent Beastly Buffet to " + targetedPlayer + "!");
    }

    /// <summary>
    /// Command which is called on the server when a player target another player with Beastly Buffet.
    /// Looks for the target player and triggers an RPC call on it to call the Receive function.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="duration"></param>
    /// <param name="pointThreshold"></param>
    /// <param name="spawnLimitMultiplier"></param>
    /// <param name="spawnWeightMultiplier"></param>
    [Command]
    public void CmdDispatchBeastlyBuffet(string caller, string target, int duration, int pointThreshold, int spawnLimitMultiplier, int spawnWeightMultiplier)
    {
        if (!isServer) return;
        var victim = GameObject.FindGameObjectsWithTag("Player").First(p => p.GetComponent<Player>().Name.Equals(target));
        victim.GetComponent<AbilityController>().RpcReceiveBeastlyBuffet(caller, duration, pointThreshold, spawnLimitMultiplier, spawnWeightMultiplier);
    }

    /// <summary>
    /// Function to be called when a player is affected by another player's Beastly Buffet ability.
    /// Chooses a random food below a certain point threshold specified by the attacker's ability
    /// and boosts its max food count and spawn weight through the associated Foodcontroller instance.
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="pointThreshold"></param>
    /// <param name="spawnLimitMultiplier"></param>
    /// <param name="spawnWeightMultiplier"></param>
    [ClientRpc]
    public void RpcReceiveBeastlyBuffet(string caller, int duration, int pointThreshold, int spawnLimitMultiplier, int spawnWeightMultiplier)
    {
        if (!isLocalPlayer) return;
        var badFoods = foodController.FoodValues.Where(food => food.Value < pointThreshold).ToList();
        var boostedFood = badFoods[new Random().Next(badFoods.Count)].Key;
        bbOriginalCount = foodController.getMaxFoodCount(boostedFood);
        foodController.setMaxFoodCount(boostedFood, bbOriginalCount * spawnLimitMultiplier);
        bbOriginalWeight = foodController.getFoodSpawnWeight(boostedFood);
        foodController.setFoodSpawnWeight(boostedFood, bbOriginalWeight * spawnWeightMultiplier);
        beastlyBuffetBoostedFood = boostedFood;
        beastlyBuffetIsActive = true;
        abilityLastActivatedTimes[AbilityName.BeastlyBuffet] = DateTime.Now;
        beastlyBuffetDuration = duration;

        NewGameMessage(caller + " sent a Beastly Buffet over!");
    }

    /// <summary>
    /// Use the Thief ability on the currently targeted player. 
    /// </summary>
    public void ActivateThief()
    {
        if (targetedPlayer.Length == 0) return;
#if UNITY_EDITOR
#else
        if (mouse.Happiness < player.PAbilities.Thief.Cost) return;
#endif
        CmdDispatchThief(player.Name, targetedPlayer, player.PAbilities.Thief.Duration, player.PAbilities.Thief.FoodUnitsTransferred);
        mouseIsThief = true;
        abilityLastActivatedTimes[AbilityName.Thief] = DateTime.Now;
        mouse.Happiness -= player.PAbilities.Thief.Cost;

        NewGameMessage("Stealing food from " + targetedPlayer + "!");
    }

    /// <summary>
    /// Command to look for the target player and call the Receive function on it.
    /// </summary>
    /// <param name="caller"></param>
    /// <param name="target"></param>
    /// <param name="duration"></param>
    /// <param name="foodUnitsTransferred"></param>
    [Command]
    public void CmdDispatchThief(string caller, string target, int duration, int foodUnitsTransferred)
    {
        if (!isServer) return;
        var victim = GameObject.FindGameObjectsWithTag("Player").First(p => p.GetComponent<Player>().Name.Equals(target));
        victim.GetComponent<AbilityController>().RpcReceiveThief(caller, duration, foodUnitsTransferred);
    }

    /// <summary>
    /// Called when another player uses the Thief ability on this mouse. Randomly chooses and removes some
    /// good food from on screen and transfers it to the caller.
    /// </summary>
    /// <param name="caller"></param>
    /// <param name="duration"></param>
    /// <param name="foodUnitsTransferred"></param>
    [ClientRpc]
    public void RpcReceiveThief(string caller, int duration, int foodUnitsTransferred)
    {
        if (!isLocalPlayer) return;
        if (foodController == null)
        {
            Debug.LogError("foodController is null");
            return;
        }
        var toTransfer = new List<string>();

        for (int i = 0; i < foodUnitsTransferred; i++)
        {
            try
            {
                var pieceOfFoodToBeStolen = foodController.GetComponentsInChildren<PoolMember>().First(o => o.gameObject.activeSelf && o.gameObject.GetComponent<Food>().NutritionalValue > 0);
                toTransfer.Add(pieceOfFoodToBeStolen.GetComponent<Food>().Type);
                pieceOfFoodToBeStolen.Deactivate();
            }
            catch (InvalidOperationException)
            {
                break;
            }
        }

        foodController.SpawnRate = 0.5f;

        mouseIsThiefVictim = true;
        lastStolenFrom = DateTime.Now;

        CmdDispatchStolenFood(caller, toTransfer.ToArray());

        NewGameMessage(caller + " stole some food from you!");
    }

    /// <summary>
    /// Called to return the stolen food back to the player who originally used Thief.
    /// </summary>
    /// <param name="caller"></param>
    /// <param name="foodsTaken"></param>
    [Command]
    public void CmdDispatchStolenFood(string caller, string[] foodsTaken)
    {
        if (!isServer) return;
        Debug.Log(caller + " stole " + string.Join(" ", foodsTaken));
        var thief = GameObject.FindGameObjectsWithTag("Player").First(o => o.GetComponent<Player>().Name.Equals(caller));
        thief.GetComponent<AbilityController>().RpcReceiveStolenFood(foodsTaken);
    }

    /// <summary>
    /// Client RPC to spawn food stolen by thief back on the original caller's screen.
    /// </summary>
    /// <param name="foodsTaken"></param>
    [ClientRpc]
    public void RpcReceiveStolenFood(string[] foodsTaken)
    {
        if (!isLocalPlayer) return;
        foreach (var food in foodsTaken)
        {
            foodController.SpawnFood(food);
        }
        foodController.SpawnRate = 2f;
    }

    //    [ClientRpc]
    //    public void RpcIncrementMouseWeight()
    //    {
    //        if (!isLocalPlayer) return;
    //        mouse.Weight++;
    //    }
    //
    //    [Command]
    //    public void CmdIncreaseScore()
    //    {
    //        if (!isServer) return;
    //        Debug.LogWarning("CmdIncreaseScore called as local player: " + isLocalPlayer.ToString());
    //        var players = GameObject.FindGameObjectsWithTag("Player");
    //        var targetPlayer = players.ElementAt(new Random().Next(players.Length));
    //        Debug.LogWarning(string.Format("Increasing score for player {0}", targetPlayer.GetComponent<Player>().Name));
    //        targetPlayer.GetComponent<AbilityController>().RpcIncrementMouseWeight();
    //    }
}
