using ItemChanger;
using ItemChanger.Internal;
using LoreMaster.Enums;
using LoreMaster.ItemChangerData.Items;
using LoreMaster.ItemChangerData.Other;
using LoreMaster.LorePowers.Crossroads;
using LoreMaster.Manager;
using LoreMaster.SaveManagement;
using LoreMaster.Settings;
using Modding;
using RandomizerMod.Extensions;
using RandomizerMod.Logging;
using RandomizerMod.RandomizerData;
using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LoreMaster.Randomizer;

public static class RandomizerManager
{
    #region Members

    private static RandomizerSettings _settings;

    private static Dictionary<Area, string> _randoAreaNames = new()
    {
        {Area.None, "Menu" },
        {Area.AncientBasin, "Ancient Basin" },
        {Area.CityOfTears, "City of Tears" },
        {Area.Dirtmouth, "Dirtmouth"},
        {Area.Crossroads, "Forgotten Crossroads"},
        {Area.Greenpath, "Greenpath"},
        {Area.Deepnest, "Deepnest" },
        {Area.FungalWastes, "Fungal Wastes"},
        {Area.QueensGarden, "Queen's Gardens"},
        {Area.Peaks, "Crystal Peaks"},
        {Area.RestingGrounds, "Resting Grounds"},
        {Area.WaterWays, "Royal Waterways"},
        {Area.KingdomsEdge, "Kingdom's Edge"},
        {Area.FogCanyon, "Fog Canyon"},
        {Area.Cliffs, "Howling Cliffs"},
        {Area.WhitePalace, "White Palace"},
    };

