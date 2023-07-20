using LoreMaster.Enums;

namespace LoreMaster.Settings;

internal class ExtraSettings
{
    #region Properties

    public GameMode GameMode { get; set; } = GameMode.Extra;

    public bool NightmareMode { get; set; }

    public BlackEggTempleCondition EndCondition { get; set; }

    public int NeededLore { get; set; }

    public bool SteelSoul { get; set; }

    #endregion
}

