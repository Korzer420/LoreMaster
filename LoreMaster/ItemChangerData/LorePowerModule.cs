﻿using ItemChanger;
using ItemChanger.Modules;
using LoreCore.Items;
using LoreMaster.LorePowers;
using LoreMaster.LorePowers.QueensGarden;
using LoreMaster.Manager;
using System.Collections.Generic;

namespace LoreMaster.ItemChangerData;

/// <summary>
/// Manages the acquisition of powers.
/// </summary>
public class LorePowerModule : Module
{
    #region Properties

    public List<string> AcquiredPowers { get; set; } = new();

    public int AcquiredLore { get; set; } = 0;

    public int MajorGlyphSlots { get; set; } = 0;

    public int MediumGlyphSlots { get; set; } = 0;

    public int SmallGlyphSlots { get; set; } = 0;

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
        AcquiredLore++;
        return originalText;
    }

    private void ModifyThorns(PlayMakerFSM fsm) => PowerManager.GetPower<QueenThornsPower>().ModifyThorns(fsm);

    #endregion

    #region Methods

    public override void Initialize()
    {
        PowerLoreItem.AcquirePowerItem += PowerLoreItem_AcquirePowerItem;
        Events.AddFsmEdit(new("Thorn Counter"), ModifyThorns);
    }

    public override void Unload()
    {
        PowerLoreItem.AcquirePowerItem -= PowerLoreItem_AcquirePowerItem;
        Events.RemoveFsmEdit(new("Thorn Counter"), ModifyThorns);
    }

    #endregion
}
