using ItemChanger;
using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.ItemChangerData;
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoreMaster.Manager;

/// <summary>
/// Manager for handling the powers.
/// </summary>
public static class PowerManager
{
    #region Members

    private static List<Power> _powerList =
    [
        // Dirtmouth/King's Pass
        new WellFocusedPower(),
        new ScrewTheRulesPower(),
        new TrueFormPower(),
        new CaringShellPower(),
        new RequiemPower(),
        // Crossroads
        new ReluctantPilgrimPower(),
        new GreaterMindPower(),
        new DiamondDashPower(),
        new BestMenderInTheWorldPower(),
        // Greenpath
        new TouchGrassPower(),
        new GiftOfUnnPower(),
        new MindblastOfUnnPower(),
        new CamouflagePower(),
        new ReturnToUnnPower(),
        new GraspOfLifePower(),
        // Fungal Wastes
        new OneOfUsPower(),
        new PaleLuckPower(),
        new ImposterPower(),
        new UnitedWeStandPower(),
        new MantisStylePower(),
        new EternalValorPower(),
        new GloryOfTheWealthPower(),
        new BagOfMushroomsPower(),
        // City of Tears
        new HotStreakPower(),
        new TouristPower(),
        new MarissasAudiencePower(),
        new SoulExtractEfficiencyPower(),
        new OverwhelmingPowerPower(),
        new PureSpiritPower(),
        new EyeOfTheWatcherPower(),
        new HappyFatePower(),
        new BlessingOfTheButterflyPower(),
        new DeliciousMealPower(),
        // Waterways
        new EternalSentinelPower(),
        new RelentlessSwarmPower(),
        // Crystal Peaks
        new DiamondCorePower(),
        // Resting Grounds
        new DreamBlessingPower(),
        new DramaticEntrancePower(),
        // Howling Cliffs
        new LifebloodOmenPower(),
        new JonisProtectionPower(),
        new StagAdoptionPower(),
        // Fog Canyon
        new FriendOfTheJellyfishPower(),
        new JellyBellyPower(),
        new JellyfishFlowPower(),
        // Ancient Basin
        new WeDontTalkAboutShadePower(),
        // Deepnest
        new MaskOverchargePower(),
        new InfestedPower(),
        new LaughableThreatPower(),
        // Kingdom's Edge
        new WisdomOfTheSagePower(),
        new ConcussiveStrikePower(),
        new YouLikeJazzPower(),
        // Queen's Garden
        new FlowerRingPower(),
        new QueenThornsPower(),
        new FollowTheLightPower(),
        // White Palace
        new ShadowForgedPower(),
        new ShiningBoundPower(),
        new DiminishingCursePower(),
        new SacredShellPower()
    ];

    #endregion

    #region Properties

    public static LorePowerModule Module => ItemChangerMod.Modules?.GetOrAdd<LorePowerModule>();

    #endregion

    #region Methods

    #region Power Information

    /// <summary>
    /// Check for a power by its language key. If another mod adds powers the need to overwrite <see cref="Power.CorrespondingKey"/> in order to work.
    /// </summary>
    /// <param name="languageKey">The ingame language key that is used.</param>
    /// <returns>True if a matching power was found</returns>
    public static Power GetPowerByKey(string languageKey)
    {
        Power power = _powerList.FirstOrDefault(x => x.CorrespondingKey == languageKey);
        return power;
    }

    /// <summary>
    /// Checks for a power by its name.
    /// </summary>
    /// <param name="name">The name of the power.</param>
    public static Power GetPowerByName(string name)
    {
        Power power = null;
        if (string.IsNullOrEmpty(name))
            return null;
        power = _powerList.FirstOrDefault(x => string.Equals(name, x.PowerName, StringComparison.CurrentCultureIgnoreCase));
        // Second attempt with removed white spaces.
        power ??= _powerList.FirstOrDefault(x => string.Equals(name, x.PowerName.Replace(" ", ""), StringComparison.CurrentCultureIgnoreCase));
        return power;
    }

