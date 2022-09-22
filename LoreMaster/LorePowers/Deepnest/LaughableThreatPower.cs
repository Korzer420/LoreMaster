using LoreMaster.Enums;
using LoreMaster.UnityComponents;
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
        _hasAttacked = false;
        _enemies.Clear();
        _enemies.AddRange(GameObject.FindObjectsOfType<HealthManager>().Select(x => x.gameObject));
        UpdateEnemyCollider();
    };

    #endregion

    #region Control

    protected override void Enable()
    {
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
    }

    protected override void Disable()
    {
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
    }

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (!_hasAttacked)
        {
            _hasAttacked = true;
            UpdateEnemyCollider();
        }
        orig(self, hitInstance);
    }

    #endregion

    #region Methods

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
