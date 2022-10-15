using ItemChanger;
using ItemChanger.Internal;
using LoreMaster.Enums;
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
        if (!RandomizerMod.RandomizerMod.IsRandoSave || !RandomizerMod.RandomizerMod.RS.GenerationSettings.PoolSettings.LoreTablets)
            return false;
        int maxPowerAmount = 0;
        int collectedPowerAmount = 0;

        if (!Settings.RandomizeNpc)
        {
            Power power = null;
            switch (area)
            {
                case Area.Dirtmouth:
                    maxPowerAmount += 2;
                    if (PowerManager.GetPowerByKey("GRAVEDIGGER", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("BRETTA", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.Crossroads:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("MYLA", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.Cliffs:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("JONI", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.FungalWastes:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("WILLOH", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.CityOfTears:
                    maxPowerAmount += 3;
                    if (PowerManager.GetPowerByKey("POGGY", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("MARISSA", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("EMILITIA", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.WaterWays:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("FLUKE_HERMIT", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.Deepnest:
                    maxPowerAmount += 2;
                    if (PowerManager.GetPowerByKey("MIDWIFE", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("MASKMAKER", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.QueensGarden:
                    maxPowerAmount += 3;
                    if (PowerManager.GetPowerByKey("MOSSPROPHET", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("QUEEN", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("GRASSHOPPER", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.KingdomsEdge:
                    maxPowerAmount += 2;
                    if (PowerManager.GetPowerByKey("HIVEQUEEN", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    if (PowerManager.GetPowerByKey("BARDOON", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
                case Area.Peaks:
                    maxPowerAmount++;
                    if (PowerManager.GetPowerByKey("QUIRREL", out power, false) && PowerManager.ObtainedPowers.Contains(power))
                        collectedPowerAmount++;
                    break;
            }
        }

        foreach (AbstractItem item in Ref.Settings.GetItems())
        {
            string areaName = item.RandoLocation()?.LocationDef?.MapArea ?? "UNKNOWN";
            if (string.Equals(areaName, _randoAreaNames[area]) && (item.name.Contains("Lore_Tablet-") || item.name.Contains("_Inspect")))
            {
                maxPowerAmount++;
                if (item.IsObtained())
                    collectedPowerAmount++;
            }
        }
        // Since neither the fountain nor record bela are randomizeable, we add them manually to the tracker
        if (area == Area.CityOfTears)
        {
            maxPowerAmount += 2;
            if (PowerManager.ObtainedPowers.Any(x => x is TouristPower))
                collectedPowerAmount++;
            if (PowerManager.ObtainedPowers.Any(x => x is OverwhelmingPowerPower))
                collectedPowerAmount++;
        }
        // Same for the dreamer tablet.
        else if (area == Area.RestingGrounds)
        {
            maxPowerAmount++;
            if (PowerManager.ObtainedPowers.Any(x => x is DreamBlessingPower))
                collectedPowerAmount++;
        }

        areaLore = $"{collectedPowerAmount}/{maxPowerAmount}";
        return true;
    }

    internal static void ApplyRando()
    {
        LoreManager.Instance.CanRead = !Settings.CursedReading;
        LoreManager.Instance.CanListen = !Settings.CursedListening;
    }

    #endregion
}
