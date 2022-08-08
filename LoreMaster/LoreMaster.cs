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
using LoreMaster.LorePowers.Ancient_Basin;
using LoreMaster.LorePowers.CityOfTears;
using LoreMaster.LorePowers.Crossroads;
using LoreMaster.LorePowers.Deepnest;
using LoreMaster.LorePowers.Dirtmouth;
using LoreMaster.LorePowers.FogCanyon;
using LoreMaster.LorePowers.FungalWastes;
using LoreMaster.LorePowers.Greenpath;
using LoreMaster.LorePowers.HowlingCliffs;
using LoreMaster.LorePowers.KingdomsEdge;
using LoreMaster.LorePowers.Peaks;
using LoreMaster.LorePowers.QueensGarden;
using LoreMaster.LorePowers.RestingGrounds;
using LoreMaster.LorePowers.Waterways;
using LoreMaster.LorePowers.WhitePalace;
using LoreMaster.SaveManagement;
using LoreMaster.UnityComponents;
using Modding;
using SFCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LoreMaster;

public class LoreMaster : Mod, IGlobalSettings<LoreMasterGlobalSaveData>, ILocalSettings<LoreMasterLocalSaveData>, IMenuMod
{
    #region Members

    private Dictionary<string, Power> _powerList = new()
    {
        // Ancient Basin
        {"ABYSS_TUT_TAB_01", new WeDontTalkAboutShadePower() },
        // City of Tears
        {"RUIN_TAB_01", new HotStreakPower() },
        {"FOUNTAIN_PLAQUE_DESC", new TouristPower() {DefaultTag = PowerTag.Global } },
        {"RUINS_MARISSA_POSTER", new MarissasAudiencePower() },
        {"MAGE_COMP_03", new OverwhelmingPower() },
        {"MAGE_COMP_01", new SoulExtractEfficiencyPower() },
        {"LURIAN_JOURNAL", new EyeOfTheWatcherPower() },
        {"EMILITIA", new HappyFatePower() },
        {"POGGY", new DeliciousMealPower() },
        // Crossroads
        {"PILGRIM_TAB_01", new ReluctantPilgrimPower() },
        {"COMPLETION_RATE_UNLOCKED", new GreaterMindPower() { DefaultTag = PowerTag.Global } },
        {"MYLA", new DiamantDashPower() },
        // Crystal Peaks
        {"QUIRREL", new DiamondCorePower() },
        // Deepnest
        {"MASKMAKER", new MaskOverchargePower() },
        {"MIDWIFE", new InfestedPower() },
        // Dirtmouth/King's Pass
        {"TUT_TAB_01", new WellFocusedPower() },
        {"TUT_TAB_02", new ScrewTheRulesPower() },
        {"TUT_TAB_03", new TrueFormPower() },
        {"BRETTA", new CaringShellPower() },
        {"GRAVEDIGGER", new RequiemPower() },
        // Fog Canyon
        {"ARCHIVE_01", new FriendOfTheJellyfishPower() },
        {"ARCHIVE_02", new JellyBellyPower() },
        {"ARCHIVE_03", new JellyfishFlowPower() },
        // Fungal Wastes
        {"FUNG_TAB_04", new OneOfUsPower() },
        {"FUNG_TAB_01", new PaleLuckPower() },
        {"FUNG_TAB_02", new ImposterPower() },
        {"FUNG_TAB_03", new UnitedWeStandPower() },
        {"MANTIS_PLAQUE_01", new MantisStylePower() },
        {"MANTIS_PLAQUE_02", new EternalValorPower() },
        {"PILGRIM_TAB_02", new GloryOfTheWealthPower() },
        {"WILLOW", new BagOfMushroomsPower() },
        // Greenpath
        {"GREEN_TABLET_01", new TouchGrassPower() },
        {"GREEN_TABLET_02", new GiftOfUnnPower() },
        {"GREEN_TABLET_03", new MindblastOfUnnPower() },
        {"GREEN_TABLET_05", new CamouflagePower() },
        {"GREEN_TABLET_06", new ReturnToUnnPower() },
        {"GREEN_TABLET_07", new GraspOfLifePower() },
        // Howling Cliffs
        {"CLIFF_TAB_02", new LifebloodOmenPower() },
        {"JONI", new JonisProtectionPower() },
        // Kingdom's Edge
        {"MR_MUSH_RIDDLE_TAB_NORMAL", new WisdomOfTheSagePower() },
        {"BARDOON", new ConcussiveStrikePower() },
        {"HIVEQUEEN", new YouLikeJazzPower() },
        // Queen's Garden
        {"XUN_GRAVE_INSPECT", new FlowerRingPower() },
        {"QUEEN", new QueenThornsPower() },
        {"MOSSPROPHET", new FollowTheLightPower() },
        // Resting Grounds
        {"DREAMERS_INSPECT_RG5", new DreamBlessingPower() },
        // Waterways
        {"DUNG_DEF_SIGN", new EternalSentinelPower() },
        {"FLUKE_HERMIT", new RelentlessSwarmPower() },
        // White Palace
        {"WP_WORKSHOP_01", new ShadowForgedPower() },
        {"WP_THRONE_01", new ShiningBoundPower() },
        {"PLAQUE_WARN", new DiminishingCursePower() },
        {"EndOfPathOfPain", new SacredShellPower() { DefaultTag = PowerTag.Exclude } },
        // Unused
        {"ELDERBUG", new ElderbugHitListPower() { DefaultTag = PowerTag.Remove, Tag = PowerTag.Remove } }
    };

