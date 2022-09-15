using LoreMaster.Enums;
using Modding;

namespace LoreMaster.LorePowers.FungalWastes;

public class PaleLuckPower : Power
{
    #region Constructors

    public PaleLuckPower() : base("Pale Luck", Area.FungalWastes) { }

    #endregion

    #region Event Handler

    /// <summary>
    /// Event handler for taking damage.
    /// </summary>
    private int ModHooks_TakeDamageHook(int hazardType, int damage)
    {
        if (damage <= 0)
            return damage;
        float chance = 2f;

        // Chance increases with king's brand and kingssoul
        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.hasKingsBrand)))
            chance += 4f;
        if (PlayerData.instance.GetBool("equippedCharm_36"))
            chance += 4f;
        chance += PlayerData.instance.GetInt(nameof(PlayerData.instance.trinket3));

        int rolledValue = LoreMaster.Instance.Generator.Next(1, 101);
        if (rolledValue <= chance)
        {
            if (PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) < PlayerData.instance.GetInt(nameof(PlayerData.instance.maxHealth)))
                HeroController.instance.AddHealth(1);
            damage = 0;
        }
        return damage;
    }

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void Enable() => ModHooks.AfterTakeDamageHook += ModHooks_TakeDamageHook;

    /// <inheritdoc/>
    protected override void Disable() => ModHooks.AfterTakeDamageHook -= ModHooks_TakeDamageHook;


    #endregion
}
