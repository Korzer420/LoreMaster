using LoreMaster.Enums;
using Modding;

namespace LoreMaster.LorePowers;

public class HazardPower : Power
{
    #region Constructors

    public HazardPower() : base("Caring shell", Area.Dirtmouth)
    {
        CustomText = "Dear Diary, today I did get lost again. Somehow I ended up in an area with many shroom and evil looking mantis guys." +
            " After wandering around a bit, I found myself at the top of a spike gauntlet, how did I even get there without noticing all the spikes? Seems like at least my shell cares about me. I was SOOOO scared O-O " +
            "Then while I cried, why all this bad things happened to me, everything faded to orange. But then, at the lowest point of my life, a mighty knight appeared and saved me. Someone with this strength is the first " +
            "person to acknowledge me after such a long time. In just that moment, all my sadness just disappeared. I'm the happiest girl in the world. UwU";
        Hint = "No hazard shall cast harm onto you... UwU";
        Description = "Enviroment Hazards (like spikes) don't deal damage to you anymore.";
    } 

    #endregion

    #region Event Handler

    /// <summary>
    /// Event handler when the player takes damage.
    /// </summary>
    /// <param name="hazardType"></param>
    /// <param name="damageAmount"></param>
    /// <returns></returns>
    private int ModHooks_TakeDamageHook(int hazardType, int damageAmount)
    {
        if (hazardType > 1 && hazardType < 5)
            damageAmount = 0;
        return damageAmount;
    }

    #endregion

    #region Protected Methods

    protected override void Enable()
        => ModHooks.AfterTakeDamageHook += ModHooks_TakeDamageHook;

    protected override void Disable()
    => ModHooks.AfterTakeDamageHook -= ModHooks_TakeDamageHook; 

    #endregion
}
