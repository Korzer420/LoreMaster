using LoreMaster.Enums;
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoreMaster.Manager;

/// <summary>
/// Manager for handling the powers.
/// </summary>
internal static class PowerManager
{
    #region Members

    private static Dictionary<string, Power> _powerList = new()
    {
        // Ancient Basin
        {"ABYSS_TUT_TAB_01", new WeDontTalkAboutShadePower() },
        // City of Tears
        {"RUIN_TAB_01", new HotStreakPower() },
        {"FOUNTAIN_PLAQUE_DESC", new TouristPower() {DefaultTag = PowerTag.Global } },
        {"RUINS_MARISSA_POSTER", new MarissasAudiencePower() },
        {"MAGE_COMP_01", new SoulExtractEfficiencyPower() },
        {"MAGE_COMP_02", new OverwhelmingPowerPower() },
        {"MAGE_COMP_03", new PureSpiritPower() },
        {"LURIAN_JOURNAL", new EyeOfTheWatcherPower() },
        {"EMILITIA", new HappyFatePower() },
        {"MARISSA", new BlessingOfTheButterflyPower() },
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
        {"WILLOH", new BagOfMushroomsPower() },
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
        {"GRASSHOPPER", new GrassBombardementPower() },
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

    #endregion

    #region Properties

    public static List<Power> ActivePowers { get; set; } = new();

    #endregion

    #region Methods

    #region Power Information
    
    /// <summary>
    /// Check if the key is binded to a power and possibly add it.
    /// </summary>
    /// <param name="key">The ingame language key that is used. For NPC this mod uses just the names of the npc</param>
    /// <param name="power">The power that matches the key, if no match has been found, this will be null.</param>
    /// <param name="collectIfPossible">If <see langword="true"/>, the power will automatically be added if it isn't in <see cref="ActivePowers"/>.</param>
    /// <returns>True if a matching power was found</returns>
    public static bool GetPowerByKey(string key, out Power power, bool collectIfPossible = true)
    {
        if (_powerList.TryGetValue(key?.ToUpper(), out power))
            try
            {
                if (collectIfPossible && !ActivePowers.Contains(power))
                {
                    if (power is EyeOfTheWatcherPower watcherPower)
                        watcherPower.EyeActive = true;
                    power.EnablePower();
                    ActivePowers.Add(power);
                    UpdateTracker(SettingManager.Instance.CurrentArea);
                    LorePage.UpdateLorePage();
                }
                return true;
            }
            catch (Exception exception)
            {
                LoreMaster.Instance.LogError(exception.Message);
            }
        return false;
    }

    public static bool GetPowerByName(string name, out Power power, bool collectIfPossible = true)
    {
        if (_powerList.Values.FirstOrDefault(x => string.Equals(name, x.PowerName, StringComparison.InvariantCultureIgnoreCase)) is Power foundPower)
            try
            {
                power = foundPower;
                if (collectIfPossible && !ActivePowers.Contains(foundPower))
                {
                    if (power is EyeOfTheWatcherPower watcherPower)
                        watcherPower.EyeActive = true;
                    power.EnablePower();
                    ActivePowers.Add(power);
                    UpdateTracker(SettingManager.Instance.CurrentArea);
                    LorePage.UpdateLorePage();
                }
                return true;
            }
            catch (Exception exception)
            {
                LoreMaster.Instance.LogError(exception.Message);
            }
        power = null;
        return false;
    }

    internal static IEnumerable<Power> GetAllPowers() => _powerList.Values;

    public static bool HasObtainedPower(string key, bool onlyActive = true)
    {
        if (_powerList.TryGetValue(key, out Power power))
            return !onlyActive || power.Active;
        return false;
    }

    internal static void ResetPowers()
    {
        // Reset tags to default.
        foreach (string key in _powerList.Keys)
            _powerList[key].Tag = _powerList[key].DefaultTag;
        // Unsure if this is needed, but just in case.
        ActivePowers.Clear();
    }

    internal static void DisableAllPowers()
    {
        foreach (Power power in ActivePowers)
            power.DisablePower(true);
    } 

    internal static void LoadPowers(LoreMasterLocalSaveData saveData)
    {
        ActivePowers.Clear();
        foreach (string key in saveData.Tags.Keys)
            _powerList[key].Tag = saveData.Tags[key];

        foreach (string key in saveData.AcquiredPowersKey)
            GetPowerByName(key, out Power pow);
    }

    internal static void SavePowers(ref LoreMasterLocalSaveData saveData)
    {
        foreach (string key in _powerList.Keys)
        { 
            saveData.Tags.Add(key, _powerList[key].Tag);
            if (ActivePowers.Contains(_powerList[key]))
                saveData.AcquiredPowersKey.Add(key);
        }
    }

    internal static void AddPower(string key, Power power) => _powerList.Add(key, power);
    
    #endregion

    /// <summary>
    /// Checks through all needed powers to determine if the powers should be granted globally.
    /// </summary>
    /// <param name="toCheck"></param>
    /// <returns></returns>
    internal static bool IsAreaGlobal(Area toCheck)
    {
        List<Power> neededAreaPowers = _powerList.Values.Where(x => x.Location == toCheck && (x.Tag == PowerTag.Local || x.Tag == PowerTag.Disable || x.Tag == PowerTag.Global)).ToList();
        foreach (Power neededPower in neededAreaPowers)
            if (!ActivePowers.Contains(neededPower))
                return false;
        return true;
    }

    public static void ExecuteSceneActions()
    {
        foreach (Power power in ActivePowers)
            if (power.Active && power.SceneAction != null)
                try
                {
                    power.SceneAction.Invoke();
                }
                catch (Exception exception)
                {
                    LoreMaster.Instance.LogError("Error while executing scene action for " + power.PowerName + ": " + exception.Message + "StackTrace: " + exception.StackTrace);
                }
    }

    internal static void CalculatePowerStates(Area newArea)
    {
            try
            {
                // Activate all local abilities
                foreach (Power power in ActivePowers.Where(x => x.Location == newArea))
                    if (power.Tag == PowerTag.Exclude || power.Tag == PowerTag.Local)
                        power.EnablePower();

                // Disable all local abilities from all other zone (this has to be done that way for randomizer compability)
                foreach (Area area in ((Area[])Enum.GetValues(typeof(Area))).Skip(1))
                    if (area != newArea && !IsAreaGlobal(area))
                        foreach (Power power in ActivePowers.Where(x => x.Location == area))
                            if (power.Tag == PowerTag.Local || power.Tag == PowerTag.Exclude)
                                power.DisablePower();
            }
            catch (Exception exception)
            {
                LoreMaster.Instance.LogError("An error occured in the area change: " + exception.Message);
            }
            UpdateTracker(newArea);
    }

    internal static void FirstPowerInitialization()
    {
        // Enables the powers beforehand. This has to be done because otherwise the effects will only stay permanent once the player enters the area.
        List<Power> toActivate = new();
        List<Power> allPowers = new(ActivePowers);
        toActivate.AddRange(allPowers.Where(x => x.Tag == PowerTag.Global));

        foreach (Area area in (Area[])Enum.GetValues(typeof(Area)))
            if (IsAreaGlobal(area))
                toActivate.AddRange(allPowers.Where(x => x.Tag != PowerTag.Global && x.Location == area));

        foreach (Power power in toActivate)
            power.EnablePower();
    }

    /// <summary>
    /// Updates the lore tracker.
    /// </summary>
    public static void UpdateTracker(Area areaToUpdate)
    {
        try
        {
            if (ActivePowers.Contains(_powerList["COMPLETION_RATE_UNLOCKED"]))
            {
                GreaterMindPower logPower = (GreaterMindPower)_powerList["COMPLETION_RATE_UNLOCKED"];
                if (logPower.Active)
                    logPower.UpdateLoreCounter(ActivePowers, _powerList.Values, areaToUpdate, IsAreaGlobal(areaToUpdate));
            }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError(exception.Message);
        }
    }

    #endregion
}
