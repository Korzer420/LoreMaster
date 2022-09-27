using LoreMaster.Enums;
using LoreMaster.LorePowers;
using LoreMaster.Manager;
using MenuChanger;
using MenuChanger.MenuElements;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LoreMaster.Settings;

internal class ExtraSettings
{
    #region Constructors

    public ExtraSettings()
    {
        foreach (Power power in PowerManager.GetAllPowers())
            PowerTags.Add(power.PowerName, power.DefaultTag);
    }

    #endregion

    #region Properties

    public GameMode GameMode { get; set; } = GameMode.Extra;

    public bool NightmareMode { get; set; }

    public BlackEggTempleCondition EndCondition { get; set; }

    public int NeededLore { get; set; }

    public Dictionary<string, PowerTag> PowerTags { get; set; } = new();

    public bool SteelSoul { get; set; }

    #endregion
}

