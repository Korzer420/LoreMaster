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
    private float _heatTimer = 3f;

    #endregion

    #region Constructors

    public EternalValorPower() : base("Eternal Valor", Area.FungalWastes) { }

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

    #region Control

    /// <inheritdoc/>
    protected override void Enable() => ModHooks.SlashHitHook += ModHooks_SlashHitHook;

    /// <inheritdoc/>
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
            _heatTimer = 3f;
            if (_runningCoroutine == null)
                _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(KeepHeat());
            
            if(_successfulHits < 24)
            _successfulHits++;
            if (_successfulHits >= 12 && PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) < PlayerData.instance.GetInt(nameof(PlayerData.instance.maxHealth)))
            {
                HeroController.instance.AddHealth(1);
                _successfulHits -= 12;
                // Honestly, this shouldn't be possible, but I want to make sure, just in case, that everything works.
                if (_successfulHits < 0)
                    _successfulHits = 0;
            }
        }
        _hasHitEnemy = false;
        _currentlyRunning = false;
    }

    /// <summary>
    /// Handles the stack duration.
    /// </summary>
    private IEnumerator KeepHeat()
    {
        while(_heatTimer > 0)
        {
            _heatTimer -= Time.deltaTime;
            yield return null;
        }
        _heatTimer = 0f;
        float fadeTimer = 0f;
        while(_heatTimer == 0 && _successfulHits > 0)
        {
            fadeTimer += Time.deltaTime;
            if(fadeTimer >= .5f)
            {
                _successfulHits--;
                fadeTimer = 0f;
            }
            yield return null;
        }
    }

    #endregion
}
