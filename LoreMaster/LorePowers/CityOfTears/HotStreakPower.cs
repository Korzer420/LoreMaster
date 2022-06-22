using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears
{
    public class HotStreakPower : Power
    {
        #region Members

        private int _damageStacks = 0;
        private bool _currentlyRunning;
        private bool _hasHitEnemy;

        #endregion

        #region Constructors

        public HotStreakPower() : base("RUIN_TAB_01", Area.CityOfTears)
        {
            Description = "[Hot Streak]Successfully striking enemies shall increase your power.";
        }

        #endregion

        #region Methods

        public override void Enable()
        {
            ModHooks.SlashHitHook += ModHooks_SlashHitHook;
            ModHooks.GetPlayerIntHook += EmpowerNail;
        }

        public override void Disable()
        {
            ModHooks.SlashHitHook -= ModHooks_SlashHitHook;
            ModHooks.GetPlayerIntHook -= EmpowerNail;
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

        private int EmpowerNail(string name, int damage)
        {
            if (string.Equals(name, "nailDamage"))
                damage += _damageStacks;
            return damage;
        }

        IEnumerator HitCooldown()
        {
            // Give the event handler time to acknowledge a hit.
            yield return new WaitForSeconds(0.25f);

            if (_hasHitEnemy)
            {
                if (_damageStacks < 10)
                    _damageStacks++;
            }
            else
                _damageStacks = 0;

            UpdateNail();
        }

        private void UpdateNail()
        {
            IEnumerator WaitThenUpdate()
            {
                yield return null;
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            }
            GameManager.instance.StartCoroutine(WaitThenUpdate());

            _hasHitEnemy = false;
            _currentlyRunning = false;
        }

        #endregion
    }
}