    private readonly Dictionary<Area, List<MapZone>> _mapZones = new()
    {
        {Area.Dirtmouth, new(){MapZone.KINGS_PASS, MapZone.TOWN} },
        {Area.Crossroads, new(){MapZone.CROSSROADS, MapZone.TRAM_UPPER, MapZone.FINAL_BOSS} },
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

    private Area _currentArea;

    #endregion

    #region Constructors

    public LoreMaster()
    {
        LorePage.PassPowers(_powerList.Values.ToList());
        InventoryHelper.AddInventoryPage(InventoryPageType.Empty, "Lore", "Loremaster", "Loremaster", "metElderbug", LorePage.GeneratePage);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the running instance of the mod.
    /// </summary>
    public static LoreMaster Instance { get; set; }

    /// <summary>
    /// Gets or sets all active power, with their tablet key.
    /// </summary>
    public Dictionary<string, Power> ActivePowers { get; set; } = new();

    /// <summary>
    /// Gets or sets the preloaded object, used by various different powers.
    /// </summary>
    public Dictionary<string, GameObject> PreloadedObjects { get; set; } = new Dictionary<string, GameObject>();

    /// <summary>
    /// Gets or sets the generator used for random rolls.
    /// </summary>
    public System.Random Generator { get; set; } = new System.Random();

    /// <summary>
    /// Gets or sets the handler that runs every coroutine.
    /// </summary>
    public CoroutineHandler Handler { get; set; }

    /// <summary>
    /// Gets or sets the flag that indicates if hints should be shown instead of the description.
    /// </summary>
    public bool UseHints { get; set; } = true;

    /// <summary>
    /// Gets or sets the flag, that indicates if custom text can be used.
    /// </summary>
    public bool UseCustomText { get; set; } = true;

    /// <summary>
    /// Gets or sets the flag, that indicates if the yellow mushroom effect should not use the nausea effect.
    /// </summary>
    public bool DisableYellowMushroom { get; set; }

    /// <summary>
    /// Gets the flag for the toggle button to disable this mod.
    /// </summary>
    public bool ToggleButtonInsideMenu => true;

    /// <summary>
    /// Gets all powers
    /// </summary>
    internal Dictionary<string, Power> AllPowers => _powerList;

    /// <summary>
    /// Gets the current map area.
    /// </summary>
    public Area CurrentArea => _currentArea;

    #endregion

    #region Event Handler

    /// <summary>
    /// This is the main control, which determines which power is on.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="sheetTitle"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    private string GetText(string key, string sheetTitle, string text)
    {
        if (key.Equals("Loremaster"))
            return "Lore Powers";
        key = ModifyKey(key);
        if (key.Equals("INV_NAME_SUPERDASH"))
        {
            bool hasDiamondDash = _powerList["MYLA"].Active;
            bool hasDiamondCore = _powerList["QUIRREL"].Active;

            if (hasDiamondDash && !hasDiamondCore)
                text = "Diamond Heart (Shell)";
            else if (!hasDiamondDash && hasDiamondCore)
                text = "Diamond Heart (Core)";
            else if (hasDiamondCore && hasDiamondDash)
                text = "Diamond Heart";
        }
        else if (key.Equals("INV_DESC_SUPERDASH"))
        {
            bool hasDiamondDash = _powerList["MYLA"].Active;
            bool hasDiamondCore = _powerList["QUIRREL"].Active;

            if (hasDiamondDash && !hasDiamondCore)
                text += "<br>The vessel of the core is forged with a powerful diamond plate. You promised that you give it back to Myla... right?";
            else if (!hasDiamondDash && hasDiamondCore)
                text += "<br>The core is pulsing in the rate of your heart. Just having the pure diamond core near you, let you feel the pressure. It just needs the right vessel to hold it.";
            else if (hasDiamondCore && hasDiamondDash)
                text += "<br>Bathed and reforged with powerful diamonds. Formerly in possession of the crystal leader, this artefact can cause massive earth quakes.";
        }
        else if (key.Equals("FOUNTAIN_PLAQUE_DESC"))
            text += " [" + _powerList[key].PowerName + "] " + (UseHints ? _powerList[key].Hint : _powerList[key].Description);
        else if (key.Contains("DREAMERS_INSPECT_RG"))
        {
            DreamBlessingPower dreamPower = (DreamBlessingPower)_powerList["DREAMERS_INSPECT_RG5"];
            text += dreamPower.GetExtraText(key);
        }
        else if (!CheckForPower(key, ref text) && _powerList["QUEEN"].Active)
        {
            if (key.Equals("CHARM_NAME_12"))
                return "Queen's Thorns";
            else if (key.Equals("CHARM_DESC_12"))
                return text + "<br>Blessed by the white lady, which causes them to drain soul and sometimes energy from their victims. Leash out more agile.";
        }
        else if (key.Equals("ELDERBUG_INTRO_MAIN"))
            text = "This town may not hold the most interesting wisdom, but the kingdom below sure does. There a plenty of tablets and creatures which you can learn from. " +
                "Maybe someday, I'll be able to call you \"Loremaster\". Oh what a thought, excuse me. Anyway, if you want to explore the world below, keeping track of " +
                "every knowledge that you acquired might be hard. Let me help you with that. This is a relic which tracks every bit of information that you've collected so far. " +
                "Sometimes, the knowledge can be more of a threat than a blessing. In those cases, touching the ability on the relic may disable them, until you touch it again. " +
                "Maybe you should not waste too much time though. I heard legends that this artifact might lock it's " +
                "power behind a test or something once the one in the time event \"Patch 1.2\" happens... whatever that might be. I hope, you've listened to me, I'll not repeat myself " +
                "again with this information. See this, as a test to see if you can step up to the challenge. Don't forget \"Knowledge is power\".";
        return text;
    }

    /// <summary>
    /// Event handler, when a new game is started.
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    /// <param name="permaDeath"></param>
    /// <param name="bossRush"></param>
    private void StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
    {
        try
        {
            ItemChangerMod.CreateSettingsProfile(false);
            List<MutablePlacement> teleportItems = new();
            MutablePlacement teleportPlacement = new CoordinateLocation() { x = 35.0f, y = 5.4f, elevation = 0, sceneName = "Ruins1_27", name = "City_Teleporter" }.Wrap() as MutablePlacement;
            teleportPlacement.Cost = new Paypal { ToTemple = true };
            teleportPlacement.Add(new TouristMagnetItem("City_Teleporter"));
            teleportItems.Add(teleportPlacement);

            MutablePlacement secondPlacement = new CoordinateLocation() { x = 57f, y = 5f, elevation = 0, sceneName = "Room_temple", name = "Temple_Teleporter" }.Wrap() as MutablePlacement;
            secondPlacement.Cost = new Paypal { ToTemple = false };
            secondPlacement.Add(new TouristMagnetItem("Temple_Teleporter"));
            teleportItems.Add(secondPlacement);
            ItemChangerMod.AddPlacements(teleportItems);
            GloryOfTheWealthPower.GloryCost = 0;
            orig(self, permaDeath, bossRush);
            ModHooks.SetPlayerBoolHook += TrackPathOfPain;
            _fromMenu = true;

            // Reset tags to default.
            foreach (string key in _powerList.Keys)
                _powerList[key].Tag = _powerList[key].DefaultTag;
            // Unsure if this is needed, but just in case.
            ActivePowers.Clear();
        }
        catch (Exception exception)
        {
            LogError(exception.Message);
        }
    }

    /// <summary>
    /// Event handler, when a game is continued (from the save file).
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    private void ContinueGame(On.UIManager.orig_ContinueGame orig, UIManager self)
    {
        _fromMenu = true;
        orig(self);
    }

    /// <summary>
    /// Event handler used for returning to the menu.
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    /// <param name="saveMode"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    private IEnumerator ReturnToMenu(On.GameManager.orig_ReturnToMainMenu orig, GameManager self, GameManager.ReturnToMainMenuSaveModes saveMode, Action<bool> callback)
    {
        foreach (Power power in ActivePowers.Values)
            power.DisablePower(true);
        Handler.StopAllCoroutines();
        _currentArea = Area.None;
        On.PlayMakerFSM.OnEnable -= FsmEdits;
        On.DeactivateIfPlayerdataTrue.OnEnable -= ForceMyla;
        On.DeactivateIfPlayerdataFalse.OnEnable -= PreventMylaZombie;
        ModHooks.SetPlayerBoolHook -= TrackPathOfPain;
        ModHooks.LanguageGetHook -= GetText;
        if (ModHooks.GetMod("Randomizer 4", true) is Mod mod)
            AbstractItem.BeforeGiveGlobal -= GiveLoreItem;
        return orig(self, saveMode, callback);
    }

    /// <summary>
    /// Event handler, for the PoP power.
    /// </summary>
    private bool TrackPathOfPain(string name, bool orig)
    {
        if (name.Equals("killedBindingSeal") && orig)
        {
            if (!ActivePowers.ContainsKey("EndOfPathOfPain"))
            {
                Power power = _powerList["EndOfPathOfPain"];
                power.EnablePower();
                ActivePowers.Add("EndOfPathOfPain", power);
                UpdateTracker(_currentArea);
                LorePage.UpdateLorePage();
            }
            // This hook is no longer needed after obtaining the power.
            ModHooks.SetPlayerBoolHook -= TrackPathOfPain;
        }
        return orig;
    }

    /// <summary>
    /// Event handler used for adjusting the active powers.
    /// </summary>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    private void SceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        try
        {
            if (GameManager.instance.IsGameplayScene() && !arg1.name.Equals("Quit_To_Menu"))
            {
                if (_fromMenu)
                {
                    if (PlayerData.instance.GetBool("killedBindingSeal") && !_powerList.ContainsKey("EndOfPathOfPain"))
                    {
                        Power power = _powerList["EndOfPathOfPain"];
                        power.EnablePower();
                        ActivePowers.Add("EndOfPathOfPain", power);
                    }
                    else
                        ModHooks.SetPlayerBoolHook += TrackPathOfPain;

                    if (arg1.name.ToLower().Equals("town"))
                    {
                        Transform elderbug = GameObject.Find("_NPCs/Elderbug").transform;
                        elderbug.localScale = new(2f, 2f, 2f);
                        elderbug.localPosition = new(126.36f, 12.35f, 0f);
                    }
                    ModHooks.LanguageGetHook += GetText;
                    On.PlayMakerFSM.OnEnable += FsmEdits;
                    On.DeactivateIfPlayerdataTrue.OnEnable += ForceMyla;
                    On.DeactivateIfPlayerdataFalse.OnEnable += PreventMylaZombie;

                    // Allow the player to read the resting grounds tablet.
                    if (ItemChangerMod.Modules.Modules.FirstOrDefault(x => x.Name.Equals("DreamNailCutsceneEvent")) is not DreamNailCutsceneEvent cutscene)
                        ItemChangerMod.Modules.Modules.Add(new DreamNailCutsceneEvent() { Faster = false });
                    else
                        cutscene.Faster = false;

                    // Load in changes from the options file (if it exists)
                    LoadOptions();

                    if (ModHooks.GetMod("Randomizer 4", true) is Mod mod)
                    {
                        Log("Detected Randomizer. Adding compability.");
                        AbstractItem.BeforeGiveGlobal += GiveLoreItem;
                    }
                }

                Handler.StartCoroutine(ManageSceneActions());
            }
        }
        catch (Exception error)
        {
            LogError("Error while trying to load new scene: " + error.Message);
            LogError(error.StackTrace);
        }
    }

    /// <summary>
    /// Event handler that handles the fsm edits.
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    private void FsmEdits(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        // The quirrel in peaks has 3 fsm (don't ask) that indicates if he can be there: If he has been encountered in archives, if you have superdash or if he has just left the mines.
        // We remove all those checks.
        if (self.gameObject.name.Equals("Quirrel Mines") && self.FsmName.Equals("FSM"))
            self.GetState("Check").RemoveTransitionsTo("Destroy");
        // The game asks for the language key for the fountain once you entered the room. To not give the power immediatly, we bind it on the inspect instead.
        else if (self.gameObject.name.Equals("Fountain Inspect") && self.FsmName.Equals("Conversation Control"))
        {
            string placeHolder = string.Empty;
            self.GetState("Anim End").ReplaceAction(new Lambda(() => CheckForPower("FOUNTAIN_PLAQUE_DESC", ref placeHolder)) { Name = "Fountain Power" });
        }
        // The game asks for the language key for the dreamer tablet once you entered the room. To not give the power immediatly, we bind it on the inspect instead.
        else if (self.gameObject.name.Equals("Dreamer Plaque Inspect") && self.FsmName.Equals("Conversation Control"))
        {
            string placeHolder = string.Empty;
            self.GetState("Anim End").ReplaceAction(new Lambda(() => CheckForPower("DREAMERS_INSPECT_RG5", ref placeHolder)) { Name = "Dreamer Power" });
        }
        // Prevent Moss Prophet from dying
        else if (self.gameObject.name.Equals("Moss Cultist") && self.FsmName.Equals("FSM"))
        {
            self.GetState("Check").RemoveTransitionsTo("Destroy");
            self.gameObject.GetComponent<BoxCollider2D>().enabled = true;
        }
        // Deactives moss prophet corpse, so that it doesn't block the living one.
        else if (self.gameObject.name.Equals("corpse set") && self.FsmName.Equals("FSM"))
            self.gameObject.FindChild("corpse0000").SetActive(false);
        else if (self.gameObject.name.Contains("Radiance") && self.FsmName.Equals("Control"))
        {
            // This disables all powers when the player defeats radiance.
            // The main purpose is to ensure, that the player is not killed by revek (Marissas Audience) or grimmkin (Lifeblood Omen).
            try
            {
                self.GetState("Final Impact").ReplaceAction(new Lambda(() =>
                {
                    PlayerData.instance.SetInt(nameof(PlayerData.instance.killsFinalBoss), 0);
                    foreach (Power power in ActivePowers.Values)
                        power.DisablePower();

                })
                { Name = "Deactivate Lore powers." }, 5);
            }
            catch (Exception exception)
            {
                LogError("Couldn't modify radiance fsm: " + exception.Message);
            }
        }
        else if (self.gameObject.name.Equals("Elderbug") && self.FsmName.Equals("npc_control"))
        {
            self.transform.localScale = new(2f, 2f, 2f);
            self.transform.localPosition = new(126.36f, 12.35f, 0f);
        }
        else if (self.FsmName.Equals("ghost_npc_death"))
        {
            string ghostName = "";
            try
            {
                ghostName = self.gameObject.LocateMyFSM("Conversation Control").FsmVariables.FindFsmString("Ghost Name").Value.ToUpper();
                if (ghostName.Equals("POGGY") || ghostName.Equals("HIVEQUEEN") || ghostName.Equals("JONI") || ghostName.Equals("GRAVEDIGGER"))
                {
                    self.GetState("Revek?").ReplaceAction(new Lambda(() =>
                    {
                        if (!ActivePowers.ContainsKey(ghostName))
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
                LogError("Error while modifying ghost: " + ghostName + ": " + exception.Message);
            }
        }
        else if (self.FsmName.Equals("Phase Control") && self.gameObject.name.Equals("Hollow Knight Boss"))
            self.GetState("Die").ReplaceAction(new Lambda(() =>
            {
                foreach (Power power in ActivePowers.Values)
                    power.DisablePower();
                PlayerData.instance.SetBool(nameof(PlayerData.instance.killedHollowKnight), true);
            })
            { Name = "Deactivate Lore Powers" }, 2);
        orig(self);
    }

    /// <summary>
    /// Despawns mylas other versions.
    /// </summary>
    private void PreventMylaZombie(On.DeactivateIfPlayerdataFalse.orig_OnEnable orig, DeactivateIfPlayerdataFalse self)
    {
        if (self.gameObject.name.Contains("Zombie Myla") || self.gameObject.name.Equals("Myla Crazy NPC"))
        {
            self.gameObject.SetActive(false);
            return;
        }
        orig(self);
    }

    /// <summary>
    /// Forces Myla (best character btw.) to always appear, like she should.
    /// </summary>
    private void ForceMyla(On.DeactivateIfPlayerdataTrue.orig_OnEnable orig, DeactivateIfPlayerdataTrue self)
    {
        if (self.gameObject.name.Equals("Miner") && (self.boolName.Equals("hasSuperDash") || self.boolName.Equals("mageLordDefeated")))
            return;
        orig(self);
    }

    /// <summary>
    /// Event handler to adjust the message and give the power of randomizer items.
    /// </summary>
    /// <param name="itemData"></param>
    private void GiveLoreItem(ReadOnlyGiveEventArgs itemData)
    {
        //If focus is randomized but the lore tablet isn't, the lore tablet becomes unavailable, which is why we add the power to focus instead.
        if (itemData.Item.name.Equals("Focus") && ItemChanger.Internal.Ref.Settings.Placements.ContainsKey(LocationNames.Focus))
        {
            string text = string.Empty;
            CheckForPower("TUT_TAB_01", ref text);
            if (itemData.Item.UIDef is BigUIDef big)
                big.descTwo = new BoxedString(text.Replace("<br>", " "));
        }
        // If world sense is randomized but the lore tablet isn't, the lore tablet becomes unavailable, which is why we add the power to world sense instead.
        else if (itemData.Item.name.Equals("World_Sense") && ItemChanger.Internal.Ref.Settings.Placements.ContainsKey(LocationNames.World_Sense))
        {
            string text = string.Empty;
            CheckForPower("COMPLETION_RATE_UNLOCKED", ref text);
            if (itemData.Item.UIDef is BigUIDef big)
                big.descTwo = new BoxedString(text.Replace("<br>", " "));
        }
        else if (itemData.Item.name.Contains("Lore_Tablet-"))
        {
            string tabletName = RandomizerHelper.TranslateRandoName(itemData.Item.name.Substring("Lore_Tablet-".Length));
            string placeHolder = string.Empty;
            CheckForPower(tabletName, ref placeHolder);
            if (itemData.Item.UIDef is MsgUIDef msg)
                msg.name = new BoxedString(_powerList[tabletName].PowerName);
        }
    }

    #endregion

    #region Configuration

    /// <summary>
    /// Get the version of the mod.
    /// </summary>
    /// <returns></returns>
    public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

    /// <summary>
    /// Gets the names (objects) that need to be preloaded.
    /// </summary>
    /// <returns></returns>
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("RestingGrounds_08", "Ghost Battle Revek"),
        ("Deepnest_43", "Mantis Heavy Flyer"), // Deepnest_43 Mantis Heavy Flyer -> PersonalObjectPool -> StartUpPool [0] is shot
        ("Ruins1_28","Flamebearer Spawn"), // Small Ghost
        ("RestingGrounds_06","Flamebearer Spawn"), // Medium Ghost
        ("Hive_03","Flamebearer Spawn"), // Large Ghost
        ("GG_Hollow_Knight", "Battle Scene/HK Prime/Focus Blast/focus_ring"),
        ("GG_Hollow_Knight", "Battle Scene/HK Prime/Focus Blast/focus_rune"),
        ("Fungus1_01b","green_grass_1"),
        ("White_Palace_09","ash_grass_02")
    };

    /// <summary>
    /// Does the initialization needed for the mod.
    /// </summary>
    /// <param name="preloadedObjects"></param>
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        if (Instance != null)
            return;
        Instance = this;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
        On.UIManager.StartNewGame += StartNewGame;
        On.UIManager.ContinueGame += ContinueGame;
        On.GameManager.ReturnToMainMenu += ReturnToMenu;

        int grimmkinIndex = 1;
        foreach (string key in preloadedObjects.Keys)
            foreach (string subKey in preloadedObjects[key].Keys)
                if (!PreloadedObjects.ContainsKey(subKey) || subKey.Equals("Flamebearer Spawn"))
                {
                    GameObject toAdd = preloadedObjects[key][subKey];
                    if (subKey.Equals("Mantis Heavy Flyer"))
                        toAdd = preloadedObjects[key][subKey].GetComponent<PersonalObjectPool>().startupPool[0].prefab;
                    else if (subKey.Equals("Flamebearer Spawn"))
                    {
                        string realKey = grimmkinIndex == 1 ? "Small Ghost" : (grimmkinIndex == 2 ? "Medium Ghost" : "Large Ghost");
                        toAdd = toAdd.LocateMyFSM("Spawn Control").FsmVariables.FindFsmGameObject("Grimmkin Obj").Value;
                        PreloadedObjects.Add(realKey, toAdd);
                        GameObject.DontDestroyOnLoad(toAdd);
                        grimmkinIndex++;
                        continue;
                    }
                    PreloadedObjects.Add(subKey, toAdd);
                    GameObject.DontDestroyOnLoad(toAdd);
                }
        GameObject loreManager = new("LoreManager");
        GameObject.DontDestroyOnLoad(loreManager);
        Handler = loreManager.AddComponent<CoroutineHandler>();
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
                    ActivePowers.Clear();
                else if (!headline.ToLower().Contains("%modify%"))
                {
                    LogError("Invalid option file. Use %override% or %modify% in the first line.");
                    return;
                };
                Log("Apply option file");
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
                    Power power = _powerList.Values.FirstOrDefault(x => x.GetType().Name.ToLower().Equals(powerName + "power") || x.PowerName.Replace(" ", "").ToLower().Equals(powerName));
                    if (power == null)
                        continue;
                    // Skip the name
                    currentLine = currentLine.Substring(powerName.Length + 1);
                    string tagText = string.Concat(currentLine.TakeWhile(x => !x.Equals('|')));
                    tagText = char.ToUpper(tagText[0]) + tagText.Substring(1);
                    if (!Enum.TryParse(tagText, out PowerTag tag))
                        continue;
                    power.Tag = tag;

                    currentLine = currentLine.Substring(tagText.Length);
                    if (currentLine.Contains("add") && !ActivePowers.ContainsValue(power))
                        ActivePowers.Add(_powerList.First(x => x.Value == power).Key, power);
                }
            }
            else
                LogDebug("Couldn't find option file");
        }
        catch (Exception exception)
        {
            LogError("Couldn't load option file: " + exception.Message);
        }
    }

