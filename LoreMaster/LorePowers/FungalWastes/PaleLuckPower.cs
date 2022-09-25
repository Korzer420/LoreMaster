using LoreMaster.Enums;
using Modding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes;

public class PaleLuckPower : Power
{
    #region Members

    private List<HealthManager> _recentEnemies = new();

    #endregion

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

    private void HealthManager_Hit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
    {
        if (LoreMaster.Instance.Generator.Next(1, 21) == 1)
        {
            hitInstance.DamageDealt = 0;
            if (!_recentEnemies.Contains(self))
            { 
                self.hp += PlayerData.instance.GetInt("nailDamage");
                LoreMaster.Instance.Handler.StartCoroutine(PreventDoubleHeal(self));
            }
        }
        orig(self, hitInstance);
    }

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void Enable() => ModHooks.AfterTakeDamageHook += ModHooks_TakeDamageHook;

    /// <inheritdoc/>
    protected override void Disable() => ModHooks.AfterTakeDamageHook -= ModHooks_TakeDamageHook;

    /// <inheritdoc/>
    protected override void TwistEnable() => On.HealthManager.Hit += HealthManager_Hit;

    /// <inheritdoc/>
    protected override void TwistDisable() => On.HealthManager.Hit -= HealthManager_Hit;

    #endregion

    #region Methods

    private IEnumerator PreventDoubleHeal(HealthManager healthManager)
    {
        _recentEnemies.Add(healthManager);
        yield return new WaitForSeconds(.2f);
        _recentEnemies.Remove(healthManager);
    }

    #endregion
}
