using LoreMaster.Enums;
using UnityEngine;

namespace LoreMaster.LorePowers.Waterways;

public class RelentlessSwarmPower : Power
{
    #region Constructors

    public RelentlessSwarmPower() : base("Relentless Swarm", Area.WaterWays)
    {
        Hint = "Flukes rip the soul out of there victims.";
        Description = "Hits with flukes restore 2 soul, or 5 if the enemy died.";
    }

    #endregion

    #region Event Handler

    private void SpellFluke_DoDamage(On.SpellFluke.orig_DoDamage orig, SpellFluke self, GameObject obj, int upwardRecursionAmount, bool burst)
    {
        orig(self, obj, upwardRecursionAmount, burst);
        if (obj.GetComponent<HealthManager>().isDead)
            HeroController.instance.AddMPCharge(5);
        else
            HeroController.instance.AddMPCharge(2);
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Enable() => On.SpellFluke.DoDamage += SpellFluke_DoDamage;

    /// <inheritdoc/>
    protected override void Disable() => On.SpellFluke.DoDamage -= SpellFluke_DoDamage;

    #endregion
}