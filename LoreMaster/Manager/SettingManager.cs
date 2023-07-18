using GlobalEnums;
using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Modules;
using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.LorePowers;
using LoreMaster.LorePowers.QueensGarden;
using LoreMaster.UnityComponents;
using Modding;
using On.HutongGames.PlayMaker.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LoreMaster.Manager;

/// <summary>
/// Game manager for modifying all things besides the lore and powers. (Fsm and stuff)
/// </summary>
public class SettingManager
{
    #region Members

    private readonly Dictionary<Area, List<MapZone>> _mapZones = new()
    {
        {Area.Dirtmouth, new(){MapZone.KINGS_PASS, MapZone.TOWN} },
        {Area.Crossroads, new(){MapZone.CROSSROADS, MapZone.TRAM_UPPER, MapZone.FINAL_BOSS, MapZone.SHAMAN_TEMPLE} },
        {Area.Greenpath, new(){MapZone.GREEN_PATH, MapZone.NOEYES_TEMPLE}},
        {Area.FungalWastes, new(){MapZone.WASTES, MapZone.MANTIS_VILLAGE, MapZone.QUEENS_STATION} },
        {Area.FogCanyon, new(){MapZone.FOG_CANYON, MapZone.OVERGROWN_MOUND, MapZone.MONOMON_ARCHIVE} },
        {Area.KingdomsEdge, new(){MapZone.COLOSSEUM, MapZone.OUTSKIRTS, MapZone.HIVE, MapZone.TRAM_LOWER, MapZone.WYRMSKIN} },
        {Area.Deepnest, new(){MapZone.DEEPNEST, MapZone.TRAM_LOWER, MapZone.BEASTS_DEN, MapZone.RUINED_TRAMWAY} },
        {Area.WaterWays, new(){MapZone.WATERWAYS, MapZone.GODS_GLORY, MapZone.GODSEEKER_WASTE, MapZone.ISMAS_GROVE} },
        {Area.Cliffs, new(){MapZone.CLIFFS, MapZone.JONI_GRAVE } },
        {Area.AncientBasin, new(){MapZone.BONE_FOREST, MapZone.PALACE_GROUNDS, MapZone.TRAM_LOWER, MapZone.ABYSS, MapZone.ABYSS_DEEP} },
        {Area.CityOfTears, new(){MapZone.CITY, MapZone.SOUL_SOCIETY, MapZone.LURIENS_TOWER, MapZone.LOVE_TOWER, MapZone.MAGE_TOWER } },
        {Area.RestingGrounds, new(){MapZone.RESTING_GROUNDS, MapZone.BLUE_LAKE, MapZone.TRAM_UPPER, MapZone.GLADE} },
        {Area.QueensGarden, new(){MapZone.ROYAL_GARDENS, MapZone.ROYAL_QUARTER} },
        {Area.WhitePalace, new(){MapZone.WHITE_PALACE, MapZone.NONE} },
        {Area.Peaks, new(){MapZone.PEAK, MapZone.MINES, MapZone.CRYSTAL_MOUND} }
    };

    private bool _fromMenu;

    private int[] _elderbugRewardStages = new int[] { 10, 15, 20, 30, 40, 50, 55, 60 };

    #endregion

    #region Constructors