    /// <summary>
    /// Handles the mod menu.
    /// </summary>
    public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
    {
        List<IMenuMod.MenuEntry> menu = new();
        menu.Add(new()
        {
            Name = "Custom Text",
            Description = "Replaces the text of tablets or conversations (if available).",
            Values = new string[] {
                    "On",
                    "Off",
                },
            // opt will be the index of the option that has been chosen
            Saver = option => UseCustomText = option == 0,
            Loader = () => UseCustomText ? 0 : 1
        });

        menu.Add(new()
        {
            Name = "Use Vague Hints",
            Description = "If on, it shows the normal more vaguely text. Otherwise it shows a clear description what the power does.",
            Values = new string[] { "On", "Off" },
            Saver = option => UseHints = option == 0,
            Loader = () => UseHints ? 0 : 1
        });

        menu.Add(new()
        {
            Name = "Disable Yellow Mushroom",
            Description = "If on, the yellow mushroom will not cause a nausea effect.",
            Values = new string[] { "On", "Off" },
            Saver = option => DisableYellowMushroom = option == 0,
            Loader = () => DisableYellowMushroom ? 0 : 1
        });

        return menu;
    }

    /// <summary>
    /// Unloads the mod. (Currently unused)
    /// </summary>
    public void Unload()
    {
        Instance = null;
        ModHooks.LanguageGetHook -= GetText;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneChanged;
        On.UIManager.StartNewGame -= StartNewGame;
        On.GameManager.ReturnToMainMenu -= ReturnToMenu;
        Handler.StopAllCoroutines();
        foreach (Power power in ActivePowers.Values)
            power.DisablePower();
    }

