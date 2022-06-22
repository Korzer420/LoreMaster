using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes
{
    public class EternalValorPower : Power
    {
        #region Members

        private bool _hasHitEnemy;
        private bool _currentlyRunning;
        private int _successfulHits = 0;

        #endregion

        #region Constructors

        public EternalValorPower() : base("MANTIS_PLAQUE_02", Area.FungalWastes)
        {
            Description = "[Eternal Valor]<br>The heat of the battle shall allow you to endure the pain.";
        }

        #endregion

        #region Methods

        public override void Enable()
        {
            ModHooks.SlashHitHook += ModHooks_SlashHitHook;
        }

        public override void Disable()
        {
            throw new NotImplementedException();
        }

        private void ModHooks_SlashHitHook(Collider2D otherCollider, GameObject slash)
        {
            // This event is fired multiple times, therefore we check every instance if an enemy was hit
            if (otherCollider.gameObject.GetComponent<HealthManager>())
                _hasHitEnemy = true;

            // To prevent running multiple coroutines
            if (_currentlyRunning)
                return;

            _currentlyRunning = true;
            GameManager.instance.StartCoroutine(HitCooldown());
        }

        IEnumerator HitCooldown()
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
}
