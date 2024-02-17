using ItemChanger;
using ItemChanger.Modules;
using LoreCore.Items;
using LoreMaster.Enums;
using LoreMaster.LorePowers;
using LoreMaster.LorePowers.QueensGarden;
using LoreMaster.Manager;
using Modding;
using System.Collections.Generic;

namespace LoreMaster.ItemChangerData;

/// <summary>
/// Manages the acquisition of powers.
/// </summary>
public class LorePowerModule : Module
{
    #region Properties

    public List<string> AcquiredPowers { get; set; } = new();

    public int MajorGlyphSlots { get; set; } = 0;

    public int MinorGlyphSlots { get; set; } = 0;

    public int SmallGlyphSlots { get; set; } = 0;

    public int MysticalScrolls { get; set; } = 0;

    public int CleansingScrolls { get; set; } = 0;

    public bool HasStagEgg { get; set; }

    #endregion

    #region Eventhandler

    private string PowerLoreItem_AcquirePowerItem(string key, string originalText)
    {
        if (PowerManager.GetPowerByKey(key) is Power power)
        {
            AcquiredPowers.Add(power.PowerName);
            if (LoreManager.GlobalSaveData.EnableCustomText && !string.IsNullOrEmpty(power.CustomText))
                originalText = power.CustomText;
            originalText += $"<page>[{power.PowerName}]<br>";
            if (LoreManager.GlobalSaveData.ShowHint)
                originalText += power.Hint;
            else
                originalText += power.Description;
            LorePage.UpdateLorePage();
        }
        return originalText;
    }

    private void ModifyThorns(PlayMakerFSM fsm) => PowerManager.GetPower<QueenThornsPower>().ModifyThorns(fsm);

    private int ModHooks_SetPlayerIntHook(string name, int orig)
    {
        if (name == "majorGlyphSlots")
            MajorGlyphSlots += orig;
        else if (name == "minorGlyphSlots")
            MinorGlyphSlots += orig;
        else if (name == "smallGlyphSlots")
            SmallGlyphSlots += orig;
        else if (name == "cleansingScrolls")
            CleansingScrolls += orig;
        else if (name == "mysticScrolls")
            MysticalScrolls += orig;
        return orig;
    }

    private bool ModHooks_SetPlayerBoolHook(string name, bool orig)
    {
        if (name == "hasStagEgg")
            HasStagEgg = orig;
        return orig;
    }

    #endregion

    #region Methods

    public override void Initialize()
    {
        PowerLoreItem.AcquirePowerItem += PowerLoreItem_AcquirePowerItem;
        Events.AddFsmEdit(new("Thorn Counter"), ModifyThorns);
        ModHooks.SetPlayerIntHook += ModHooks_SetPlayerIntHook;
        ModHooks.SetPlayerBoolHook += ModHooks_SetPlayerBoolHook;
        foreach (Power power in PowerManager.GetAllPowers())
            if (power.Rank != PowerRank.Permanent)
                AcquiredPowers.Add(power.PowerName);
    }

    public override void Unload()
    {
        PowerLoreItem.AcquirePowerItem -= PowerLoreItem_AcquirePowerItem;
        Events.RemoveFsmEdit(new("Thorn Counter"), ModifyThorns);
        ModHooks.SetPlayerIntHook -= ModHooks_SetPlayerIntHook;
        ModHooks.SetPlayerBoolHook -= ModHooks_SetPlayerBoolHook;
    }

    public bool IsIndexAvailable((int, PowerRank) powerIndex)
    {
        return powerIndex.Item2 switch
        {
            PowerRank.Permanent => true,
            PowerRank.Lower => SmallGlyphSlots >= powerIndex.Item1 + 1,
            PowerRank.Medium => MinorGlyphSlots >= powerIndex.Item1 + 1,
            _ => MajorGlyphSlots >= powerIndex.Item1 + 1
        };
    }

    #endregion
}
