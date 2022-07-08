using LoreMaster.Enums;
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

    public HotStreakPower() : base("Hot Streak", Area.CityOfTears)
    {
        Hint = "Successfully striking enemies shall increase your power.";
        Description = "When hitting an enemy with the nail, increases it's damage by 1 (max. 3 stacks per nail upgrade (15 total)). Resets if you don't hit an enemy.";
    }

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
        LoreMaster.Instance.Handler.StartCoroutine(HitCooldown());
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

    #endregion

    #region Protected Methods

    protected override void Enable()
    {
        ModHooks.SlashHitHook += NailSlash;
        ModHooks.GetPlayerIntHook += EmpowerNail;
    }

    protected override void Disable()
    {
        ModHooks.SlashHitHook -= NailSlash;
        ModHooks.GetPlayerIntHook -= EmpowerNail;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Waits for the hit to finish and then checks if an enemy was hit.
    /// </summary>
    /// <returns></returns>
    private IEnumerator HitCooldown()
    {
        // Give the event handler time to acknowledge a hit.
        yield return new WaitForSeconds(0.25f);

        if (_hasHitEnemy)
        {
            if (_damageStacks < PlayerData.instance.nailSmithUpgrades * 3)
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
