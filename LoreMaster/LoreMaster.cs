using ItemChanger;
using ItemChanger.Locations;
using KorzUtils.Helper;
using LoreCore.Data;
using LoreCore.Enums;
using LoreCore.Modules;
using LoreCore.Other;
using LoreMaster.Manager;
using LoreMaster.ModInterop;
using LoreMaster.SaveManagement;
using LoreMaster.Settings;
using LoreMaster.UnityComponents;
using Modding;
using Newtonsoft.Json;
using SFCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using static LoreCore.Data.ItemList;
using static LoreCore.Data.LocationList;

namespace LoreMaster;

public class LoreMaster : Mod, IGlobalSettings<LoreMasterGlobalSaveData>, IMenuMod
{
    #region Constructors

    public LoreMaster()
    {
        Instance = this;
        //MenuManager.AddMode();
        InventoryHelper.AddInventoryPage(InventoryPageType.Empty, "Lore", "LoreMaster", "LoreMaster", "LoreArtifact", LorePage.GeneratePage);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the running instance of the mod.
    /// </summary>
    public static LoreMaster Instance { get; set; }

    public RandoSettings RandomizerSettings { get; set; } = new();

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

    #region Eventhandler

    private System.Collections.IEnumerator UIManager_ReturnToMainMenu(On.UIManager.orig_ReturnToMainMenu orig, UIManager self)
    {
        LoreManager.Unload();
        yield return orig(self);
    }

    private void UIManager_ContinueGame(On.UIManager.orig_ContinueGame orig, UIManager self)
    {
        orig(self);
        LoreManager.Initialize();
    }

    private void UIManager_StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
    {
        orig(self, permaDeath, bossRush);

        // If any traveller location is already known to IC, the rando did setup the traveller stages already.
        if (!ItemChanger.Internal.Ref.Settings?.Placements?.ContainsKey(Dialogue_Quirrel_City) == true)
            foreach (Traveller traveller in TravellerControlModule.CurrentModule.Stages.Keys)
                TravellerControlModule.CurrentModule.Stages[traveller] = 0;

        // To do: Rando check
        CreateVanillaPlacements(!RandoInterop.PlayingRandomizer);
        LoreManager.Initialize();
        LorePage.UpdateLorePage();
    }

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

            // IC
            AddToICFinder();
            On.UIManager.StartNewGame += UIManager_StartNewGame;
            On.UIManager.ContinueGame += UIManager_ContinueGame;
            On.UIManager.ReturnToMainMenu += UIManager_ReturnToMainMenu;

            if (ModHooks.GetMod("DebugMod") is Mod)
                HookDebug();
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
                Saver = option => LoreManager.GlobalSaveData.EnableCustomText = option == 0,
                Loader = () => LoreManager.GlobalSaveData.EnableCustomText ? 0 : 1
            },
            new()
            {
                Name = "Power Explanations",
                Description = "Determines how powers show be descripted",
                Values = new string[] { "Vague Hints", "Descriptions" },
                Saver = option => LoreManager.GlobalSaveData.ShowHint = option == 0,
                Loader = () => LoreManager.GlobalSaveData.ShowHint ? 0 : 1
            },
            new()
            {
                Name = "Disable Yellow Mushroom",
                Description = "If on, the yellow mushroom will not cause a nausea effect.",
                Values = new string[] { "On", "Off" },
                Saver = option => LoreManager.GlobalSaveData.DisableNausea = option == 0,
                Loader = () => LoreManager.GlobalSaveData.DisableNausea ? 0 : 1
            },
            new()
            {
                Name = "Tracker Permanent",
                Description = "If off, the tracker will disappear after 5 seconds.",
                Values = new string[] { "On", "Off" },
                Saver = option => LoreManager.GlobalSaveData.TrackerPermanently = option == 0,
                Loader = () => LoreManager.GlobalSaveData.TrackerPermanently ? 0 : 1
            }
        };

        return menu;
    }

    #endregion

    #region Save Management

    /// <summary>
    /// Loads the data for the global mod settings.
    /// </summary>
    /// <param name="globalSaveData"></param>
    public void OnLoadGlobal(LoreMasterGlobalSaveData globalSaveData)
    {
        globalSaveData ??= new();
        LoreManager.GlobalSaveData = globalSaveData;
    }

    /// <summary>
    /// Saves the data for the global mod settings.
    /// </summary>
    LoreMasterGlobalSaveData IGlobalSettings<LoreMasterGlobalSaveData>.OnSaveGlobal()
    {
        LoreManager.GlobalSaveData ??= new();
        return new()
        {
            ShowHint = LoreManager.GlobalSaveData.ShowHint,
            EnableCustomText = LoreManager.GlobalSaveData.EnableCustomText,
            TrackerPermanently = LoreManager.GlobalSaveData.TrackerPermanently,
            DisableNausea = LoreManager.GlobalSaveData.DisableNausea
        };
    }

    #endregion

    #region IC Setup

    private static void AddToICFinder()
    {
        Finder.DefineCustomLocation(new CoordinateLocation() { x = 35.0f, y = 5.4f, elevation = 0, sceneName = "Ruins1_27", name = "City_Teleporter" });
        Finder.DefineCustomLocation(new CoordinateLocation() { x = 57f, y = 5f, elevation = 0, sceneName = "Room_temple", name = "Temple_Teleporter" });

        using Stream itemStream = ResourceHelper.LoadResource<LoreMaster>("Items.json");
        using StreamReader reader = new(itemStream);
        JsonSerializer jsonSerializer = new()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
        foreach (AbstractItem item in jsonSerializer.Deserialize<List<AbstractItem>>(new JsonTextReader(reader)))
            Finder.DefineCustomItem(item);
    }

    /// <summary>
    /// Creates and adds all placement with their vanilla items to the save file.
    /// </summary>
    /// <param name="generateSettings">If <paramref name="generateSettings"/> the item changer settings will be generated. Only use this, if rando is not unused.</param>
    public void CreateVanillaPlacements(bool generateSettings = false)
    {
        LoreCore.LoreCore.Instance.CreateVanillaCustomLore(generateSettings);
        List<AbstractPlacement> placements =
        [
            // Npc
            GeneratePlacement(Dialogue_Bardoon, Bardoon),
            GeneratePlacement(Dialogue_Bretta, Bretta),
            GeneratePlacement(Dialogue_Dung_Defender, Dung_Defender),
            GeneratePlacement(Dialogue_Emilitia, Emilitia),
            GeneratePlacement(Dialogue_Fluke_Hermit, Fluke_Hermit),
            GeneratePlacement(Dialogue_Grasshopper, Grasshopper),
            GeneratePlacement(Dialogue_Gravedigger, Gravedigger),
            GeneratePlacement(Dialogue_Joni, Joni),
            GeneratePlacement(Dialogue_Marissa, Marissa),
            GeneratePlacement(Dialogue_Mask_Maker, Mask_Maker),
            GeneratePlacement(Dialogue_Menderbug_Diary, Menderbug_Diary),
            GeneratePlacement(Dialogue_Midwife, Midwife),
            GeneratePlacement(Dialogue_Moss_Prophet, Moss_Prophet),
            GeneratePlacement(Dialogue_Myla, Myla),
            GeneratePlacement(Dialogue_Poggy, Poggy),
            GeneratePlacement(Dialogue_Queen, Queen),
            GeneratePlacement(Dialogue_Vespa, Vespa),
            GeneratePlacement(Dialogue_Willoh, Willoh),
            // Dream dialogue
            GeneratePlacement(Dream_Dialogue_Ancient_Nailsmith_Golem, Ancient_Nailsmith_Golem_Dream),
            GeneratePlacement(Dream_Dialogue_Aspid_Queen, Aspid_Queen_Dream),
            GeneratePlacement(Dream_Dialogue_Crystalized_Shaman, Crystalized_Shaman_Dream),
            GeneratePlacement(Dream_Dialogue_Dashmaster_Statue, Dashmaster_Statue_Dream),
            GeneratePlacement(Dream_Dialogue_Dream_Shield_Statue, Dream_Shield_Statue_Dream),
            GeneratePlacement(Dream_Dialogue_Dryya, Dryya_Dream),
            GeneratePlacement(Dream_Dialogue_Grimm_Summoner, Grimm_Summoner_Dream),
            GeneratePlacement(Dream_Dialogue_Hopper_Dummy, Hopper_Dummy_Dream),
            GeneratePlacement(Dream_Dialogue_Isma, Isma_Dream),
            GeneratePlacement(Dream_Dialogue_Kings_Mould_Machine, Kings_Mould_Machine_Dream),
            GeneratePlacement(Dream_Dialogue_Mine_Golem, Mine_Golem_Dream),
            GeneratePlacement(Dream_Dialogue_Overgrown_Shaman, Overgrown_Shaman_Dream),
            GeneratePlacement(Dream_Dialogue_Pale_King, Pale_King_Dream),
            GeneratePlacement(Dream_Dialogue_Radiance_Statue, Radiance_Statue_Dream),
            GeneratePlacement(Dream_Dialogue_Shade_Golem_Normal, Shade_Golem_Dream_Normal),
            GeneratePlacement(Dream_Dialogue_Shade_Golem_Void, Shade_Golem_Dream_Void),
            GeneratePlacement(Dream_Dialogue_Shriek_Statue, Shriek_Statue_Dream),
            GeneratePlacement(Dream_Dialogue_Shroom_King, Shroom_King_Dream),
            GeneratePlacement(Dream_Dialogue_Snail_Shaman_Tomb, Snail_Shaman_Tomb_Dream),
            GeneratePlacement(Dream_Dialogue_Tiso_Corpse, Tiso_Corpse),
            // Point of interest
            GeneratePlacement(Inscription_City_Fountain, City_Fountain),
            GeneratePlacement(Inscription_Dreamer_Tablet, Dreamer_Tablet),
            GeneratePlacement(Inspect_Stag_Egg, Stag_Nest).Add(Finder.GetItem("Stag_Egg")),
            GeneratePlacement(ItemList.Lore_Tablet_Record_Bela, LocationList.Lore_Tablet_Record_Bela),
            GeneratePlacement(Inspect_Beast_Den_Altar, Beast_Den_Altar),
            GeneratePlacement(Inspect_Garden_Golem, Garden_Golem),
            GeneratePlacement(Inspect_Grimm_Machine, Grimm_Machine),
            GeneratePlacement(Inspect_Grimm_Summoner_Corpse, Grimm_Summoner_Corpse),
            GeneratePlacement(Inspect_Grub_Seal, Grub_Seal),
            GeneratePlacement(Inspect_White_Palace_Nursery, White_Palace_Nursery),
            GeneratePlacement(Inspect_Gorb, Gorb_Grave),
            GeneratePlacement(Inspect_Galien, Galien_Corpse),
            GeneratePlacement(Inspect_Elder_Hu, Elder_Hu_Grave),
            GeneratePlacement(Inspect_Markoth, Markoth_Corpse),
            GeneratePlacement(Inspect_Marmu, Marmu_Grave),
            GeneratePlacement(Inspect_No_Eyes, No_Eyes_Statue),
            GeneratePlacement(Inspect_Xero, Xero_Grave),
            // Traveller
            // Quirrel
            GeneratePlacement(Dialogue_Quirrel_Archive, Quirrel_After_Monomon),
            GeneratePlacement(Dialogue_Quirrel_Blue_Lake, Quirrel_Blue_Lake),
            GeneratePlacement(Dialogue_Quirrel_Peaks, Quirrel_Peaks),
            GeneratePlacement(Dialogue_Quirrel_City, Quirrel_City),
            GeneratePlacement(Dialogue_Quirrel_Deepnest, Quirrel_Deepnest),
            GeneratePlacement(Dialogue_Quirrel_Crossroads, Quirrel_Crossroads),
            GeneratePlacement(Dialogue_Quirrel_Greenpath, Quirrel_Greenpath),
            GeneratePlacement(Dialogue_Quirrel_Mantis_Village, Quirrel_Mantis_Village),
            GeneratePlacement(Dialogue_Quirrel_Queen_Station, Quirrel_Queen_Station),
            GeneratePlacement(Dialogue_Quirrel_Outside_Archive, Quirrel_Outside_Archive),
            // Tiso
            GeneratePlacement(Dialogue_Tiso_Blue_Lake, Tiso_Blue_Lake),
            GeneratePlacement(Dialogue_Tiso_Colosseum, Tiso_Colosseum),
            GeneratePlacement(Dialogue_Tiso_Crossroads, Tiso_Crossroads),
            GeneratePlacement(Dialogue_Tiso_Dirtmouth, Tiso_Dirtmouth),
            // Zote
            GeneratePlacement(Dialogue_Zote_Greenpath, Zote_Greenpath),
            GeneratePlacement(Dialogue_Zote_Dirtmouth_Intro, Zote_Dirtmouth_Intro),
            GeneratePlacement(Dialogue_Zote_City, Zote_City),
            GeneratePlacement(Dialogue_Zote_Deepnest, Zote_Deepnest),
            GeneratePlacement(Dialogue_Zote_Colosseum, Zote_Colosseum),
            GeneratePlacement(Dialogue_Zote_Dirtmouth_After_Colosseum, Zote_Dirtmouth_After_Colosseum),
            // Cloth
            GeneratePlacement(Dialogue_Cloth_Fungal_Wastes, Cloth_Fungal_Wastes),
            GeneratePlacement(Dialogue_Cloth_Basin, Cloth_Basin),
            GeneratePlacement(Dialogue_Cloth_Deepnest, Cloth_Deepnest),
            GeneratePlacement(Dialogue_Cloth_Garden, Cloth_Garden),
            new DualLocation()
            {
                name = Cloth_End,
                trueLocation = Finder.GetLocation(Cloth_Ghost),
                falseLocation = Finder.GetLocation(Cloth_Town),
                Test = new ClothTest()
            }.Wrap().Add(Finder.GetItem(Dialogue_Cloth_Ghost))
        ];

        int[] loreCost =
        [
            2,
            5,
            7,
            10,
            20,
            25,
            30,
            35,
            40,
            45,
            50,
            55,
            60,
            65,
            70,
            77,
            86,
            99
        ];
        string[] items =
        [
            "Small_Glyph",
            "Minor_Glyph",
            "Small_Glyph",
            "Major_Glyph", // 10
            "Small_Glyph",
            "Cleansing_Scroll",
            "Minor_Glyph",
            "Small_Glyph",
            "Major_Glyph", // 40
            "Mystical_Scroll",
            "Small_Glyph",
            "Minor_Glyph",
            "Small_Glyph",
            "Minor_Glyph",
            "Major_Glyph", // 70
            "Small_Glyph",
            "Minor_Glyph",
            "Small_Glyph"
        ];

        AbstractPlacement elderBugPlacement = ItemChanger.Internal.Ref.Settings.Placements.ContainsKey(Elderbug_Shop) 
            ? ItemChanger.Internal.Ref.Settings.Placements[Elderbug_Shop]
            : Finder.GetLocation(Elderbug_Shop).Wrap();
        for (int i = 0; i < items.Length; i++)
        {
            AbstractItem abstractItem = Finder.GetItem(items[i]);
            abstractItem.tags ??= [];
            abstractItem.AddTag(new CostTag() { Cost = new LoreCost() { NeededLore = loreCost[i] } });
            elderBugPlacement.Add(abstractItem);
        }
        if (!ItemChanger.Internal.Ref.Settings.Placements.ContainsKey(Elderbug_Shop))
            placements.Add(elderBugPlacement);
        placements.Add(Finder.GetLocation("City_Teleporter").Wrap().Add(Finder.GetItem("City_Ticket")));
        placements.Add(Finder.GetLocation("Temple_Teleporter").Wrap().Add(Finder.GetItem("Temple_Ticket")));
        ItemChangerMod.AddPlacements(placements);
    }

    private AbstractPlacement GeneratePlacement(string itemName, string locationName)
        => Finder.GetLocation(locationName).Wrap().Add(Finder.GetItem(itemName));

    #endregion

    #region ModInterop

    private void HookDebug() => DebugInterop.Initialize();

    private void HookRando() => RandoInterop.Initialize();
    
    #endregion

    #endregion
}
