using GlobalEnums;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Modules;
using ItemChanger.Placements;
using ItemChanger.UIDefs;
using LoreMaster.CustomItem;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
using LoreMaster.LorePowers;
using LoreMaster.LorePowers.CityOfTears;
using LoreMaster.LorePowers.FungalWastes;
using LoreMaster.LorePowers.QueensGarden;
using LoreMaster.LorePowers.WhitePalace;
using LoreMaster.Properties;
using LoreMaster.Randomizer;
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
internal class SettingManager
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

    #endregion

    #region Constructors

    public SettingManager()
    {
        Instance = this;
    }

    #endregion

    #region Properties

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
    public RandomizerEndCondition EndCondition { get; set; }

    /// <summary>
    /// The running instance of this manager.
    /// </summary>
    public static SettingManager Instance { get; set; }

    #endregion

    #region Event handler

    #region Game Management

    private void StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
    {
        try
        {
            ItemChangerMod.CreateSettingsProfile(false);
            ItemManager.ResetItems();
            On.PlayMakerFSM.OnEnable += FsmEdits;
            orig(self, permaDeath, bossRush);
            ModHooks.SetPlayerBoolHook += TrackPathOfPain;
            _fromMenu = true;
            PowerManager.ResetPowers();
#if DEBUG
            //foreach (Power power in PowerManager.GetAllPowers())
            //    if (!PowerManager.ActivePowers.Contains(power))
            //        PowerManager.ActivePowers.Add(power);
#endif
            ModHooks.LanguageGetHook += LoreManager.Instance.GetText;
            On.DeactivateIfPlayerdataTrue.OnEnable += ForceMyla;
            On.DeactivateIfPlayerdataFalse.OnEnable += PreventMylaZombie;
            SendEventByName.OnEnter += EndAllPowers;
            if (ModHooks.GetMod("Randomizer 4") is Mod)
            {
                AbstractItem.BeforeGiveGlobal += GiveLoreItem;
                RandomizerManager.CheckForRandoFile();
            }
            else
            {
                LoreManager.Instance.CanRead = true;
                LoreManager.Instance.CanListen = true;
            }
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
        if (PlayerData.instance.GetBool("killedBindingSeal") && !PowerManager.ActivePowers.Any(x => x is SacredShellPower))
            PowerManager.GetPowerByKey("EndOfPathOfPain", out Power power);
        else
            ModHooks.SetPlayerBoolHook += TrackPathOfPain;

        ModHooks.LanguageGetHook += LoreManager.Instance.GetText;
        On.PlayMakerFSM.OnEnable += FsmEdits;
        On.DeactivateIfPlayerdataTrue.OnEnable += ForceMyla;
        On.DeactivateIfPlayerdataFalse.OnEnable += PreventMylaZombie;
        SendEventByName.OnEnter += EndAllPowers;
        if (ModHooks.GetMod("Randomizer 4") is Mod)
        {
            AbstractItem.BeforeGiveGlobal += GiveLoreItem;
            RandomizerManager.CheckForRandoFile();
        }
        else
        {
            LoreManager.Instance.CanRead = true;
            LoreManager.Instance.CanListen = true;
        }
    }

    private IEnumerator ReturnToMenu(On.GameManager.orig_ReturnToMainMenu orig, GameManager self, GameManager.ReturnToMainMenuSaveModes saveMode, Action<bool> callback)
    {
        PowerManager.DisableAllPowers();
        LoreMaster.Instance.Handler.StopAllCoroutines();
        CurrentArea = Area.None;
        On.PlayMakerFSM.OnEnable -= FsmEdits;
        On.DeactivateIfPlayerdataTrue.OnEnable -= ForceMyla;
        On.DeactivateIfPlayerdataFalse.OnEnable -= PreventMylaZombie;
        ModHooks.SetPlayerBoolHook -= TrackPathOfPain;
        SendEventByName.OnEnter -= EndAllPowers;
        ModHooks.LanguageGetHook -= LoreManager.Instance.GetText;
        if (ModHooks.GetMod("Randomizer 4", true) is Mod mod)
            AbstractItem.BeforeGiveGlobal -= GiveLoreItem;
        return orig(self, saveMode, callback);
    }

    #endregion

    #region Ingame Modifications

    /// <summary>
    /// Event handler, for the PoP power.
    /// </summary>
    private bool TrackPathOfPain(string name, bool orig)
    {
        if (name.Equals("killedBindingSeal") && orig)
        {
            PowerManager.GetPowerByKey("EndOfPathOfPain", out Power power);
            // This hook is no longer needed after obtaining the power.
            ModHooks.SetPlayerBoolHook -= TrackPathOfPain;
        }
        return orig;
    }

    /// <summary>
    /// Event handler used for adjusting the active powers.
    /// </summary>
    private void SceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        try
        {
            if (GameManager.instance != null && GameManager.instance.IsGameplayScene() && !string.Equals(arg1.name, "Quit_To_Menu"))
            {
                // Resets the flag, that powers can be used.
                PowerManager.CanPowersActivate = true;
                // Spawn extra lore tablet.
                if (string.Equals(arg1.name, "Ruins1_30"))
                {
                    GameObject tablet = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Glow Response Mage Computer"]);
                    tablet.name = "Mage_Computer_2";
                    tablet.transform.localPosition = new(70f, 6.21f, .02f);
                    tablet.SetActive(true);

                    GameObject inspectRegion = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Inspect Region"]);
                    inspectRegion.name = "Computer_2_Inspect";
                    inspectRegion.transform.localPosition = new(70f, 4.31f, .006f);
                    inspectRegion.SetActive(true);
                    inspectRegion.LocateMyFSM("inspect_region").FsmVariables.FindFsmString("Game Text Convo").Value = "MAGE_COMP_02";
                }
                // Initialization
                if (_fromMenu)
                {
#if DEBUG

#endif
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
                        LoreMaster.Instance.LogError("An error occured while modifying dreamer cutscene: " + exception.Message);
                        LoreMaster.Instance.LogError("An error occured while modifying dreamer cutscene: " + exception.StackTrace);
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
        // The quirrel in peaks has 3 fsm (don't ask) that indicates if he can be there: If he has been encountered in archives, if you have superdash or if he has just left the mines.
        // We remove all those checks.
        if (string.Equals(self.gameObject.name, "Quirrel Mines") && string.Equals(self.FsmName, "FSM"))
            self.GetState("Check").RemoveTransitionsTo("Destroy");
        // The game asks for the language key for the fountain once you entered the room. To not give the power immediatly, we bind it on the inspect instead.
        else if (string.Equals(self.gameObject.name, "Fountain Inspect") && string.Equals(self.FsmName, "Conversation Control"))
        {
            string placeHolder = string.Empty;
            self.GetState("Anim End").ReplaceAction(new Lambda(() => PowerManager.GetPowerByKey("FOUNTAIN_PLAQUE_DESC", out Power power)) { Name = "Fountain Power" });
        }
        // The game asks for the language key for the dreamer tablet once you entered the room. To not give the power immediatly, we bind it on the inspect instead.
        else if (string.Equals(self.gameObject.name, "Dreamer Plaque Inspect") && string.Equals(self.FsmName, "Conversation Control"))
        {
            string placeHolder = string.Empty;
            self.GetState("Anim End").ReplaceAction(new Lambda(() => PowerManager.GetPowerByKey("DREAMERS_INSPECT_RG5", out Power power)) { Name = "Dreamer Power" });
        }
        // Prevent Moss Prophet from dying
        else if (string.Equals(self.gameObject.name, "Moss Cultist") && string.Equals(self.FsmName, "FSM"))
        {
            self.GetState("Check").RemoveTransitionsTo("Destroy");
            self.gameObject.GetComponent<BoxCollider2D>().enabled = true;
        }
        // Deactives moss prophet corpse, so that it doesn't block the living one.
        else if (string.Equals(self.gameObject.name, "corpse set") && string.Equals(self.FsmName, "FSM"))
            self.gameObject.FindChild("corpse0000").SetActive(false);
        else if (string.Equals(self.gameObject.name, "Elderbug"))
        {
            if (string.Equals(self.FsmName, "npc_control"))
            {
                self.transform.localScale = new(2f, 2f, 2f);
                self.transform.localPosition = new(126.36f, 12.35f, 0f);
                if (!LoreManager.Instance.CanListen)
                    self.GetState("Idle").ClearTransitions();
            }
            else if (string.Equals(self.FsmName, "Conversation Control"))
            {
                self.GetState("Convo Choice").ClearTransitions();
                self.GetState("Convo Choice").Actions = new HutongGames.PlayMaker.FsmStateAction[]
                {
                    new Lambda(() => self.SendEvent("FINISHED"))
                };
                self.GetState("Convo Choice").AddTransition("FINISHED", "Meeting Choice");
            }
        }
        // Prevent killing ghosts with abilities.
        else if (string.Equals(self.FsmName, "ghost_npc_death"))
        {
            string ghostName = "";
            try
            {
                ghostName = self.gameObject.LocateMyFSM("Conversation Control").FsmVariables.FindFsmString("Ghost Name").Value.ToUpper();
                if (string.Equals(ghostName, "POGGY") || string.Equals(ghostName, "HIVEQUEEN")
                    || string.Equals(ghostName, "JONI") || string.Equals(ghostName, "GRAVEDIGGER")
                    || string.Equals(ghostName, "GRASSHOPPER") || string.Equals(ghostName, "MARISSA"))
                {
                    self.GetState("Revek?").ReplaceAction(new Lambda(() =>
                    {
                        // If rando is used, to prevent locking out of progress, ghost with abilities will became immune entirely.
                        if (!PowerManager.HasObtainedPower(ghostName, false))
                            self.SendEvent("IMMUNE");
                        else
                            self.SendEvent(self.FsmVariables.FindFsmBool("z_Revek").Value ? "REVEK" : "FINISHED");
                    })
                    { Name = "Prevent Death" }, 0);
                    self.GetState("Revek?").AddTransition("IMMUNE", "Idle");
                }
            }
            catch (Exception exception)
            {
                LoreMaster.Instance.LogError("Error while modifying ghost: " + ghostName + ": " + exception.Message);
            }
        }
        // Prevent the player from reading lore tablets without the item (rando only)
        else if (string.Equals(self.FsmName, "Inspection") && !LoreManager.Instance.CanRead && PowerManager.GetPowerByKey(self.FsmVariables.FindFsmString("Convo Name")?.Value, out Power power, false))
            self.GetState("Init").ClearTransitions();
        // Prevent the player from reading lore tablets without the item (rando only)
        else if (string.Equals(self.FsmName, "inspect_region") && !LoreManager.Instance.CanRead && (PowerManager.GetPowerByKey(self.FsmVariables.FindFsmString("Game Text Convo")?.Value, out power, false)
            || string.Equals(self.gameObject.name, "Inspect Region Ghost")))
            self.GetState("Init").ClearTransitions();
        else if (string.Equals(self.FsmName, "npc_control")
            && ((!LoreManager.Instance.CanListen && (!string.Equals(self.gameObject.name, "Dreamer Plaque Inspect")
            && !string.Equals(self.gameObject.name, "Fountain Inspect") && !string.Equals(self.gameObject.name, "Fountain Donation")))
            || (!LoreManager.Instance.CanRead && (string.Equals(self.gameObject.name, "Dreamer Plaque Inspect")
            || string.Equals(self.gameObject.name, "Fountain Inspect") || string.Equals(self.gameObject.name, "Fountain Donation")))))
        {
            // There are a few exceptions with npc which we want to ignore.
            if (self.gameObject.LocateMyFSM("Conversation Control") != null && !(string.Equals(self.gameObject.name, "Moth NPC")
                || string.Equals(self.gameObject.name, "Corpse Inspect")
                || string.Equals(self.gameObject.name, "Dream Nail Get") || string.Equals(self.gameObject.name, "Centipede Inspect")
                || string.Equals(self.gameObject.name, "Goam Inspect") || string.Equals(self.gameObject.name, "Zap Bug Inspect")
                || string.Equals(self.gameObject.name, "AbyssTendril Inspect") || string.Equals(self.gameObject.name, "End Scene")
                || string.Equals(self.transform.parent?.name, "Dreamer Monomon") || string.Equals(self.gameObject.name, "Mage Door")
                || string.Equals(self.gameObject.name, "Love Door") || string.Equals(self.gameObject.name, "Jiji Door")
                || string.Equals(self.gameObject.name, "Waterways Machine") || string.Equals(self.transform.parent?.name, "Bathhouse Door")
                || string.Equals(self.gameObject.name, "Coffin") || string.Equals(self.gameObject.name, "Tram Call Box")))
                self.GetState("Idle").ClearTransitions();
        }
        else if (string.Equals(self.FsmName, "Stag Control") && !LoreManager.Instance.CanListen)
            self.GetState("Idle").ClearTransitions();
        else if (string.Equals(self.FsmName, "Shop Region") && !LoreManager.Instance.CanListen)
            self.GetState("Out Of Range").ClearTransitions();
        else if (string.Equals(self.FsmName, "Control") && string.Equals(self.gameObject.name, "Final Boss Door") && ModHooks.GetMod("Randomizer 4", true) is Mod mod)
            RandomizerManager.ModifyTempleDoor(self);
        else if (string.Equals(self.gameObject.name, "Ghost Activator") && self.transform.childCount > 0 && string.Equals("Ghost NPC Joni", self.transform.GetChild(0)?.name) && ModHooks.GetMod("Randomizer 4", true) is Mod)
            RandomizerManager.ModifyJoni(self);
        else if (string.Equals(self.FsmName, "Thorn Counter"))
        {
            PowerManager.GetPowerByKey("QUEEN", out power, false);
            ((QueenThornsPower)power).ModifyThorns(self);
        }

        orig(self);
    }

    /// <summary>
    /// Despawns mylas other versions.
    /// </summary>
    private void PreventMylaZombie(On.DeactivateIfPlayerdataFalse.orig_OnEnable orig, DeactivateIfPlayerdataFalse self)
    {
        if (self.gameObject.name.Contains("Zombie Myla") || string.Equals(self.gameObject.name, "Myla Crazy NPC"))
        {
            self.gameObject.SetActive(false);
            return;
        }
        orig(self);
    }

    /// <summary>
    /// Forces Myla (best character btw) to always appear, like she should.
    /// </summary>
    private void ForceMyla(On.DeactivateIfPlayerdataTrue.orig_OnEnable orig, DeactivateIfPlayerdataTrue self)
    {
        if (string.Equals(self.gameObject.name, "Miner"))
            return;
        orig(self);
    }

    /// <summary>
    /// Event handler to adjust the message and give the power of randomizer items.
    /// </summary>
    private void GiveLoreItem(ReadOnlyGiveEventArgs itemData)
    {
        try
        {
            //If focus is randomized but the lore tablet isn't, the lore tablet becomes unavailable, which is why we add the power to focus instead.
            if (itemData.Item.name.Equals("Focus") && ItemChanger.Internal.Ref.Settings.Placements.ContainsKey(LocationNames.Focus))
            {
                string text = string.Empty;
                LoreManager.Instance.ModifyText("TUT_TAB_01", ref text);
                if (itemData.Item.UIDef is BigUIDef big && !string.IsNullOrEmpty(text))
                    big.descTwo = new BoxedString(text.Replace("<br>", " "));
            }
            // If world sense is randomized but the lore tablet isn't, the lore tablet becomes unavailable, which is why we add the power to world sense instead.
            else if (itemData.Item.name.Equals("World_Sense") && ItemChanger.Internal.Ref.Settings.Placements.ContainsKey(LocationNames.World_Sense))
            {
                string text = string.Empty;
                LoreManager.Instance.ModifyText("COMPLETION_RATE_UNLOCKED", ref text);
                if (itemData.Item.UIDef is BigUIDef big && !string.IsNullOrEmpty(text))
                    big.descTwo = new BoxedString(text.Replace("<br>", " "));
            }
            else if (itemData.Item.name.Contains("Lore_Tablet-"))
            {
                string tabletName = RandomizerHelper.TranslateRandoName(itemData.Item.name.Substring("Lore_Tablet-".Length));
                // If the tablet name is empty, a "fake lore tablet" has been obtained.
                if (string.IsNullOrEmpty(tabletName))
                {
                    // We add a fake power
                    PowerManager.ActivePowers.Add(new PlaceholderPower());
                    return;
                }
                string placeHolder = string.Empty;
                LoreManager.Instance.ModifyText(tabletName, ref placeHolder);
                PowerManager.GetPowerByKey(tabletName, out Power power, false);
                if (itemData.Item.UIDef is MsgUIDef msg)
                    msg.name = new BoxedString(power.PowerName);
            }
            else if (itemData.Item.name.Equals("Journal_Entry-Seal_of_Binding") && itemData.Item.UIDef is MsgUIDef msg)
                msg.name = new BoxedString("Sacred Shell (PoP)");
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("An error occured while modifying a lore item drop: " + exception.Message);
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
            if (File.Exists(optionFile))
            {
                using StreamReader reader = new(optionFile);

                string headline = reader.ReadLine();
                if (headline.ToLower().Contains("%override%"))
                    PowerManager.ActivePowers.Clear();
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
                    LoreMaster.Instance.Log("Power name should be: " + powerName);
                    if (!PowerManager.GetPowerByName(powerName, out Power power, true, false) && !PowerManager.GetPowerByKey(powerName, out power, false))
                        continue;
                    // Skip the name
                    currentLine = currentLine.Substring(powerName.Length + 1);
                    string tagText = string.Concat(currentLine.TakeWhile(x => !x.Equals('|')));
                    tagText = char.ToUpper(tagText[0]) + tagText.Substring(1);
                    if (!Enum.TryParse(tagText, out PowerTag tag))
                        continue;
                    if (power.Tag != tag)
                        LoreMaster.Instance.Log($"Change {power.PowerName} tag from {power.Tag} to {tag}");
                    power.Tag = tag;

                    currentLine = currentLine.Substring(tagText.Length);
                    if (currentLine.Contains("add") && !PowerManager.ActivePowers.Contains(power))
                    {
                        LoreMaster.Instance.Log($"Add {power.PowerName} to player.");
                        PowerManager.ActivePowers.Add(power);
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

        if (CurrentArea != newArea)
            PowerManager.CalculatePowerStates(newArea);

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
            PowerManager.FirstPowerInitialization();
            PowerManager.UpdateTracker(newArea);
            TreasureHunterPower.UpdateTreasurePage();
        }
        LorePage.UpdateLorePage();

        // Execute all actions that powers want to do when the scene changes.
        PowerManager.ExecuteSceneActions();

        CurrentArea = newArea;
    }

    #endregion
}
