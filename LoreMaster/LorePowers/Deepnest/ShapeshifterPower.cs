using LoreMaster.Enums;

namespace LoreMaster.LorePowers.Deepnest;

internal class ShapeshifterPower : Power
{
	#region Constructor

	public ShapeshifterPower() : base("Shapeshifter", Area.Deepnest) { }

    #endregion

    #region Properties

    public override PowerRank Rank => PowerRank.Medium;

    #endregion
}
