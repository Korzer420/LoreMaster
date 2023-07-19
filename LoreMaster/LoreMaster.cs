using LoreMaster.LorePowers;
using LoreMaster.Manager;
using LoreMaster.SaveManagement;
using LoreMaster.UnityComponents;
using Modding;
using SFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LoreMaster;

public class LoreMaster : Mod, IGlobalSettings<LoreMasterGlobalSaveData>, ILocalSettings<LoreMasterLocalSaveData>, IMenuMod
{
    #region Constructors

    public LoreMaster()
    {
        InitializeManager();
        LorePage.PassPowers(PowerManager.GetAllPowers().ToList());
        InventoryHelper.AddInventoryPage(InventoryPageType.Empty, "Lore", "LoreMaster", "LoreMaster", "LoreArtifact", LorePage.GeneratePage);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the running instance of the mod.
    /// </summary>
    public static LoreMaster Instance { get; set; }

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
    /// Gets the flag for the toggle button to disable this mod.
    /// </summary>
    public bool ToggleButtonInsideMenu => true;

    #endregion

    #region Methods

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
        ("Ruins1_28", "Flamebearer Spawn"), // Small Ghost
        ("RestingGrounds_06", "Flamebearer Spawn"), // Medium Ghost
        ("Hive_03", "Flamebearer Spawn"), // Large Ghost
        ("GG_Hollow_Knight", "Battle Scene/HK Prime/Focus Blast/focus_ring"),
        ("GG_Hollow_Knight", "Battle Scene/HK Prime/Focus Blast/focus_rune"),
        ("Fungus1_01b", "green_grass_1"),
        ("White_Palace_09", "ash_grass_02"),
        ("Ruins1_01", "Ceiling Dropper"),
        ("Ruins1_23", "Glow Response Mage Computer"), // Soul sanctum lore tablet.
        ("Ruins1_23", "Inspect Region"), // Inspect region for soul sanctum tablet.
        ("Ruins1_23", "Mage"),
        ("Deepnest_East_16", "Quake Floor"),
        ("Crossroads_47", "Stag"),
        ("Abyss_15", "Shade Sibling (25)"),
        ("Fungus1_22", "Plant Trap"),
        ("Fungus3_02", "Jellyfish"),
        ("Fungus3_02", "Jellyfish Baby"),
        ("Ruins_Elevator", "Ghost NPC"),
        ("Room_nailsmith", "Nailsmith"),
        ("Deepnest_41", "Spider Flyer"),
        ("Deepnest_East_10", "Dream Gate"),
        ("Crossroads_46", "Tram Call Box")
    };

    /// <summary>
    /// Does the initialization needed for the mod.
    /// </summary>
    /// <param name="preloadedObjects"></param>
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        GameObject loreManager = new("LoreManager");
        GameObject.DontDestroyOnLoad(loreManager);
        Handler = loreManager.AddComponent<CoroutineHandler>();

