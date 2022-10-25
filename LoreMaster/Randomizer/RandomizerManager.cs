using ItemChanger;
using ItemChanger.Internal;
using LoreMaster.Enums;
using LoreMaster.ItemChangerData.Items;
using LoreMaster.LorePowers;
using LoreMaster.LorePowers.CityOfTears;
using LoreMaster.LorePowers.RestingGrounds;
using LoreMaster.Manager;
using LoreMaster.SaveManagement;
using LoreMaster.Settings;
using Modding;
using RandomizerMod.Extensions;
using System.Collections.Generic;
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
    }

    private static int RandoController_OnCalculateHash(RandomizerMod.RC.RandoController controller, int original)
    {
        if (Settings.PowerBehaviour != LoreSetOption.Default || Settings.BlackEggTempleCondition != BlackEggTempleCondition.Dreamers
            || Settings.ForceCompassForTreasure || Settings.TravellerOrder != TravelOrder.Vanilla)
            return 72767 + PowerManager.GetAllPowers().Count() + ((int)Settings.PowerBehaviour * 120 + 
                (int)Settings.BlackEggTempleCondition * Settings.NeededLore + (int)Settings.TravellerOrder * 520);
        return original;
    }

    internal static bool RandoTracker(Area area, out string areaLore)
    {
        areaLore = null;
        if (!PlayingRandomizer)
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
        LoreManager.Instance.CanRead = !Settings.CursedReading;
        LoreManager.Instance.CanListen = !Settings.CursedListening;
        SettingManager.Instance.NeededLore = Settings.NeededLore;
        SettingManager.Instance.EndCondition = Settings.BlackEggTempleCondition;
    }

    internal static (bool,bool) CheckSpecialLore() 
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