    #endregion

    #region Internal Methods

    /// <summary>
    /// Adds a power to the dictionary.
    /// </summary>
    /// <param name="key">The language key which activates the power</param>
    /// <param name="power">The power which should be added.</param>
    /// <returns></returns>
    internal bool AddPower(string key, Power power)
    {
        if (power == null)
            return false;
        try
        {
            _powerList.Add(key, power);
            return true;
        }
        catch (Exception exception)
        {
            LogError("Failed adding " + power.PowerName + ". Error: " + exception.Message);
        }
        return false;
    }

    /// <summary>
    /// Checks through all needed powers to determine if the powers should be granted globally.
    /// </summary>
    /// <param name="toCheck"></param>
    /// <returns></returns>
    internal bool IsAreaGlobal(Area toCheck)
    {
        List<Power> neededAreaPowers = _powerList.Values.Where(x => x.Location == toCheck && (x.Tag == PowerTag.Local || x.Tag == PowerTag.Disable || x.Tag == PowerTag.Global)).ToList();
        foreach (Power neededPower in neededAreaPowers)
            if (!ActivePowers.ContainsValue(neededPower))
                return false;
        return true;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Main logic to check if key contains a power, if so, add it.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    private bool CheckForPower(string key, ref string text)
    {
        if (_powerList.ContainsKey(key))
        {
            try
            {
                Power power;
                if (!ActivePowers.ContainsKey(key))
                {
                    power = _powerList[key];
                    power.EnablePower();
                    ActivePowers.Add(key, power);
                    UpdateTracker(_currentArea);
                    LorePage.UpdateLorePage();
                }
                else
                    power = ActivePowers[key];
                if (UseCustomText && !string.IsNullOrEmpty(power.CustomText))
                    text = power.CustomText;
                text += "<br>[" + power.PowerName + "]";
                text += "<br>" + (UseHints ? power.Hint : power.Description);
                if (key.Equals("PLAQUE_WARN"))
                {
                    Power popPower = _powerList["EndOfPathOfPain"];
                    text += "<page>For those, that reveals the secret, awaits the power:";
                    text += "<br>[" + popPower.PowerName + "] ";
                    text += "<br>" + (UseHints ? popPower.Hint : popPower.Description);
                }
                return true;
            }
            catch (Exception exception)
            {
                LogError(exception.Message);
            }
        }
        return false;
    }

    /// <summary>
    /// Modifies the language key, to keep consistancy between the lore keys (mostly for NPC).
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string ModifyKey(string key)
    {
        if (key.Contains("BRETTA_DIARY"))
            key = "BRETTA";
        else if (IsMidwife(key))
            key = "MIDWIFE";
        else if (IsBardoon(key))
            key = "BARDOON";
        else if (key.Equals("HIVEQUEEN_TALK") || key.Equals("HIVEQUEEN_REPEAT"))
            key = "HIVEQUEEN";
        else if (key.Equals("JONI_TALK") || key.Equals("JONI_REPEAT"))
            key = "JONI";
        else if (key.Equals("POGGY_TALK") || key.Equals("POGGY_REPEAT"))
            key = "POGGY";
        else if (key.Equals("GRAVEDIGGER_TALK") || key.Equals("GRAVEDIGGER_REPEAT"))
            key = "GRAVEDIGGER";
        else if (IsFlukeHermit(key))
            key = "FLUKE_HERMIT";
        else if (IsQueen(key))
            key = "QUEEN";
        else if (IsMaskMaker(key))
            key = "MASKMAKER";
        else if (IsWillow(key))
            key = "WILLOW";
        else if (IsMyla(key))
            key = "MYLA";
        else if (IsQuirrel(key))
            key = "QUIRREL";
        else if (IsEmilitia(key))
            key = "EMILITIA";
        else if (IsMossProphet(key))
            key = "MOSSPROPHET";
        return key;
    }

    /// <summary>
    /// Manages the activation/deactivation of powers and scene change trigger.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ManageSceneActions()
    {
        yield return new WaitForFinishedEnteringScene();
        // Just to make sure the controller exist. A desperate attempt.
        if (HeroController.instance == null || !HeroController.instance.acceptingInput)
            yield return new WaitUntil(() => HeroController.instance != null && HeroController.instance.acceptingInput);

        Area newArea = _currentArea;

        // Figure out the current Map zone. (Dream world counts as the same area)
        if (!Enum.TryParse(GameManager.instance.GetCurrentMapZone(), out MapZone newMapZone))
            LogError("Couldn't convert map zone to enum. Value: " + GameManager.instance.GetCurrentMapZone());
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
                LogError("Couldn't find area: " + newMapZone);
        }

        // If the area changes, we adjust the powers.
        if (_currentArea != newArea)
        {
            try
            {
                // Activate all local abilities
                foreach (Power power in ActivePowers.Values.Where(x => x.Location == newArea))
                    if (power.Tag == PowerTag.Exclude || power.Tag == PowerTag.Local)
                        power.EnablePower();

                // Disable all local abilities from all other zone (this has to be done that way for randomizer compability)
                foreach (Area area in ((Area[])Enum.GetValues(typeof(Area))).Skip(1))
                    if (area != newArea && !IsAreaGlobal(area))
                        foreach (Power power in ActivePowers.Values.Where(x => x.Location == area))
                            if (power.Tag == PowerTag.Local || power.Tag == PowerTag.Exclude)
                                power.DisablePower();
            }
            catch (Exception exception)
            {
                LogError("An error occured in the area change: " + exception.Message);
            }
            UpdateTracker(newArea);
        }

        // Initialization taken when we entered from the menu.
        if (_fromMenu)
        {
            _fromMenu = false;
            // Enables the powers beforehand. This has to be done because otherwise the effects will only stay permanent once the player enters the area.
            List<Power> toActivate = new();
            List<Power> allPowers = ActivePowers.Select(x => x.Value).ToList();
            toActivate.AddRange(allPowers.Where(x => x.Tag == PowerTag.Global));

            foreach (Area area in (Area[])Enum.GetValues(typeof(Area)))
                if (IsAreaGlobal(area))
                    toActivate.AddRange(allPowers.Where(x => x.Tag != PowerTag.Global && x.Location == area));

            foreach (Power power in toActivate)
                power.EnablePower();
            UpdateTracker(newArea);
        }
        LorePage.UpdateLorePage();

        // Execute all actions that powers want to do when the scene changes.
        foreach (Power powers in ActivePowers.Values)
            if (powers.Active && powers.SceneAction != null)
                try
                {
                    powers.SceneAction.Invoke();
                }
                catch (Exception exception)
                {
                    LogError("Error while executing scene action for " + powers.PowerName + ": " + exception.Message + "StackTrace: " + exception.StackTrace);
                }
        _currentArea = newArea;
    }

