using LoreMaster.Enums;
using Modding;

namespace LoreMaster.LorePowers.Peaks;

internal class TechniqueOfTheAncientsPower : Power
{
    private int _level = 0;
    private int _hits = 0;

    public TechniqueOfTheAncientsPower() : base("Technique of the Ancients", Area.Peaks)
    {
        CustomText = "Why did you leave your duty as a smith? We gave you the most powerful stones we could find. You betrayed us. You lied to us." +
            " Do you still craft these into your blades for a different kind? This power in the wrong hands will cause huge destruction. You will pay for that.";
    }

    #region Event handler

    private int ModHooks_AfterTakeDamageHook(int hazardType, int damageAmount)
    {
        if (damageAmount > 0)
        {
            _hits = 0;
            _level = 0;
            PlayerData.instance.SetInt("ArtifactPower", _level);
        }
        return damageAmount;
    }

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        orig(self, hitInstance);
        if (hitInstance.AttackType == AttackTypes.Nail && hitInstance.DamageDealt > 0 && _level < 4)
        {
            _hits++;
            switch (_hits)
            {
                case 15 when _level == 0:
                case 25 when _level == 1:
                case 35 when _level == 2:
                case 50 when _level == 3:
                    _hits = 0;
                    _level++;
                    PlayerData.instance.SetInt("ArtifactPower", _level);
                    break;
            }
        }
    }

    #endregion

    #region Control

    protected override void Enable()
    {
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        ModHooks.AfterTakeDamageHook += ModHooks_AfterTakeDamageHook;
    }

    protected override void Disable()
    {
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
        ModHooks.AfterTakeDamageHook -= ModHooks_AfterTakeDamageHook;
    } 

    #endregion
}