    private static List<string> _loreItemList = new()
    {
        ItemNames.Lore_Tablet_Ancient_Basin+ "_Empowered",
        ItemNames.Lore_Tablet_Archives_Left+ "_Empowered",
        ItemNames.Lore_Tablet_Archives_Right+ "_Empowered",
        ItemNames.Lore_Tablet_Archives_Upper+ "_Empowered",
        ItemNames.Lore_Tablet_City_Entrance+ "_Empowered",
        ItemNames.Lore_Tablet_Dung_Defender+ "_Empowered",
        ItemNames.Lore_Tablet_Fungal_Core+ "_Empowered",
        ItemNames.Lore_Tablet_Fungal_Wastes_Below_Shrumal_Ogres+ "_Empowered",
        ItemNames.Lore_Tablet_Fungal_Wastes_Hidden+ "_Empowered",
        ItemNames.Lore_Tablet_Greenpath_Below_Toll+ "_Empowered",
        ItemNames.Lore_Tablet_Greenpath_Lifeblood+ "_Empowered",
        ItemNames.Lore_Tablet_Greenpath_Lower_Hidden+ "_Empowered",
        ItemNames.Lore_Tablet_Greenpath_QG+ "_Empowered",
        ItemNames.Lore_Tablet_Greenpath_Stag+ "_Empowered",
        ItemNames.Lore_Tablet_Greenpath_Upper_Hidden+ "_Empowered",
        ItemNames.Lore_Tablet_Howling_Cliffs+ "_Empowered",
        ItemNames.Lore_Tablet_Kingdoms_Edge+ "_Empowered",
        ItemNames.Lore_Tablet_Kings_Pass_Exit+ "_Empowered",
        ItemNames.Lore_Tablet_Kings_Pass_Focus+ "_Empowered",
        ItemNames.Lore_Tablet_Kings_Pass_Fury+ "_Empowered",
        ItemNames.Lore_Tablet_Mantis_Outskirts+ "_Empowered",
        ItemNames.Lore_Tablet_Mantis_Village+ "_Empowered",
        ItemNames.Lore_Tablet_Palace_Throne+ "_Empowered",
        ItemNames.Lore_Tablet_Palace_Workshop+ "_Empowered",
        ItemNames.Lore_Tablet_Path_of_Pain_Entrance+ "_Empowered",
        ItemNames.Lore_Tablet_Pilgrims_Way_1+ "_Empowered",
        ItemNames.Lore_Tablet_Pilgrims_Way_2+ "_Empowered",
        ItemNames.Lore_Tablet_Pleasure_House+ "_Empowered",
        ItemNames.Lore_Tablet_Sanctum_Entrance+ "_Empowered",
        ItemNames.Lore_Tablet_Sanctum_Past_Soul_Master+ "_Empowered",
        ItemNames.Lore_Tablet_Spore_Shroom+ "_Empowered",
        ItemNames.Lore_Tablet_Watchers_Spire+ "_Empowered",
        ItemNames.Lore_Tablet_World_Sense+ "_Empowered",
        ItemList.Dialogue_Bardoon,
        ItemList.Dialogue_Bretta_Diary,
        ItemList.Dialogue_Cloth_Basin,
        ItemList.Dialogue_Cloth_Deepnest,
        ItemList.Dialogue_Cloth_Dirtmouth,
        ItemList.Dialogue_Cloth_Fungal_Wastes,
        ItemList.Dialogue_Cloth_Garden,
        ItemList.Dialogue_Cloth_Ghost,
        ItemList.Dialogue_Dung_Defender,
        ItemList.Dialogue_Emilitia,
        ItemList.Dialogue_Fluke_Hermit,
        ItemList.Dialogue_Grasshopper,
        ItemList.Dialogue_Gravedigger,
        ItemList.Dialogue_Joni,
        ItemList.Dialogue_Marissa,
        ItemList.Dialogue_Mask_Maker,
        ItemList.Dialogue_Menderbug_Diary,
        ItemList.Dialogue_Midwife,
        ItemList.Dialogue_Moss_Prophet,
        ItemList.Dialogue_Myla,
        ItemList.Dialogue_Poggy,
        ItemList.Dialogue_Queen,
        ItemList.Dialogue_Quirrel_Archive,
        ItemList.Dialogue_Quirrel_Blue_Lake,
        ItemList.Dialogue_Quirrel_City,
        ItemList.Dialogue_Quirrel_Crossroads,
        ItemList.Dialogue_Quirrel_Deepnest,
        ItemList.Dialogue_Quirrel_Greenpath,
        ItemList.Dialogue_Quirrel_Mantis_Village,
        ItemList.Dialogue_Quirrel_Outside_Archive,
        ItemList.Dialogue_Quirrel_Peaks,
        ItemList.Dialogue_Quirrel_Queen_Station,
        ItemList.Dialogue_Tiso_Blue_Lake,
        ItemList.Dialogue_Tiso_Colosseum,
        ItemList.Dialogue_Tiso_Crossroads,
        ItemList.Dialogue_Tiso_Dirtmouth,
        ItemList.Dialogue_Vespa,
        ItemList.Dialogue_Willoh,
        ItemList.Dialogue_Zote_City,
        ItemList.Dialogue_Zote_Colosseum,
        ItemList.Dialogue_Zote_Deepnest,
        ItemList.Dialogue_Zote_Dirtmouth_After_Colosseum,
        ItemList.Dialogue_Zote_Dirtmouth_Intro,
        ItemList.Dialogue_Zote_Greenpath,
        ItemList.Dream_Dialogue_Ancient_Nailsmith_Golem,
        ItemList.Dream_Dialogue_Aspid_Queen,
        ItemList.Dream_Dialogue_Crystalized_Shaman,
        ItemList.Dream_Dialogue_Dashmaster_Statue,
        ItemList.Dream_Dialogue_Dream_Shield_Statue,
        ItemList.Dream_Dialogue_Dryya,
        ItemList.Dream_Dialogue_Grimm_Summoner,
        ItemList.Dream_Dialogue_Hopper_Dummy,
        ItemList.Dream_Dialogue_Isma,
        ItemList.Dream_Dialogue_Kings_Mould_Machine,
        ItemList.Dream_Dialogue_Mine_Golem,
        ItemList.Dream_Dialogue_Overgrown_Shaman,
        ItemList.Dream_Dialogue_Pale_King,
        ItemList.Dream_Dialogue_Radiance_Statue,
        ItemList.Dream_Dialogue_Shade_Golem_Normal,
        ItemList.Dream_Dialogue_Shade_Golem_Void,
        ItemList.Dream_Dialogue_Shriek_Statue,
        ItemList.Dream_Dialogue_Shroom_King,
        ItemList.Dream_Dialogue_Snail_Shaman_Tomb,
        ItemList.Dream_Dialogue_Tiso_Corpse,
        ItemList.Inscription_City_Fountain,
        ItemList.Inscription_Dreamer_Tablet,
        ItemList.Inspect_Beast_Den_Altar,
        ItemList.Inspect_Elder_Hu,
        ItemList.Inspect_Galien,
        ItemList.Inspect_Garden_Golem,
        ItemList.Inspect_Gorb,
        ItemList.Inspect_Grimm_Machine,
        ItemList.Inspect_Grimm_Summoner_Corpse,
        ItemList.Inspect_Grub_Seal,
        ItemList.Inspect_Markoth,
        ItemList.Inspect_Marmu,
        ItemList.Inspect_No_Eyes,
        ItemList.Inspect_Weaver_Seal,
        ItemList.Inspect_White_Palace_Nursery,
        ItemList.Inspect_Xero,
        ItemList.Stag_Egg_Inspect
    };

    #endregion

    #region Properties

    /// <summary>
    /// Gets the settings of a randomizer.
    /// </summary>
    public static RandomizerSettings Settings => _settings ??= new();

    /// <summary>
    /// Gets the flag, that indicates if this is a rando file.
    /// </summary>
    public static bool PlayingRandomizer
    {
        get
        {
            if (ModHooks.GetMod("Randomizer 4", true) is not Mod)
                return false;
            else
                return RandoFile;
        }
    }