    /// <summary>
    /// Updates the lore tracker.
    /// </summary>
    private void UpdateTracker(Area areaToUpdate)
    {
        try
        {
            if (ActivePowers.ContainsKey("COMPLETION_RATE_UNLOCKED"))
            {
                GreaterMindPower logPower = (GreaterMindPower)ActivePowers["COMPLETION_RATE_UNLOCKED"];
                if (logPower.Active)
                    logPower.UpdateLoreCounter(ActivePowers.Values, _powerList.Values, areaToUpdate, IsAreaGlobal(areaToUpdate));
            }
        }
        catch (Exception exception)
        {
            LogError(exception.Message);
        }
    }

    #region NPC Dialogues

    private bool IsBardoon(string key)
    {
        return key.Equals("BIGCAT_INTRO") || key.Equals("BIGCAT_TALK_01")
            || key.Equals("BIGCAT_TALK_02") || key.Equals("BIGCAT_TALK_03")
            || key.Equals("BIGCAT_TAIL_HIT") || key.Equals("BIGCAT_KING_BRAND")
            || key.Equals("BIGCAT_SHADECHARM") || key.Equals("BIGCAT_REPEAT");
    }

    private bool IsMidwife(string key)
    {
        return key.Equals("SPIDER_MEET") || key.Equals("SPIDER_GREET")
            || key.Equals("SPIDER_GREET2") || key.Equals("SPIDER_REPEAT") || key.Equals("MIDWIFE_WEAVERSONG");
    }

