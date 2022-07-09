using LoreMaster.Enums;
using Modding;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes;

public class EternalValorPower : Power
{
    #region Members

    private bool _hasHitEnemy;
    private bool _currentlyRunning;
    private int _successfulHits = 0;

    #endregion

    #region Constructors

    public EternalValorPower() : base("Eternal Valor", Area.FungalWastes)
    {
        Hint = "The heat of the battle shall allow you to endure the pain.";
        Description = "Each 30 hits on enemies, heal you for 1 mask.";
    }

    #endregion

    #region Event Handler

    /// <summary>
    /// Event handler for slashing with the nail
    /// </summary>
    private void ModHooks_SlashHitHook(Collider2D otherCollider, GameObject slash)
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

    #endregion

    #region Protected Methods

    protected override void Enable() => ModHooks.SlashHitHook += ModHooks_SlashHitHook;
    
    protected override void Disable() => ModHooks.SlashHitHook -= ModHooks_SlashHitHook;

    #endregion

    #region Private Methods

    /// <summary>
    /// Wait for the nail hit to resolve.
    /// </summary>
    /// <returns></returns>
    private IEnumerator HitCooldown()
    {
        // Give the event handler time to acknowledge a hit.
        yield return new WaitForSeconds(0.25f);

        if (_hasHitEnemy)
        {
            _successfulHits++;
            if (_successfulHits >= 30 && PlayerData.instance.health < PlayerData.instance.maxHealth)
            {
                HeroController.instance.AddHealth(1);
                _successfulHits = 0;
            }
        }
        _hasHitEnemy = false;
        _currentlyRunning = false;
    }

    #endregion
}
