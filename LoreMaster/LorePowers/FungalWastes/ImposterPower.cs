using LoreMaster.Enums;

namespace LoreMaster.LorePowers.FungalWastes;

public class ImposterPower : Power
{
    #region Constructors

    public ImposterPower() : base("Imposter", Area.FungalWastes)
    {
        CustomText = "Pity those bugs. Their society shattered to pieces. While our kind should survive it all, we fear that they are imposter among us, which causes the blue illness upon our colony.";
    }

    #endregion

    #region Event Handler

    /// <summary>
    /// Event handler when the player is healing.
    /// </summary>
    private void ExtraHeal(On.HeroController.orig_AddHealth orig, HeroController self, int amount)
    {
        if (PlayerData.instance.GetBool("equippedCharm_17") && LoreMaster.Instance.Generator.Next(0, 5) == 0 && PlayerData.instance.healthBlue < 5)
            EventRegister.SendEvent("ADD BLUE HEALTH");

        orig(self, amount);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable() => On.HeroController.AddHealth += ExtraHeal;

    /// <inheritdoc/>
    protected override void Disable() => On.HeroController.AddHealth -= ExtraHeal;

    #endregion
}
