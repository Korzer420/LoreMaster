using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.LorePowers.CityOfTears;
using UnityEngine;

namespace LoreMaster.LorePowers.Waterways;

public class RelentlessSwarmPower : Power
{
    #region Constructors

    public RelentlessSwarmPower() : base("Relentless Swarm", Area.WaterWays) { }

    #endregion

    #region Event Handler

    private void SpellFluke_DoDamage(On.SpellFluke.orig_DoDamage orig, SpellFluke self, GameObject obj, int upwardRecursionAmount, bool burst)
    {
        orig(self, obj, upwardRecursionAmount, burst);
        if (obj.GetComponent<HealthManager>().isDead)
            HeroController.instance.AddMPCharge(5);
        else
            HeroController.instance.AddMPCharge(2);
    }

    private void FlingObjectsFromGlobalPool_OnEnter(On.HutongGames.PlayMaker.Actions.FlingObjectsFromGlobalPool.orig_OnEnter orig, HutongGames.PlayMaker.Actions.FlingObjectsFromGlobalPool self)
    {
        if (self.IsCorrectContext("Fireball Cast", null, "Flukes"))
        {
            if (State == PowerState.Twisted)
            { 
                self.spawnMin.Value = PlayerData.instance.GetBool("equippedCharm_11") ? 4 : 1;
                self.spawnMax.Value = PlayerData.instance.GetBool("equippedCharm_11") ? 4 : 1;
            }
            else if (State == PowerState.Active && OverwhelmingPowerPower.Instance.State == PowerState.Active
                && OverwhelmingPowerPower.Instance.HasFullSoul)
            {
                self.spawnMin.Value = self.Fsm.GameObjectName.Contains("2") ? 20 : 13;
                self.spawnMax.Value = self.Fsm.GameObjectName.Contains("2") ? 26 : 19;
            }
            else
            {
                self.spawnMin.Value = self.Fsm.GameObjectName.Contains("2") ? 16 : 9;
                self.spawnMax.Value = self.Fsm.GameObjectName.Contains("2") ? 16 : 9;
            }
        }
        orig(self);
    }

    private void PlayerDataBoolTest_OnEnter(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, HutongGames.PlayMaker.Actions.PlayerDataBoolTest self)
    {
        orig(self);
        if (State == PowerState.Twisted && self.IsCorrectContext("Fireball Cast", null, "Cast Right") || self.IsCorrectContext("Fireball Cast", null, "Cast Left"))
            self.Fsm.FsmComponent.SendEvent("FLUKE");
    }

    private void AudioPlayerOneShotSingle_OnEnter(On.HutongGames.PlayMaker.Actions.AudioPlayerOneShotSingle.orig_OnEnter orig, HutongGames.PlayMaker.Actions.AudioPlayerOneShotSingle self)
    {
        orig(self);
        if (self.IsCorrectContext("Fireball Cast", null, "Fluke L") || self.IsCorrectContext("Fireball Cast", null, "Fluke R"))
            self.Fsm.FsmComponent.SendEvent("FINISHED");
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize() => On.HutongGames.PlayMaker.Actions.FlingObjectsFromGlobalPool.OnEnter += FlingObjectsFromGlobalPool_OnEnter;

    /// <inheritdoc/>
    protected override void Terminate() => On.HutongGames.PlayMaker.Actions.FlingObjectsFromGlobalPool.OnEnter -= FlingObjectsFromGlobalPool_OnEnter;

    /// <inheritdoc/>
    protected override void Enable() => On.SpellFluke.DoDamage += SpellFluke_DoDamage;

    /// <inheritdoc/>
    protected override void Disable() => On.SpellFluke.DoDamage -= SpellFluke_DoDamage;

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        On.HutongGames.PlayMaker.Actions.AudioPlayerOneShotSingle.OnEnter += AudioPlayerOneShotSingle_OnEnter;
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += PlayerDataBoolTest_OnEnter;
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        On.HutongGames.PlayMaker.Actions.AudioPlayerOneShotSingle.OnEnter -= AudioPlayerOneShotSingle_OnEnter;
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter -= PlayerDataBoolTest_OnEnter;
    }

    #endregion
}