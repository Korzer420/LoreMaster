using LoreMaster.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LoreMaster.LorePowers.Dirtmouth;

/// <summary>
/// Unused currently.
/// </summary>
public class ElderbugHitListPower : Power
{
    #region Members

    private List<HealthManager> _enemies = new();
    private string _currentScene;

    #endregion

    #region Constructors

    public ElderbugHitListPower() : base("Elderbug Hit List", Area.Dirtmouth) { }

    #endregion

    #region Properties

    public Dictionary<string, ElderbugReward> ElderbugRewards { get; set; } = new();

    public bool CanAchieveGeo => ElderbugRewards.Count(x => x.Value == ElderbugReward.Geo) < 15;

    public bool CanAchieveMask => ElderbugRewards.Count(x => x.Value == ElderbugReward.MaskShard) < 12;

    public bool CanAchieveVessel => ElderbugRewards.Count(x => x.Value == ElderbugReward.SoulVessel) < 9;

    public bool CanAchieveNail => ElderbugRewards.Count(x => x.Value == ElderbugReward.Nail) < 5;

    public bool CanAchieveNotch => ElderbugRewards.Count(x => x.Value == ElderbugReward.Notch) < 5;

    public bool CanAchieveReward => CanAchieveGeo || CanAchieveMask || CanAchieveNail || CanAchieveReward || CanAchieveVessel;

    public override Action SceneAction => () => 
    {
        _currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        _enemies.Clear();
        if (!_currentScene.Contains("GG") && !ElderbugRewards.ContainsKey(_currentScene) && CanAchieveReward)
        {
            _enemies = GameObject.FindObjectsOfType<HealthManager>().ToList();
            if (_enemies.Count > 0)
                MarkEnemy();
        }
    };

    #endregion

    #region Event handler

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        if (self.GetComponents<Outline>() != null)
        {
            _enemies.Remove(self);
            GameObject.Destroy(self.GetComponent<Outline>());
            if (_enemies.Count > 0)
                MarkEnemy();
            else
                GiveReward();
        }
        else
        {
            HealthManager markedEnemy = _enemies.FirstOrDefault(x => x.GetComponent<Outline>() != null);
            if (markedEnemy != null)
                GameObject.Destroy(markedEnemy.GetComponent<Outline>());
        }
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.HealthManager.Die += HealthManager_Die;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.HealthManager.Die -= HealthManager_Die;
    }

    #endregion

    #region Private Methods

    private void MarkEnemy()
    {
        HealthManager selectedEnemy = _enemies[LoreMaster.Instance.Generator.Next(0, _enemies.Count)];
        Outline outline = selectedEnemy.gameObject.AddComponent<Outline>();
        outline.effectColor = Color.red;
    }

    private void GiveReward()
    {
        List<ElderbugReward> receivableRewards = new();

        if (CanAchieveGeo)
            receivableRewards.Add(ElderbugReward.Geo);
        if (CanAchieveMask)
            receivableRewards.Add(ElderbugReward.MaskShard);
        if (CanAchieveVessel)
            receivableRewards.Add(ElderbugReward.SoulVessel);
        if (CanAchieveNail)
            receivableRewards.Add(ElderbugReward.Nail);
        if (CanAchieveNotch)
            receivableRewards.Add(ElderbugReward.Notch);

        ElderbugReward selectedReward = receivableRewards[LoreMaster.Instance.Generator.Next(0, receivableRewards.Count)];

        switch (selectedReward)
        {
            case ElderbugReward.Geo:
                HeroController.instance.AddGeo(200);
                break;
            case ElderbugReward.MaskShard:

                break;
            case ElderbugReward.SoulVessel:
                break;
            case ElderbugReward.Notch:

                break;
            case ElderbugReward.Nail:
                break;
            default:
                break;
        }

        if (!CanAchieveReward)
            On.HealthManager.Die -= HealthManager_Die;
    }

    #endregion
}