    /// <summary>
    /// Gets the flag, that indicates if this is a rando file. To prevent missing reference exceptions, this is seperated from <see cref="PlayingRandomizer"/>.
    /// </summary>
    private static bool RandoFile => RandomizerMod.RandomizerMod.IsRandoSave;

    #endregion

    #region Methods

    internal static void LoadSettings(LoreMasterGlobalSaveData saveData)
    {
        _settings = saveData.RandoSettings;
    }

    /// <summary>
    /// Attach the lore master to randomizer
    /// </summary>
    public static void AttachToRandomizer()
    {
        RandomizerMenu.AttachMenu();
        RandomizerRequestModifier.ModifyRequest();
        LogicManager.AttachLogic();
        RandomizerMod.RC.RandoController.OnCalculateHash += RandoController_OnCalculateHash;
        SettingsLog.AfterLogSettings += AddLoreMasterSettings;

        if (ModHooks.GetMod("RandoSettingsManager") is Mod)
            HookRandoSettingsManager();

        CondensedSpoilerLogger.AddCategory("Lore items:", () => Settings.Enabled && (Settings.BlackEggTempleCondition != BlackEggTempleCondition.Dreamers
        || Settings.RandomizeElderbugRewards || Settings.DefineRefs), _loreItemList);
    }

    private static void AddLoreMasterSettings(LogArguments args, TextWriter textWriter)
    {
        textWriter.WriteLine("Lore Master settings:");
        using Newtonsoft.Json.JsonTextWriter jsonTextWriter = new(textWriter) { CloseOutput = false, };
        JsonUtil._js.Serialize(jsonTextWriter, Settings);
        textWriter.WriteLine();
    }

    private static void HookRandoSettingsManager()
    {
        RandoSettingsManagerMod.Instance.RegisterConnection(new SimpleSettingsProxy<FullRandoSettings>(LoreMaster.Instance,
        RandomizerMenu.Instance.PasteSettings,
        () => new() { BaseSettings = Settings, Tags = PowerManager.GlobalPowerStates.Values.ToList() }));
    }

    private static int RandoController_OnCalculateHash(RandomizerMod.RC.RandoController controller, int original)
    {
        if (!Settings.Enabled)
            if (Settings.PowerBehaviour != LoreSetOption.Default || Settings.BlackEggTempleCondition != BlackEggTempleCondition.Dreamers
                || Settings.ForceCompassForTreasure || Settings.TravellerOrder != TravelOrder.Vanilla)
                return 72767 + PowerManager.GetAllPowers().Count() + ((int)Settings.PowerBehaviour * 120 +
                    (int)Settings.BlackEggTempleCondition * Settings.NeededLore + (int)Settings.TravellerOrder * 520);
        return 0;
    }

    internal static bool RandoTracker(Area area, out string areaLore)
    {
        areaLore = null;
        if (!PlayingRandomizer || GreaterMindPower.NormalTracker)
            return false;
        int maxPowerAmount = 0;
        int collectedPowerAmount = 0;

        foreach (AbstractItem item in Ref.Settings.GetItems())
        {
            if (item is not PowerLoreItem)
                continue;
            string areaName = item.RandoLocation()?.LocationDef?.MapArea ?? "UNKNOWN";
            if (string.Equals(areaName, _randoAreaNames[area]))
            {
                maxPowerAmount++;
                if (item.IsObtained())
                    collectedPowerAmount++;
            }
        }

        areaLore = $"{collectedPowerAmount}/{maxPowerAmount}";
        return true;
    }

    internal static void ApplyRando()
    {
        LoreManager.Instance.CanRead = !Settings.CursedReading || !Settings.Enabled;
        LoreManager.Instance.CanListen = !Settings.CursedListening || !Settings.Enabled;
        SettingManager.Instance.NeededLore = Settings.Enabled ? Settings.NeededLore : 0;
        SettingManager.Instance.EndCondition = Settings.Enabled ? Settings.BlackEggTempleCondition : BlackEggTempleCondition.Dreamers;
    }

    internal static (bool, bool) CheckSpecialLore()
        => (RandomizerMod.RandomizerMod.RS.GenerationSettings.NoveltySettings.RandomizeFocus, RandomizerMod.RandomizerMod.RS.GenerationSettings.PoolSettings.Dreamers);

    /// <summary>
    /// Check if the player can consume a rancid egg (for rando logic purposes)
    /// </summary>
    internal static bool CanConsumeEgg()
    {
        if (!RandomizerMod.RandomizerMod.RS.GenerationSettings.NoveltySettings.EggShop)
            return true;
        if (Ref.Settings.Placements.ContainsKey(LocationNames.Egg_Shop))
        {
            AbstractPlacement eggShop = Ref.Settings.Placements[LocationNames.Egg_Shop];
            return eggShop.Items.All(x => x.IsObtained());
        }
        else
            LoreMaster.Instance.LogWarn("Couldn't find egg shop.");
        return false;
    }

    #endregion
}