        int grimmkinIndex = 1;
        try
        {
            foreach (string key in preloadedObjects.Keys)
                foreach (string subKey in preloadedObjects[key].Keys)
                    if (!PreloadedObjects.ContainsKey(subKey) || subKey.Equals("Flamebearer Spawn"))
                    {
                        GameObject toAdd = preloadedObjects[key][subKey];
                        if (subKey.Equals("Mantis Heavy Flyer"))
                            toAdd = toAdd.GetComponent<PersonalObjectPool>().startupPool[0].prefab;
                        else if (subKey.Equals("Flamebearer Spawn"))
                        {
                            string realKey = grimmkinIndex == 1 ? "Small Ghost" : (grimmkinIndex == 2 ? "Medium Ghost" : "Large Ghost");
                            toAdd = toAdd.LocateMyFSM("Spawn Control").FsmVariables.FindFsmGameObject("Grimmkin Obj").Value;
                            PreloadedObjects.Add(realKey, toAdd);
                            GameObject.DontDestroyOnLoad(toAdd);
                            grimmkinIndex++;
                            continue;
                        }
                        else if (subKey.Equals("Mage"))
                            toAdd = toAdd.GetComponent<PersonalObjectPool>().startupPool[0].prefab;
                        PreloadedObjects.Add(subKey, toAdd);
                        GameObject.DontDestroyOnLoad(toAdd);
                    }
        }
        catch (Exception exception)
        {
            LogError("Error while preloading: " + exception.Message);
        }

    }

    /// <summary>
    /// Handles the mod menu.
    /// </summary>
    public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
    {
        List<IMenuMod.MenuEntry> menu = new()
        {
            new()
            {
                Name = "Custom Text",
                Description = "Replaces the text of tablets or conversations (if available).",
                Values = new string[] { "On", "Off" },
                Saver = option => LoreManager.UseCustomText = option == 0,
                Loader = () => LoreManager.UseCustomText ? 0 : 1
            },
            new()
            {
                Name = "Power Explanations",
                Description = "Determines how powers show be descripted",
                Values = new string[] { "Vague Hints", "Descriptions" },
                Saver = option => LoreManager.UseHints = option == 0,
                Loader = () => LoreManager.UseHints ? 0 : 1
            },
            //new()
            //{
            //    Name = "Disable Yellow Mushroom",
            //    Description = "If on, the yellow mushroom will not cause a nausea effect.",
            //    Values = new string[] { "On", "Off" },
            //    Saver = option => SettingManager.Instance.DisableYellowMushroom = option == 0,
            //    Loader = () => SettingManager.Instance.DisableYellowMushroom ? 0 : 1
            //},
            //new()
            //{
            //    Name = "Allow Bomb quick cast",
            //    Description = "If on, the bomb spell can cast via quickcast.",
            //    Values = new string[] { "On", "Off" },
            //    Saver = option => SettingManager.Instance.BombQuickCast = option == 0,
            //    Loader = () => SettingManager.Instance.BombQuickCast ? 0 : 1
            //},
            new()
            {
                Name = "Tracker Permanent",
                Description = "If off, the tracker will disappear after 5 seconds.",
                Values = new string[] { "On", "Off" },
                Saver = option => LorePowers.Crossroads.GreaterMindPower.PermanentTracker = option == 0,
                Loader = () => LorePowers.Crossroads.GreaterMindPower.PermanentTracker ? 0 : 1
            },
            new()
            {
                Name = "Tracker Mode",
                Description = "If Rando, the tracker will display the RANDOMIZED(!) lore power item amount.",
                Values = new string[] { "Normal", "Rando" },
                Saver = option => LorePowers.Crossroads.GreaterMindPower.NormalTracker = option == 0,
                Loader = () => LorePowers.Crossroads.GreaterMindPower.NormalTracker ? 0 : 1
            }
        };

        return menu;
    }

    /// <summary>
    /// Initializes all manager.
    /// </summary>
    public void InitializeManager()
    {
        Instance = this;
        MenuManager.AddMode();
    }

    #endregion

    #region Save Management

    /// <summary>
    /// Loads the data for the global mod settings.
    /// </summary>
    /// <param name="globalSaveData"></param>
    public void OnLoadGlobal(LoreMasterGlobalSaveData globalSaveData)
    {
        LogDebug("Loaded global data");
        InitializeManager();
        LoreManager.UseHints = globalSaveData.ShowHint;
        LoreManager.UseCustomText = globalSaveData.EnableCustomText;
        LorePowers.Crossroads.GreaterMindPower.PermanentTracker = globalSaveData.TrackerPermanently;
    }

    /// <summary>
    /// Saves the data for the global mod settings.
    /// </summary>
    LoreMasterGlobalSaveData IGlobalSettings<LoreMasterGlobalSaveData>.OnSaveGlobal()
        => new()
        {
            ShowHint = LoreManager.UseHints,
            EnableCustomText = LoreManager.UseCustomText,
            TrackerPermanently = LorePowers.Crossroads.GreaterMindPower.PermanentTracker
        };

    /// <summary>
    /// Loads the data from the save file.
    /// </summary>
    public void OnLoadLocal(LoreMasterLocalSaveData saveData)
    {
        try
        {
            PowerManager.ControlState = saveData.PageState;
        }
        catch (Exception exception)
        {
            LogError("Error while loading local save data: " + exception.Message);
            LogError(exception.StackTrace);
        }
    }

    /// <summary>
    /// Saves the data from the save file.
    /// </summary>
    LoreMasterLocalSaveData ILocalSettings<LoreMasterLocalSaveData>.OnSaveLocal()
    {
        LoreMasterLocalSaveData saveData = new();
        try
        {
            saveData.PageState = PowerManager.ControlState;
        }
        catch (Exception ex)
        {
            LoreMaster.Instance.LogError("An error occured while saving local: " + ex.StackTrace);
        }
        return saveData;
    }

    #endregion 

    #endregion
}
