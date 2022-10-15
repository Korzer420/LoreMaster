using LoreMaster.Enums;
using Modding;
using System;
using UnityEngine;

namespace LoreMaster.LorePowers.Dirtmouth;

public class CaringShellPower : Power
{
    #region Constructors

    public CaringShellPower() : base("Caring shell", Area.Dirtmouth)
    {
        CustomText = "Dear Diary, today I did get lost again. Somehow I ended up in an area with many shroom and evil looking mantis guys." +
            " After wandering around a bit, I found myself at the top of a spike gauntlet, how did I even get there without noticing all the spikes? Seems like at least my shell cares about me. I was SOOOO scared O-O " +
            "Then while I cried, why all this bad things happened to me, everything faded to orange. But then, at the lowest point of my life, a mighty knight appeared and saved me. Someone with this strength is the first " +
            "person to acknowledge me after such a long time. In just that moment, all my sadness just disappeared. I'm the happiest girl in the world. UwU";
    }

    #endregion

    #region Properties

    /// <summary>
    /// Deactivates all hazard respawns.
    /// </summary>
    public override Action SceneAction => () =>
    {
        if (State == PowerState.Twisted)
            foreach (DeactivateInDarknessWithoutLantern hazardRespawn in GameObject.FindObjectsOfType<DeactivateInDarknessWithoutLantern>())
                hazardRespawn.gameObject.SetActive(false);
    };

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

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
           => ModHooks.AfterTakeDamageHook += ModHooks_TakeDamageHook;

    /// <inheritdoc/>
    protected override void Disable()
       => ModHooks.AfterTakeDamageHook -= ModHooks_TakeDamageHook;

    #endregion
}