    public SettingManager()
    {
        Instance = this;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the current area the player is in.
    /// </summary>
    public Area CurrentArea { get; set; }

    /// <summary>
    /// Gets or sets the flag, that indicates if the yellow mushroom effect should not use the nausea effect.
    /// </summary>
    public bool DisableYellowMushroom { get; set; }

    /// <summary>
    /// Gets or sets the flag that indicates if the <see cref="GrassBombardementPower"/> bombs can be cast via quickcast.
    /// </summary>
    public bool BombQuickCast { get; set; } = true;

    /// <summary>
    /// Gets or sets the value which indicates what has to be done, to open the black egg temple.
    /// </summary>
    public BlackEggTempleCondition EndCondition { get; set; }

    /// <summary>
    /// Gets or sets the lore needed to open the black egg temple door.
    /// </summary>
    public int NeededLore { get; set; } = 0;

    /// <summary>
    /// The running instance of this manager.
    /// </summary>
    public static SettingManager Instance { get; set; }

    /// <summary>
    /// Gets or sets the current game mode.
    /// </summary>
    public GameMode GameMode { get; set; }

    /// <summary>
    /// Gets or sets the current state of elderbug for the quest.
    /// </summary>
    public int ElderbugState { get; set; } = 0;

    #endregion

    #region Event handler

    #region Game Management

    private void StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
    {
        try
        {
            orig(self, permaDeath, bossRush);

            ItemChangerMod.Modules.GetOrAdd<MenderbugUnlock>();
            _fromMenu = true;
            LoreManager.Instance.JokerScrolls = -1;
            LoreManager.Instance.CleansingScrolls = -1;
            ElderbugState = 0;
            ModHooks.SetPlayerBoolHook += PlayerData_SetBool;
            On.PlayerData.IntAdd += PlayerData_IntAdd;
            ModHooks.GetPlayerBoolHook += CheckIfInventoryAccessible;

            SendEventByName.OnEnter += EndAllPowers;
            On.PlayMakerFSM.OnEnable += FsmEdits;
            AbstractItem.BeforeGiveGlobal += GiveLoreItem;
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError(exception.Message);
        }
    }

    private void ContinueGame(On.UIManager.orig_ContinueGame orig, UIManager self)
    {
        _fromMenu = true;
        orig(self);
        if (GameMode == GameMode.Disabled)
            return;
        try
        {
            ModHooks.SetPlayerBoolHook += PlayerData_SetBool;
            On.PlayerData.IntAdd += PlayerData_IntAdd;
            ModHooks.GetPlayerBoolHook += CheckIfInventoryAccessible;
            On.PlayMakerFSM.OnEnable += FsmEdits;
            SendEventByName.OnEnter += EndAllPowers;
            AbstractItem.BeforeGiveGlobal += GiveLoreItem;
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("An error occured in continue: " + exception.Message);
            LoreMaster.Instance.LogError("An error occured in continue: " + exception.StackTrace);
        }
    }

    private IEnumerator ReturnToMenu(On.GameManager.orig_ReturnToMainMenu orig, GameManager self, GameManager.ReturnToMainMenuSaveModes saveMode, Action<bool> callback)
    {
        try
        {
            if (GameMode != GameMode.Disabled)
            {
                PowerManager.DisableAllPowers();
                LoreMaster.Instance.Handler.StopAllCoroutines();
                CurrentArea = Area.None;
                On.PlayMakerFSM.OnEnable -= FsmEdits;
                SendEventByName.OnEnter -= EndAllPowers;
                AbstractItem.BeforeGiveGlobal -= GiveLoreItem;
                ModHooks.SetPlayerBoolHook -= PlayerData_SetBool;
                On.PlayerData.IntAdd -= PlayerData_IntAdd;
                ModHooks.GetPlayerBoolHook -= CheckIfInventoryAccessible;
            }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("An error occured in returning: " + exception.Message);
            LoreMaster.Instance.LogError(exception.StackTrace);
        }
        return orig(self, saveMode, callback);
    }

    #endregion

    #region Ingame Modifications

    private bool PlayerData_SetBool(string name, bool orig)
    {
        if (string.Equals(name, "Read"))
            LoreManager.Instance.CanRead = orig;
        else if (string.Equals(name, "Listen"))
            LoreManager.Instance.CanListen = orig;
        else if (string.Equals(name, "LorePage"))
            PowerManager.ControlState = PowerControlState.ReadAccess;
        else if (string.Equals(name, "LorePageControl"))
            PowerManager.ControlState = PowerControlState.ToggleAccess;
        else if (name == "PopLore")
            PowerManager.GetPowerByKey("POP", out Power power);
        return orig;
    }

    private bool CheckIfInventoryAccessible(string name, bool orig)
    {
        if (string.Equals(name, "LoreArtifact"))
            orig = PowerManager.ControlState != PowerControlState.NotObtained;
        return orig;
    }

    private void PlayerData_IntAdd(On.PlayerData.orig_IntAdd orig, PlayerData self, string intName, int amount)
    {
        orig(self, intName, amount);
        if (string.Equals(intName, "JokerScroll"))
        {
            LoreManager.Instance.JokerScrolls += LoreManager.Instance.JokerScrolls == -1 ? amount + 1 : amount;
            LorePage.UpdateLorePage();
        }
        else if (string.Equals(intName, "CleansingScroll"))
        {
            LoreManager.Instance.CleansingScrolls += LoreManager.Instance.CleansingScrolls == -1 ? amount + 1 : amount;
            LorePage.UpdateLorePage();
        }
    }

    /// <summary>
    /// Event handler used for adjusting the active powers.
    /// </summary>
    private void SceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        try
        {
            if (GameMode == GameMode.Disabled)
                return;
            if (GameManager.instance != null && GameManager.instance.IsGameplayScene() && !string.Equals(arg1.name, "Quit_To_Menu"))
            {
                // Resets the flag, that powers can be used.
                PowerManager.CanPowersActivate = true;
                // Move the bench slightly to the side to allow easier pick up of Elderbugs items.
                if (GameMode != GameMode.Normal && string.Equals(arg1.name, "Town")
                    && GameObject.Find("RestBench") != null)
                    GameObject.Find("RestBench").transform.position += new Vector3(3f, 0f);

                // Initialization
                if (_fromMenu)
                {
                    if (string.Equals(arg1.name.ToLower(), "town"))
                    {
                        Transform elderbug = GameObject.Find("_NPCs/Elderbug").transform;
                        elderbug.localScale = new(2f, 2f, 2f);
                        elderbug.localPosition = new(126.36f, 12.35f, 0f);
                    }

                    try
                    {
                        // Allow the player to read the resting grounds tablet.
                        if (ItemChangerMod.Modules != null && ItemChangerMod.Modules.Modules != null)
                        {
                            if (ItemChangerMod.Modules.Modules.FirstOrDefault(x => string.Equals(x?.Name, "DreamNailCutsceneEvent")) is not DreamNailCutsceneEvent cutscene)
                                ItemChangerMod.Modules.Modules.Add(new DreamNailCutsceneEvent() { Faster = false });
                            else
                                cutscene.Faster = false;
                        }
                        else
                            LoreMaster.Instance.LogWarn("Couldn't find item changer modules. This might cause the dreamer tablet to be unreadable!");
                    }
                    catch (Exception exception)
                    {
                        LoreMaster.Instance.LogError("An error occured while modifying IC: " + exception.Message);
                    }

                    // Load in changes from the options file (if it exists)
                    LoadOptions();
                }
                LoreMaster.Instance.Handler.StartCoroutine(ManageSceneActions());
            }
            // Reset curses (in case a rando is done and then a normal game)
            else if (string.Equals(arg1.name, "Menu_Title"))
            {
                LoreManager.Instance.CanRead = true;
                LoreManager.Instance.CanListen = true;
            }
        }
        catch (Exception error)
        {
            LoreMaster.Instance.LogError("Error while trying to load new scene: " + error.Message);
            LoreMaster.Instance.LogError(error.StackTrace);
        }
    }