    private bool IsMaskMaker(string key)
    {
        return key.Equals("MASK_MAKER_GREET") || key.Equals("MASK_MAKER_REPEAT")
            || key.Equals("MASK_MAKER_REPEAT2") || key.Equals("MASK_MAKER_REPEAT3")
            || key.Equals("MASK_MAKER_UNMASK") || key.Equals("MASK_MAKER_UNMASK3")
            || key.Equals("MASK_MAKER_UNMASK4") || key.Equals("MASK_MAKER_UNMASK2") || key.Equals("MASK_MAKER_UNMASK_REPEAT")
            || key.Equals("MASKMAKER_GREET") || key.Equals("MASKMAKER_REPEAT")
            || key.Equals("MASKMAKER_REPEAT2") || key.Equals("MASKMAKER_REPEAT3")
            || key.Equals("MASKMAKER_UNMASK") || key.Equals("MASKMAKER_UNMASK3")
            || key.Equals("MASKMAKER_UNMASK4") || key.Equals("MASKMAKER_UNMASK2") || key.Equals("MASKMAKER_UNMASK_REPEAT");
    }

    private bool IsFlukeHermit(string key)
    {
        return key.Equals("FLUKE_HERMIT_PRAY") || key.Equals("FLUKE_HERMIT_PRAY_REPEAT")
            || key.Equals("FLUKE_HERMIT_IDLE_1") || key.Equals("FLUKE_HERMIT_IDLE_2")
            || key.Equals("FLUKE_HERMIT_IDLE_3") || key.Equals("FLUKE_HERMIT_IDLE_4")
            || key.Equals("FLUKE_HERMIT_IDLE_5") || key.Equals("MASK_MAKER_UNMASK2") || key.Equals("MASK_MAKER_UNMASK_REPEAT");
    }