    /// <summary>
    /// Gets a power by its type.
    /// </summary>
    public static T GetPower<T>() where T : Power => _powerList.FirstOrDefault(x => x is T) as T;

    /// <summary>
    /// Returns a copy of all powers.
    /// </summary>
    /// <returns></returns>
    public static List<Power> GetAllPowers() => _powerList.ToList();

    /// <summary>
    /// Check if a power has been obtained already.
    /// </summary>
    /// <param name="onlyActive">If <see langword="true"/>, this will only return <see langword="true"/> if the power is equipped.</param>
    public static bool HasObtainedPower<T>(bool onlyActive = true) where T : Power
    {
        Power power = _powerList.FirstOrDefault(x => x is T);
        if (power == null)
            return false;
        return !onlyActive || (LoreManager.Module?.AcquiredPowers?.Contains(power.PowerName) ?? false);
    }

    /// <summary>
    /// Disables all powers. WHO WOULD'VE THOUGHT??? :O
    /// </summary>
    internal static void DisableAllPowers() => Module.CanPowersActivate = false;

    /// <summary>
    /// Adds a power to the internal list.
    /// </summary>
    /// <param name="power">The power to add. Only one power per type can be added.</param>
    public static void AddPower(Power power)
    {
        if (!_powerList.Any(x => x.GetType() == power.GetType()))
            _powerList.Add(power);
    }

    /// <summary>
    /// Returns all active powers.
    /// </summary>
    public static IEnumerable<Power> GetAllActivePowers()
    {
        IEnumerable<string> allActivePowerNames = Module.MajorPowers.Concat(Module.MinorPowers).Concat(Module.SmallPowers).Concat(Module.PermanentPowers);
        return _powerList.Where(x => allActivePowerNames.Contains(x.PowerName));
    }

    /// <summary>
    /// Get all powers from a certain rank.
    /// </summary>
    public static IEnumerable<Power> GetPowersByRank(PowerRank powerRank, bool onlyObtained = true)
        => _powerList.Where(x => x.Rank == powerRank && (!onlyObtained || LoreManager.Module.AcquiredPowers.Contains(x.PowerName)));

    #endregion

    /// <summary>
    /// Let all powers execute their behaviour for entering a new room.
    /// </summary>
    internal static void ExecuteSceneActions()
    {
        foreach (Power power in GetAllActivePowers())
            if (power.SceneAction != null)
                try
                {
                    power.SceneAction.Invoke();
                }
                catch (Exception exception)
                {
                    LoreMaster.Instance.LogError("Error while executing scene action for " + power.PowerName + ": " + exception.Message + "StackTrace: " + exception.StackTrace);
                }
    }

    internal static Power GetPowerInSlot((int, PowerRank) powerIndex)
    {
        try
        {
            string powerName = powerIndex.Item2 switch
            {
                PowerRank.Permanent => Module.PermanentPowers[powerIndex.Item1],
                PowerRank.Lower => Module.SmallPowers[powerIndex.Item1],
                PowerRank.Medium => Module.MinorPowers[powerIndex.Item1],
                _ => Module.MajorPowers[powerIndex.Item1],
            };
            return GetPowerByName(powerName);
        }
        catch (Exception exception)
        {
            LogHelper.Write("An error occured while trying to find power on slot.", exception);
            return null;
        }
    }

    internal static void SwapPower((int, PowerRank) powerIndex, string newPower)
    {
        try
        {
            Power activePower = GetPowerInSlot(powerIndex);
            activePower?.DisablePower();
            switch (powerIndex.Item2)
            {
                case PowerRank.Greater:
                    Module.MajorPowers[powerIndex.Item1] = newPower;
                    break;
                case PowerRank.Medium:
                    Module.MinorPowers[powerIndex.Item1] = newPower;
                    break;
                default:
                    Module.SmallPowers[powerIndex.Item1] = newPower;
                    break;
            }
            if (!string.IsNullOrEmpty(newPower))
            {
                GetPowerByName(newPower).EnablePower();
            }
        }
        catch (Exception exception)
        {
            LogHelper.Write("An error occured while trying to swap powers.", exception);
        }
    }

    #endregion
}