    /// <summary>
    /// Event handler that handles the fsm edits.
    /// </summary>
    private void FsmEdits(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        try
        {
            if (string.Equals(self.gameObject.name, "Elderbug") && string.Equals(self.FsmName, "npc_control"))
            {
                self.transform.localScale = new(2f, 2f, 2f);
                self.transform.localPosition = new(126.36f, 12.35f, 0f);
            }
            else if (string.Equals(self.FsmName, "Shop Region") && !LoreManager.Instance.CanListen)
                self.GetState("Out Of Range").ClearTransitions();
            else if (string.Equals(self.FsmName, "Thorn Counter"))
            {
                PowerManager.GetPowerByKey("QUEEN", out Power power2, false);
                ((QueenThornsPower)power2).ModifyThorns(self);
            }
            else if (EndCondition != BlackEggTempleCondition.Dreamers && string.Equals(self.FsmName, "Control")
                && string.Equals(self.gameObject.name, "Final Boss Door"))
            {
                self.AddState(new FsmState(self.Fsm)
                {
                    Name = "Lore Condition",
                    Actions = new FsmStateAction[]
                    {
                        new Lambda(() =>
                        {
                            if (EndCondition == BlackEggTempleCondition.Lore)
                                self.SendEvent(PowerManager.ObtainedPowers.Count >= NeededLore ? "READY" : "FINISHED");
                            else
                                self.SendEvent(PowerManager.ObtainedPowers.Count >= NeededLore && PlayerData.instance.GetInt("guardiansDefeated") >= 3
                                    ? "READY" : "FINISHED");
                        })
                    }
                });
                self.GetState("Lore Condition").AddTransition("READY", "Ready");
                self.GetState("Lore Condition").AddTransition("FINISHED", "Idle");
                self.GetState("Init").AdjustTransition("READY", "Lore Condition");
                self.GetState("Init").AdjustTransition("FINISHED", "Lore Condition");
            }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Couldn't modify fsm " + self.FsmName + ": " + exception.Message + " at " + exception.StackTrace);
        }

        orig(self);
    }

