using LoreMaster.Enums;

namespace LoreMaster.LorePowers.CityOfTears;

internal class CharmingTrapPower : Power
{
    #region Constructors

    public CharmingTrapPower() : base("Charming Trap", Area.CityOfTears)
    {
        Hint = "[Not Implemented] Sometimes your fireball resonate a charming aura which lures the first enemy hit, causing them to not harm you directly.";
        Description = "[Not Implemented] Every 20-60 seconds your next fireball charms the first enemy hit for 5 seconds, causing them to not deal contact damage to you.";
    }

    #endregion
}
