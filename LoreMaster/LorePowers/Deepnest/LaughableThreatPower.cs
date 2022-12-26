using LoreMaster.Enums;
using LoreMaster.UnityComponents;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoreMaster.LorePowers.Deepnest;

public class LaughableThreatPower : Power
{
    #region Members

    private bool _hasAttacked;

    private List<GameObject> _enemies = new();

    #endregion

    #region Constructors

    public LaughableThreatPower() : base ("Laughable Threat", Area.Deepnest)
    {
        
    }

    #endregion

    #region Properties

    public override Action SceneAction => () => 
    {
        if (State == PowerState.Active)
        {
            _hasAttacked = false;
            _enemies.Clear();
            _enemies.AddRange(GameObject.FindObjectsOfType<HealthManager>().Select(x => x.gameObject));
            UpdateEnemyCollider();
        }
        else
        {
            PlayMakerFSM.BroadcastEvent("ALERT");
            PlayMakerFSM.BroadcastEvent("TOOK DAMAGE");
        }
    };

    #endregion

    #region Control

    protected override void Enable()
    {
        ModHooks.HitInstanceHook += ModHooks_HitInstanceHook;
    }

    protected override void Disable()
    {
        ModHooks.HitInstanceHook -= ModHooks_HitInstanceHook;
    }

    #endregion

    #region Methods

    private HitInstance ModHooks_HitInstanceHook(HutongGames.PlayMaker.Fsm owner, HitInstance hit)
    {
        if (hit.AttackType == AttackTypes.Nail && !_hasAttacked)
        {
            _hasAttacked = true;
            UpdateEnemyCollider();
        }
        return hit;
    }

    private void UpdateEnemyCollider()
    {
        foreach (GameObject enemy in _enemies)
            if (!_hasAttacked && enemy.GetComponent<Pacify>() == null)
                enemy.AddComponent<Pacify>();
            else if(_hasAttacked && enemy.GetComponent<Pacify>() != null)
                Component.Destroy(enemy.GetComponent<Pacify>());
    }

    #endregion
}