    /// <summary>
    /// Event handler to adjust the Elderbug questline.
    /// </summary>
    private void GiveLoreItem(ReadOnlyGiveEventArgs itemData)
    {
        try
        {
            if (itemData.Placement.Name.StartsWith("Elderbug_Reward_"))
            {
                ElderbugState = !itemData.Placement.Name.EndsWith("1")
                    ? (itemData.Placement.Name.EndsWith("2")
                        ? 4
                        : Convert.ToInt32(itemData.Placement.Name.Substring(16)) + 3)
                    : 2;
                GameObject elderBug = GameObject.Find("Elderbug");
                if (elderBug != null && elderBug.GetComponent<BoxCollider2D>() is BoxCollider2D collider)
                    collider.size = new(3.8361f, 0.2408f);
            }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("An error occured while modifying a Elderbug " + exception.Message);
            LoreMaster.Instance.LogError(exception.StackTrace);
        }
    }

    /// <summary>
    /// Event handler to disable all powers after a final boss has been killed.
    /// </summary>
    private void EndAllPowers(SendEventByName.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SendEventByName self)
    {
        orig(self);
        if (string.Equals(self.sendEvent.Value, "ALL CHARMS END") && (string.Equals(self.Fsm.GameObjectName, "Hollow Knight Boss")
            || string.Equals(self.Fsm.GameObjectName, "Radiance") || string.Equals(self.Fsm.GameObjectName, "Absolute Radiance")))
            PowerManager.DisableAllPowers();
    }

    #endregion

    #endregion

    #region Methods

