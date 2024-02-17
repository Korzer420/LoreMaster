using HutongGames.PlayMaker;
using LoreMaster.Enums;
using Modding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes;

public class EternalValorPower : Power
{
    #region Members

    private bool _hasHitEnemy;
    private bool _currentlyRunning;
    private int _successfulHits = 0;
    private float _heatTimer = 3f;
    private List<HealthManager> _hitEnemies = new();

    #endregion

    #region Constructors

    public EternalValorPower() : base("Eternal Valor", Area.FungalWastes) { }

    #endregion

    public float StackTimer => State == PowerState.Twisted ? 1f : 0.5f;

    public override PowerRank Rank => PowerRank.Greater;

    #region Event Handler

    /// <summary>
    /// Event handler for slashing with the nail
    /// </summary>
    private void ModHooks_SlashHitHook(Collider2D otherCollider, GameObject slash)
    {
        // This event is fired multiple times, therefore we check every instance if an enemy was hit
        if (otherCollider.gameObject.GetComponent<HealthManager>() is HealthManager enemy)
        {
            _hasHitEnemy = true;
            if (!_hitEnemies.Contains(enemy))
                _hitEnemies.Add(enemy);
        }

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

    /// <inheritdoc/>
    protected override void TwistEnable() => ModHooks.SlashHitHook += ModHooks_SlashHitHook;

    /// <inheritdoc/>
    protected override void TwistDisable() => ModHooks.SlashHitHook -= ModHooks_SlashHitHook;

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
            if (State == PowerState.Active && _successfulHits >= 12 
                && PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) < PlayerData.instance.GetInt(nameof(PlayerData.instance.maxHealth)))
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
            if(fadeTimer >= StackTimer)
            {
                _successfulHits--;
                fadeTimer = 0f;
            }
            yield return null;
        }
        _hitEnemies.RemoveAll(x => x == null);
        if(State == PowerState.Twisted && _hitEnemies.Any(x => x.hp > 0))
        {
            PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value, "Display");
            playMakerFSM.FsmVariables.GetFsmInt("Convo Amount").Value = 1;
            playMakerFSM.FsmVariables.GetFsmString("Convo Title").Value = "Coward";
            playMakerFSM.SendEvent("DISPLAY ENEMY DREAM");
            HeroController.instance.TakeDamage(null, GlobalEnums.CollisionSide.top, 2, 0);
        }
    }

    #endregion
}
