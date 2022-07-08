using LoreMaster.Enums;

namespace LoreMaster.LorePowers.FungalWastes;

public class ImposterPower : Power
{
    #region Constructors

    public ImposterPower() : base("Imposter", Area.FungalWastes)
    {
        CustomText = "Pity those bugs. Their society shattered to pieces. While our kind should survive it all, we fear that they are imposter among us, which causes the blue illness upon our colony.";
        Hint = "While being part of the shrooms, sometime you focus treacherous energys... AMOGUS";
        Description = "While wearing spore shrooms, focus has a 20% chance to add a lifeblood (doesn't work if you have 3 or more lifeblood).";
    }

    #endregion

    #region Event Handler

    /// <summary>
    /// Event handler when the player is healing.
    /// </summary>
    private void ExtraHeal(On.HeroController.orig_AddHealth orig, HeroController self, int amount)
    {
        if (PlayerData.instance.GetBool("equippedCharm_17") && LoreMaster.Instance.Generator.Next(0, 5) == 0 && PlayerData.instance.healthBlue < 3)
            EventRegister.SendEvent("ADD BLUE HEALTH");

        orig(self, amount);
    }

    #endregion

    #region Protected Methods

    protected override void Enable() => On.HeroController.AddHealth += ExtraHeal;
    
    protected override void Disable() => On.HeroController.AddHealth -= ExtraHeal;
    
    #endregion
}
