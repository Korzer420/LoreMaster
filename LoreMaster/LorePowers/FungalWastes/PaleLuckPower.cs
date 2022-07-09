using LoreMaster.Enums;
using Modding;

namespace LoreMaster.LorePowers.FungalWastes;

public class PaleLuckPower: Power 
{
    #region Constructors

    public PaleLuckPower() : base("Pale Luck", Area.FungalWastes)
    {
        Hint = "When someone casts harm on you, sometimes you are blessed by the higher being instead. Especially if you have some artefacts related to him.";
        Description = "When you would take damage, you have a 1% chance to be healed instead. Increased by 2% for each King's Brand and Kingssoul.";
    }

    #endregion

    #region Event Handler

    /// <summary>
    /// Event handler for taking damage.
    /// </summary>
    private int ModHooks_TakeDamageHook(int hazardType, int damage)
    {
        if (damage <= 0)
            return damage;
        int chance = 1;

        // Chance increases with king's brand and kingssoul
        if (PlayerData.instance.hasKingsBrand)
            chance += 2;
        if (PlayerData.instance.GetBool("equippedCharm_36"))
            chance += 2;

        int rolledValue = LoreMaster.Instance.Generator.Next(1, 101);
        if (rolledValue <= chance)
        {
            if (PlayerData.instance.health < PlayerData.instance.maxHealth)
                HeroController.instance.AddHealth(1);
            damage = 0;
        }
        return damage;
    }

    #endregion

    #region Methods

    protected override void Enable() => ModHooks.AfterTakeDamageHook += ModHooks_TakeDamageHook;
    
    protected override void Disable() => ModHooks.AfterTakeDamageHook -= ModHooks_TakeDamageHook;
    

    #endregion
}
