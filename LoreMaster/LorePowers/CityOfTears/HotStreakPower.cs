using LoreMaster.Enums;
using LoreMaster.UnityComponents;
using Modding;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

public class HotStreakPower : Power
{
    #region Members

    private int _damageStacks = 0;
    private bool _currentlyRunning;
    private bool _hasHitEnemy;

    #endregion

    #region Constructors

    public HotStreakPower() : base("Hot Streak", Area.CityOfTears) { }

    #endregion

    #region Properties

    public override PowerRank Rank => PowerRank.Greater;

    #endregion

    #region Event Handler

    /// <summary>
    /// Event handler when the player slashes with the nail.
    /// </summary>
    private void NailSlash(Collider2D otherCollider, GameObject slash)
    {
        // This event is fired multiple times, therefore we check every instance if an enemy was hit
        if (otherCollider.gameObject.GetComponent<HealthManager>())
            _hasHitEnemy = true;
        // To prevent running multiple coroutines
        if (_currentlyRunning)
            return;
        _currentlyRunning = true;
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(HitCooldown());
    }

    /// <summary>
    /// Event handler, when the game asks for the nail damage.
    /// </summary>
    private int EmpowerNail(string name, int damage)
    {
        if (string.Equals(name, "nailDamage"))
            damage += _damageStacks;
        return damage;
    }

    private void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        orig(self, go, damageSide, damageAmount, hazardType);
        GameObject enemy = (go.GetComponent<HealthManager>() ? go : null) ?? (go.transform.parent?.GetComponent<HealthManager>()
            ? go.transform.parent.gameObject
            : null);
        if (enemy == null || enemy.GetComponent<EnemyBuff>()?.PowerName != PowerName)
            return;
        if (enemy.GetComponent<DamageHero>() is DamageHero hero)
            hero.damageDealt++;
        if (enemy.GetComponentInChildren<DamageHero>() is DamageHero damage)
            damage.damageDealt++;
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
    {
        ModHooks.SlashHitHook += NailSlash;
        ModHooks.GetPlayerIntHook += EmpowerNail;
        On.HeroController.TakeDamage += HeroController_TakeDamage;

    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ModHooks.SlashHitHook -= NailSlash;
        ModHooks.GetPlayerIntHook -= EmpowerNail;
        _damageStacks = 0;
        _currentlyRunning = false;
        _hasHitEnemy = false;
        UpdateNail();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Waits for the hit to finish and then checks if an enemy was hit.
    /// </summary>
    private IEnumerator HitCooldown()
    {
        // Give the event handler time to acknowledge a hit.
        yield return new WaitForSeconds(0.25f);

        if (_hasHitEnemy)
        {
            if (_damageStacks < (PlayerData.instance.GetInt(nameof(PlayerData.instance.nailSmithUpgrades)) + 1) * 3)
                _damageStacks++;
        }
        else
            _damageStacks = 0;

        UpdateNail();
    }

    /// <summary>
    /// Updates the nail and resets the flags.
    /// </summary>
    private void UpdateNail()
    {
        LoreMaster.Instance.Handler.StartCoroutine(WaitThenUpdate());
        _hasHitEnemy = false;
        _currentlyRunning = false;
    }

    /// <summary>
    /// Wait a frame and then call upon a nail damage update.
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitThenUpdate()
    {
        yield return null;
        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    #endregion
}