    private bool IsQueen(string key)
    {
        return key.Equals("QUEEN_MEET") || key.Equals("QUEEN_MEET_REPEAT")
            || key.Equals("QUEEN_TALK_01") || key.Equals("QUEEN_TALK_02")
            || key.Equals("QUEEN_HORNET") || key.Equals("QUEEN_DUNG")
            || key.Equals("QUEEN_DUNG_02") || key.Equals("QUEEN_REPEAT_KINGSOUL")
            || key.Equals("QUEEN_TALK_EXTRA") || key.Equals("QUEEN_REPEAT_SHADECHARM")
            || key.Equals("QUEEN_GRIMMCHILD") || key.Equals(" QUEEN_GRIMMCHILD_FULL");
    }

    private bool IsWillow(string key)
    {
        return key.Equals("GIRAFFE_MEET") || key.Equals("GIRAFFE_LOWER") || key.Equals("GIRAFFE_LOWER_REPEAT");
    }

    private bool IsMyla(string key)
    {
        return key.Equals("MINER_MEET_1_B") || key.Equals("MINER_MEET_REPEAT") || key.Equals("MINER_EARLY_1_B") || key.Equals("MINER_EARLY_2_B") || key.Equals("MINER_EARLY_3");
    }

    private bool IsQuirrel(string key)
    {
        return key.Equals("QUIRREL_MINES_1") || key.Equals("QUIRREL_MINES_2") || key.Equals("QUIRREL_MINES_3") || key.Equals("QUIRREL_MINES_4");
    }

