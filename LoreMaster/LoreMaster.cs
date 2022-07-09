using GlobalEnums;
using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Placements;
using LoreMaster.CustomItem;
using LoreMaster.Enums;
using LoreMaster.LorePowers;
using LoreMaster.LorePowers.Ancient_Basin;
using LoreMaster.LorePowers.CityOfTears;
using LoreMaster.LorePowers.Crossroads;
using LoreMaster.LorePowers.Deepnest;
using LoreMaster.LorePowers.FogCanyon;
using LoreMaster.LorePowers.FungalWastes;
using LoreMaster.LorePowers.Greenpath;
using LoreMaster.LorePowers.HowlingCliffs;
using LoreMaster.LorePowers.KingdomsEdge;
using LoreMaster.LorePowers.QueensGarden;
using LoreMaster.LorePowers.RestingGrounds;
using LoreMaster.LorePowers.Waterways;
using LoreMaster.LorePowers.WhitePalace;
using LoreMaster.SaveManagement;
using LoreMaster.UnityComponents;
using Modding;
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

    private readonly Dictionary<string, Power> _powerList = new()
    {
        // Ancient Basin
        {"ABYSS_TUT_TAB_01", new WeDontTalkAboutShadePower() },
        // City of Tears
        {"RUINS_TAB_01", new HotStreakPower() },
        {"FOUNTAIN_PLAQUE_MAIN", new TouristPower() },
        {"RUINS_MARISSA_POSTER", new MarissasAudiencePower() },
        {"MAGE_COMP_03", new OverwhelmingPower() },
        {"MAGE_COMP_01", new SoulExtractEfficiencyPower() },
        {"LURIAN_JOURNAL", new EyeOfTheWatcherPower() },
        // Dirtmouth/King's Pass
        {"TUT_TAB_01", new WellFocusedPower() },
        {"TUT_TAB_03", new ScrewTheRulesPower() },
        {"TUT_TAB_02", new TrueFormPower() },
        {"BRETTA", new CaringShellPower() },
        // Fog Canyon
        {"ARCHIVE_01", new FriendOfTheJellyfishPower() },
        {"ARCHIVE_02", new JellyBellyPower() },
        {"ARCHIVE_03", new JellyfishFlowPower() },
        // Crossroads
        {"PILGRIM_TAB_01", new ReluctantPilgerPower() },
        {"COMPLETION_RATE_UNLOCKED", new GreaterMindPower() },
        // Fungal Wastes
        {"FUNG_TAB_04", new OneOfUsPower() },
        {"FUNG_TAB_01", new PaleLuckPower() },
        {"FUNG_TAB_02", new ImposterPower() },
        {"FUNG_TAB_03", new UnitedWeStandPower() },
        {"MANTIS_PLAQUE_01", new MantisStylePower() },
        {"MANTIS_PLAQUE_02", new EternalValorPower() },
        {"PILGRIM_TAB_02", new GloryOfTheWealthPower() },
        // Greenpath
        {"GREEN_TABLET_01", new TouchGrassPower() },
        {"GREEN_TABLET_02", new GiftOfUnnPower() },
        {"GREEN_TABLET_03", new MindblastOfUnnPower() },
        {"GREEN_TABLET_05", new CamouflagePower() },
        {"GREEN_TABLET_06", new ReturnToUnnPower() },
        {"GREEN_TABLET_07", new RootedPower() },
        // Howling Cliffs
        {"CLIFF_TAB_02", new LifebloodWingsPower() },
        {"JONI", new JonisProtectionPower() },
        // Queen's Garden
        {"XUN_GRAVE_INSPECT", new FlowerRingPower() },
        {"QUEEN", new QueenThornsPower() },
        // Resting Grounds
        {"DREAMERS_INSPECT_RG5", new DreamBlessingPower() },
        // Waterways
        {"DUNG_DEF_SIGN", new EternalSentinelPower() },
        {"FLUKE_HERMIT", new RelentlessSwarmPower() },
        // Kingdom's Edge
        {"MR_MUSH_RIDDLE_TAB_NORMAL", new WisdomOfTheSagePower() },
        {"BADOON", new ConcussiveStrikePower() },
        {"HIVEQUEEN", new YouLikeJazzPower() },
        // Deepnest
        {"MASKMAKER", new MaskOverchargePower() },
        {"MIDWIFE", new InfestedPower() },
        // White Palace
        {"WP_WORKSHOP_01", new ShadowForgedPower() },
        {"WP_THRONE_01", new ShiningBoundPower() },
        {"PLAQUE_WARN", new DiminishingCursePower() },
        {"EndOfPathOfPain", new SacredShellPower() }
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
        {Area.WhitePalace, new(){MapZone.WHITE_PALACE} },
        {Area.Peaks, new(){MapZone.PEAK, MapZone.MINES, MapZone.CRYSTAL_MOUND} }
    };

    private bool _fromMenu;

    private Area _currentArea;

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
    public bool UseHints { get; set; }

    /// <summary>
    /// Gets or sets the flag, that indicates if custom text can be used.
    /// </summary>
    public bool UseCustomText { get; set; }

    /// <summary>
    /// Gets or sets all actions of powers that should be executed once the scene loaded. 
    /// This is used to prevent messing around with powers that may use activeSceneChanged.
    /// <para/>No power shall use activeSceneChanged!!!
    /// </summary>
    public Dictionary<string, Action> SceneActions { get; set; } = new();

    /// <summary>
    /// Gets the flag for the toggle button to disable this mod.
    /// </summary>
    public bool ToggleButtonInsideMenu => true;

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
        key = ModifyKey(key);
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
                    text += "<br>For those, that reveals the secret, awaits the power:";
                    text += "<br>[" + popPower.PowerName + "]";
                    text += "<br>" + (UseHints ? popPower.Hint : popPower.Description);
                }
            }
            catch (Exception exception)
            {
                LogError(exception.Message);
            }
        }
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
        ItemChangerMod.CreateSettingsProfile();
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
        orig(self, permaDeath, bossRush);
        ModHooks.SetPlayerBoolHook += ModHooks_SetPlayerBoolHook;
        _fromMenu = true;

        // Reset tags to default.
        foreach (string key in _powerList.Keys)
            _powerList[key].Tag = PowerTag.Local;
        _powerList["EndOfPathOfPain"].Tag = PowerTag.Exclude;

        // Unsure if this is needed, but just in case.
        ActivePowers.Clear();
    }

    private void UIManager_ContinueGame(On.UIManager.orig_ContinueGame orig, UIManager self)
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
    private IEnumerator GameManager_ReturnToMainMenu(On.GameManager.orig_ReturnToMainMenu orig, GameManager self, GameManager.ReturnToMainMenuSaveModes saveMode, Action<bool> callback)
    {
        foreach (Power power in ActivePowers.Values)
            power.DisablePower(true);
        Handler.StopAllCoroutines();
        return orig(self, saveMode, callback);
    }

    /// <summary>
    /// Event handler, for the PoP power.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="orig"></param>
    /// <returns></returns>
    private bool ModHooks_SetPlayerBoolHook(string name, bool orig)
    {
        if (name.Equals("killedBindingSeal") && orig)
        {
            if (!ActivePowers.ContainsKey("EndOfPathOfPain"))
            {
                Power power = _powerList["EndOfPathOfPain"];
                power.EnablePower();
                ActivePowers.Add("EndOfPathOfPain", power);
                UpdateTracker(_currentArea);
            }
            ModHooks.SetPlayerBoolHook -= ModHooks_SetPlayerBoolHook;
        }
        return orig;
    }

    /// <summary>
    /// Event handler used for adjusting the active powers.
    /// </summary>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        try
        {
            if (GameManager.instance.IsGameplayScene())
            {
                if (_fromMenu)
                {
                    if (PlayerData.instance == null)
                        Log("Playerdata doesn't exist");
                    if (PlayerData.instance.GetBool("killedBindingSeal") && !_powerList.ContainsKey("EndOfPathOfPain"))
                    {
                        Power power = _powerList["EndOfPathOfPain"];
                        power.EnablePower();
                        ActivePowers.Add("EndOfPathOfPain", power);
                    }
                    else
                        ModHooks.SetPlayerBoolHook += ModHooks_SetPlayerBoolHook;

                    // Load in changes from the options file (if it exists)
                    LoadOptions();
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
    public override List<(string, string)> GetPreloadNames() => new List<(string, string)>()
    {
        ("RestingGrounds_08", "Ghost Battle Revek"),
        ("sharedassets156", "Lil Jellyfish"),
        ("sharedassets34", "Shot Mantis"),
        ("GG_Hollow_Knight", "Battle Scene/HK Prime/Focus Blast/focus_ring"),
        ("GG_Hollow_Knight", "Battle Scene/HK Prime/Focus Blast/focus_rune")
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
        ModHooks.LanguageGetHook += GetText;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        On.UIManager.StartNewGame += StartNewGame;
        On.UIManager.ContinueGame += UIManager_ContinueGame;
        On.GameManager.ReturnToMainMenu += GameManager_ReturnToMainMenu;

        foreach (string key in preloadedObjects.Keys)
            foreach (string subKey in preloadedObjects[key].Keys)
                if (!PreloadedObjects.ContainsKey(subKey))
                {
                    PreloadedObjects.Add(subKey, preloadedObjects[key][subKey]);
                    GameObject.DontDestroyOnLoad(preloadedObjects[key][subKey]);
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
                if (headline.ToLower().Contains("%modify%"))
                {

                }
                else if (headline.ToLower().Contains("%override%"))
                    ActivePowers.Clear();
                else
                    throw new Exception("Invalid option configuration file");
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
                GameManager.instance.SaveGame();
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
    /// Unloads the mod. (Currently unused)
    /// </summary>
    public void Unload()
    {
        Instance = null;
        ModHooks.LanguageGetHook -= GetText;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        On.UIManager.StartNewGame -= StartNewGame;
        On.GameManager.ReturnToMainMenu -= GameManager_ReturnToMainMenu;
        Handler.StopAllCoroutines();
        foreach (Power power in ActivePowers.Values)
            power.DisablePower(false);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Modifies the language key, to keep consistancy between the lore keys (mostly for NPC).
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string ModifyKey(string key)
    {
        if (key.Contains("Bretta_Diary"))
            key = "BRETTA";
        else if (IsMidwife(key))
            key = "MIDWIFE";
        else if (IsBardoon(key))
            key = "BADOON";
        else if (key.Equals("HIVEQUEEN_TALK") || key.Equals("HIVEQUEEN_REPEAT"))
            key = "HIVEQUEEN";
        else if (key.Equals("JONI_TALK") || key.Equals("JONI_REPEAT"))
            key = "JONI";
        else if (IsFlukeHermit(key))
            key = "FLUKE_HERMIT";
        else if (IsQueen(key))
            key = "QUEEN";
        else if (IsMaskMaker(key))
            key = "MASKMAKER";
        return key;
    }

    /// <summary>
    /// Manages the activation/deactivation of powers and scene change trigger.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ManageSceneActions()
    {
        yield return new WaitForFinishedEnteringScene();
        Area newArea = _currentArea;
        if (!Enum.TryParse(GameManager.instance.GetCurrentMapZone(), out MapZone newMapZone))
            LogError("Couldn't convert map zone to enum. Value: " + GameManager.instance.GetCurrentMapZone());
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
        if (_currentArea != newArea)
        {
            // Activate all local abilities
            foreach (Power power in ActivePowers.Values.Where(x => x.Location == newArea))
                if (power.Tag == PowerTag.Exclude || power.Tag == PowerTag.Local)
                    power.EnablePower();

            // Disable all local abilities from the other zone
            if (!IsAreaGlobal(_currentArea))
                foreach (Power power in ActivePowers.Values.Where(x => x.Location == _currentArea))
                    if (power.Tag == PowerTag.Local || power.Tag == PowerTag.Exclude)
                        power.DisablePower(false);

            // To prevent making all members public, we manually call the completion counter here.
            UpdateTracker(newArea);
        }

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
            // To prevent making all members public, we manually call the completion counter here.
            UpdateTracker(newArea);
        }

        foreach (Action action in SceneActions.Values)
            action();
        _currentArea = newArea;
    }

    /// <summary>
    /// Checks through all needed powers to determine if the powers should be granted globally.
    /// </summary>
    /// <param name="toCheck"></param>
    /// <returns></returns>
    private bool IsAreaGlobal(Area toCheck)
    {

        List<Power> neededAreaPowers = _powerList.Values.Where(x => x.Location == toCheck && (x.Tag == PowerTag.Local || x.Tag == PowerTag.Disabled)).ToList();
        foreach (Power neededPower in neededAreaPowers)
            if (!ActivePowers.ContainsValue(neededPower))
                return false;
        return true;
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
                    logPower.UpdateLoreCounter(ActivePowers.Values, _powerList.Values, areaToUpdate, IsAreaGlobal(_currentArea));
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

    #endregion

    #region Save Management

    public void OnLoadGlobal(LoreMasterGlobalSaveData globalSaveData)
    {
        UseHints = globalSaveData.ShowHint;
        UseCustomText = globalSaveData.EnableCustomText;
    }

    LoreMasterGlobalSaveData IGlobalSettings<LoreMasterGlobalSaveData>.OnSaveGlobal()
        => new() { ShowHint = UseHints, EnableCustomText = UseCustomText };

    public void OnLoadLocal(LoreMasterLocalSaveData s)
    {
        ActivePowers.Clear();
        foreach (string key in s.Tags.Keys)
            _powerList[key].Tag = s.Tags[key];

        foreach (string key in s.AcquiredPowersKey)
            ActivePowers.Add(key, _powerList[key]);
    }

    LoreMasterLocalSaveData ILocalSettings<LoreMasterLocalSaveData>.OnSaveLocal()
    {
        LoreMasterLocalSaveData saveData = new();

        foreach (string key in _powerList.Keys)
            saveData.Tags.Add(key, _powerList[key].Tag);

        foreach (string key in ActivePowers.Keys)
            saveData.AcquiredPowersKey.Add(key);

        return saveData;
    }

    #endregion

    public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
    {
        
        return new List<IMenuMod.MenuEntry>
        {
            new IMenuMod.MenuEntry {
                Name = "Custom Text",
                Description = "Replaces the text of tablets of conversations (if available).",
                Values = new string[] {
                    "On",
                    "Off",
                },
                // opt will be the index of the option that has been chosen
                Saver = option => UseCustomText = option == 0,
                Loader = () => UseCustomText ? 0 : 1
            },
            new IMenuMod.MenuEntry {
                Name = "Use Vague Hints",
                Description = "If on, it shows the normal more vaguely text. Otherwise it shows a clear description what the power does.",
                Values = new string[] {
                    "On",
                    "Off"
                },
                Saver = option => UseHints = option == 0,
                Loader = () => UseHints ? 0 : 1
            }
        };
    }

    #endregion
}
