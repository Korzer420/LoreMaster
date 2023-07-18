using ItemChanger.Modules;
using LoreCore.Items;
using LoreMaster.LorePowers;
using LoreMaster.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.ItemChangerData;

/// <summary>
/// Manages the acquisition of powers.
/// </summary>
internal class LorePowerModule : Module
{
    #region Properties

    public List<string> AcquiredPowers { get; set; } = new();

    #endregion

    #region Eventhandler

    private string PowerLoreItem_AcquirePowerItem(string key, string originalText)
    {
        if (PowerManager.GetPowerByKey(key, out Power power))
        {
            AcquiredPowers.Add(power.PowerName);
            LorePage.UpdateLorePage();
        }
        return originalText;
    }

    #endregion

    #region Methods

    public override void Initialize()
    {
        PowerLoreItem.AcquirePowerItem += PowerLoreItem_AcquirePowerItem;
    }

    public override void Unload()
    {
        PowerLoreItem.AcquirePowerItem -= PowerLoreItem_AcquirePowerItem;
    }

    #endregion
}