    private bool IsEmilitia(string key)
    {
        return key.Equals("EMILITIA_MEET") || key.Equals("EMILITIA_KING_BRAND") || key.Equals("EMILITIA_GREET") || key.Equals("EMILITIA_REPEAT");
    }

    private bool IsMossProphet(string key)
    {
        return key.Equals("MOSS_CULTIST_01") || key.Equals("MOSS_CULTIST_02") || key.Equals("MOSS_CULTIST_03");
    }

    #endregion

    #region Save Management

    /// <summary>
    /// Loads the data for the global mod settings.
    /// </summary>
    /// <param name="globalSaveData"></param>
    public void OnLoadGlobal(LoreMasterGlobalSaveData globalSaveData)
    {
        Log("Loaded global data");
        UseHints = globalSaveData.ShowHint;
        UseCustomText = globalSaveData.EnableCustomText;
        DisableYellowMushroom = globalSaveData.DisableNausea;
    }

    /// <summary>
    /// Saves the data for the global mod settings.
    /// </summary>
    /// <returns></returns>
    LoreMasterGlobalSaveData IGlobalSettings<LoreMasterGlobalSaveData>.OnSaveGlobal()
        => new() { ShowHint = UseHints, EnableCustomText = UseCustomText, DisableNausea = DisableYellowMushroom };

    /// <summary>
    /// Loads the data from the save file.
    /// </summary>
    public void OnLoadLocal(LoreMasterLocalSaveData saveData)
    {
        ActivePowers.Clear();

        try
        {
            foreach (string key in saveData.Tags.Keys)
                _powerList[key].Tag = saveData.Tags[key];

            foreach (string key in saveData.AcquiredPowersKey)
                ActivePowers.Add(key, _powerList[key]);

            GloryOfTheWealthPower.GloryCost = saveData.GloryCost;
        }
        catch (Exception exception)
        {
            LogError("Error while loading file powers: " + exception.Message);
        }
    }

    /// <summary>
    /// Saves the data from the save file.
    /// </summary>
    LoreMasterLocalSaveData ILocalSettings<LoreMasterLocalSaveData>.OnSaveLocal()
    {
        LoreMasterLocalSaveData saveData = new();

        foreach (string key in _powerList.Keys)
            saveData.Tags.Add(key, _powerList[key].Tag);

        foreach (string key in ActivePowers.Keys)
            saveData.AcquiredPowersKey.Add(key);

        saveData.GloryCost = GloryOfTheWealthPower.GloryCost;

        return saveData;
    }

    #endregion

    #endregion
}
