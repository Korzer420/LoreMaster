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

    public CursedLore UseCursedLore { get; set; }

    public int MinCursedLore { get; set; } = 1;

    public int MaxCursedLore { get; set; } = 10;

    #endregion
}

