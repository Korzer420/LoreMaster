using LoreMaster.Enums;

namespace LoreMaster.LorePowers.CityOfTears;

public class TouristPower : Power
{
    #region Constructors

    public TouristPower() : base("Tourist", Area.CityOfTears)
    {
        Hint = "Want to see the incredible black egg temple? Talk to the firefly to the right. Drinks are not included in the price. Also offers back travel.";
        Description = "You can talk to the firefly to the right to teleport to black egg temple for 50 geo, or back to this room from the temple.";
    }

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
