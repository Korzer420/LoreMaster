using ItemChanger.Modules;
using LoreCore.Items;
using LoreMaster.LorePowers;
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

    #endregion

    #region Eventhandler

    private string PowerLoreItem_AcquirePowerItem(string key, string originalText)
    {
        if (PowerManager.GetPowerByKey(key) is Power power)
        {
            AcquiredPowers.Add(power.PowerName);
            if (LoreManager.UseCustomText && !string.IsNullOrEmpty(power.CustomText))
                originalText = power.CustomText;
            originalText += $"<page>[{power.PowerName}]<br>";
            if (LoreManager.UseHints)
                originalText += power.Hint;
            else
                originalText += power.Description;
            LorePage.UpdateLorePage();
        }
        AcquiredLore++;
        return originalText;
    }

    #endregion

    #region Methods

    public override void Initialize()
        => PowerLoreItem.AcquirePowerItem += PowerLoreItem_AcquirePowerItem;


    public override void Unload()
        => PowerLoreItem.AcquirePowerItem -= PowerLoreItem_AcquirePowerItem;


    #endregion
}
