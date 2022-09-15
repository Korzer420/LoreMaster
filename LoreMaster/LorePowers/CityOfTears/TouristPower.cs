using LoreMaster.Enums;

namespace LoreMaster.LorePowers.CityOfTears;

public class TouristPower : Power
{
    #region Constructors

    public TouristPower() : base("Tourist", Area.CityOfTears) { }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the flag that indicates, if the fast travel can be taken.
    /// <para/> This is used to increase performance (Could also be done by searching through the active powers).
    /// </summary>
    public static bool Inspected { get; set; }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Enable() => Inspected = true;

    /// <inheritdoc/>
    protected override void Disable() => Inspected = false;

    #endregion
}