    /// <summary>
    /// Initializes the manager.
    /// </summary>
    public void Initialize()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
        On.UIManager.StartNewGame += StartNewGame;
        On.UIManager.ContinueGame += ContinueGame;
        On.GameManager.ReturnToMainMenu += ReturnToMenu;
    }

    /// <summary>
    /// Loads the options from the option file (if it exists)
    /// </summary>
    private void LoadOptions()
    {
        string optionFile = Path.Combine(Path.GetDirectoryName(typeof(MindBlast).Assembly.Location), "options_" + GameManager.instance.profileID + ".txt");
        try
        {
#if DEBUG
            //foreach (Power power in PowerManager.GetAllPowers())
            //{
            //    if (!PowerManager.ActivePowers.Contains(power))
            //        PowerManager.ActivePowers.Add(power);
            //    power.Tag = PowerTag.Global;
            //}
#endif
            if (File.Exists(optionFile))
            {
                using StreamReader reader = new(optionFile);

                string headline = reader.ReadLine();
                if (headline.ToLower().Contains("%override%"))
                    PowerManager.ObtainedPowers.Clear();
                else if (!headline.ToLower().Contains("%modify%"))
                {
                    LoreMaster.Instance.LogError("Invalid option file. Use %override% or %modify% in the first line.");
                    return;
                };
                LoreMaster.Instance.Log("Apply option file");
                while (!reader.EndOfStream)
                {
                    string currentLine = reader.ReadLine().Replace(" ", "").ToLower();
                    if (currentLine.Contains("#"))
                        continue;
                    string powerName = "";
                    currentLine = string.Concat(currentLine.SkipWhile(x => !x.Equals('%')).Skip(1));
                    foreach (char letter in currentLine)
                    {
                        if (letter == '%')
                            break;
                        powerName += letter;
                    }

                    if (!PowerManager.GetPowerByName(powerName, out Power power, true, false) && !PowerManager.GetPowerByKey(powerName, out power, false))
                        continue;
                    // Skip the name
                    currentLine = currentLine.Substring(powerName.Length + 1);
                    string tagText = string.Concat(currentLine.TakeWhile(x => !x.Equals('|')));
                    tagText = char.ToUpper(tagText[0]) + tagText.Substring(1);
                    if (!Enum.TryParse(tagText, out PowerRank tag))
                        continue;

                    currentLine = currentLine.Substring(tagText.Length);
                    if (currentLine.Contains("add") && !PowerManager.ObtainedPowers.Contains(power))
                    {
                        LoreMaster.Instance.Log($"Add {power.PowerName} to player.");
                        PowerManager.ObtainedPowers.Add(power);
                    }
                }
            }
            else
                LoreMaster.Instance.LogDebug("Couldn't find option file");
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Couldn't load option file: " + exception.Message);
        }
    }

    /// <summary>
    /// Manages the activation/deactivation of powers and scene change trigger.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ManageSceneActions()
    {
        yield return new WaitForFinishedEnteringScene();
        // Just to make sure the controller exist. A desperate attempt.
        if (_fromMenu && (HeroController.instance == null || !HeroController.instance.acceptingInput))
            yield return new WaitUntil(() => HeroController.instance != null && HeroController.instance.acceptingInput);
        Area newArea = CurrentArea;

        // Figure out the current Map zone. (Dream world counts as the same area)
        if (!Enum.TryParse(GameManager.instance.GetCurrentMapZone(), out MapZone newMapZone))
            LoreMaster.Instance.LogError("Couldn't convert map zone to enum. Value: " + GameManager.instance.GetCurrentMapZone());
        // Dreams will be counted as the zone where you entered them.
        else if (newMapZone != MapZone.DREAM_WORLD)
        {
            bool foundResult = false;
            foreach (Area area in _mapZones.Keys)
                if (_mapZones[area].Contains(newMapZone))
                {
                    newArea = area;
                    foundResult = true;
                    break;
                }
            if (!foundResult)
                LoreMaster.Instance.LogError("Couldn't find area: " + newMapZone);
        }

        // Initialization taken when we entered from the menu.
        if (_fromMenu)
        {
            // Ensures that the flower doesn't get destroyed instantly when powers apply fake damage.
            PlayMakerFSM fsm = HeroController.instance.proxyFSM;
            fsm.GetState("Flower?").ReplaceAction(new Lambda(() =>
            {
                if (Power.FakeDamage
                || !PlayerData.instance.GetBool(nameof(PlayerData.instance.hasXunFlower))
                || PlayerData.instance.GetBool(nameof(PlayerData.instance.xunFlowerBroken)))
                {
                    if (Power.FakeDamage)
                        Power.FakeDamage = false;
                    fsm.SendEvent("FINISHED");
                }
            })
            { Name = "Fake Damage" }, 0);

            _fromMenu = false;
            PowerManager.UpdateTracker(newArea);
        }
        LorePage.UpdateLorePage();

        // Execute all actions that powers want to do when the scene changes.
        PowerManager.ExecuteSceneActions();
        CurrentArea = newArea;
    }

    #endregion
}
