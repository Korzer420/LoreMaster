using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.Helper;
using LoreMaster.Manager;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

public class OverwhelmingPowerPower : Power
{
    #region Members

    private bool _hasFullSoulMeter;

    #endregion

    #region Constructors

    public OverwhelmingPowerPower() : base("Overwhelming Power", Area.CityOfTears) => Instance = this;

    #endregion

    #region Properties

    public static OverwhelmingPowerPower Instance { get; set; }

    public bool HasFullSoul => _hasFullSoulMeter;

    #endregion

    #region Event Handler

    private void SetFsmInt_OnEnter(On.HutongGames.PlayMaker.Actions.SetFsmInt.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetFsmInt self)
    {
        orig(self);
        if (self.IsCorrectContext("Set Damage", null, "Set Damage"))
        {
            tk2dSprite sprite = self.Fsm.FsmComponent.transform.parent?.GetComponent<tk2dSprite>();
            if (_hasFullSoulMeter && State == PowerState.Active)
            {
                self.Fsm.FsmComponent.gameObject.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt").Value *= 2;
                sprite.color = Color.cyan;
                self.Fsm.FsmComponent.transform.localScale = new(2f, 2f);
            }
            else if (State != PowerState.Twisted || _hasFullSoulMeter)
            {
                sprite.color = Color.white;
                self.Fsm.FsmComponent.transform.localScale = new(1f, 1f);
            }
            else if (!_hasFullSoulMeter && State == PowerState.Twisted)
            {
                self.Fsm.FsmComponent.gameObject.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt").Value = 1;
                sprite.color = Color.gray;
            }
        }
    }

    private void GetPlayerDataInt_OnEnter(On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.orig_OnEnter orig, HutongGames.PlayMaker.Actions.GetPlayerDataInt self)
    {
        orig(self);
        if (self.IsCorrectContext("Spell Control", "Knight", "Can Cast? QC") || self.IsCorrectContext("Spell Control", "Knight", "Can Cast?"))
            _hasFullSoulMeter = PlayerData.instance.GetInt(nameof(PlayerData.instance.MPCharge)) >= 99
            && !PlayerData.instance.GetBool(nameof(PlayerData.instance.soulLimited));
    }

    private void FloatCompare_OnEnter(On.HutongGames.PlayMaker.Actions.FloatCompare.orig_OnEnter orig, HutongGames.PlayMaker.Actions.FloatCompare self)
    {
        orig(self);
        if (self.IsCorrectContext("Fireball Control", null, "Init"))
        {
            tk2dSprite sprite = self.Fsm.FsmComponent.gameObject.GetComponent<tk2dSprite>();
            if (_hasFullSoulMeter && State == PowerState.Active)
            {
                self.Fsm.FsmComponent.gameObject.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt").Value *= 2;
                sprite.color = Color.cyan;
                self.Fsm.FsmComponent.transform.localScale = new(2f, 2f);
            }
            else if (State != PowerState.Twisted || _hasFullSoulMeter)
            {
                sprite.color = Color.white;
                self.Fsm.FsmComponent.transform.localScale = new(1f, 1f);
            }
            else if (!_hasFullSoulMeter && State == PowerState.Twisted)
            {
                self.Fsm.FsmComponent.gameObject.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt").Value = 1;
                sprite.color = Color.gray;
            }
        }
    }

    private void SetGravity2dScale_OnEnter(On.HutongGames.PlayMaker.Actions.SetGravity2dScale.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetGravity2dScale self)
    {
        orig(self);
        // Roll to determine if the player takes damage.
        if (_hasFullSoulMeter && State == PowerState.Active
            && self.IsCorrectContext("Spell Control", "Knight", "Spell End") && LoreMaster.Instance.Generator.Next(1, 11) == 1)
            LoreMaster.Instance.Handler.StartCoroutine(Overload());
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.OnEnter += GetPlayerDataInt_OnEnter;
        On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter += SetFsmInt_OnEnter;
        On.HutongGames.PlayMaker.Actions.FloatCompare.OnEnter += FloatCompare_OnEnter;
        On.HutongGames.PlayMaker.Actions.SetGravity2dScale.OnEnter += SetGravity2dScale_OnEnter;
    }

    /// <inheritdoc/>
    protected override void Terminate()
    {
        On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.OnEnter -= GetPlayerDataInt_OnEnter;
        On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter -= SetFsmInt_OnEnter;
        On.HutongGames.PlayMaker.Actions.FloatCompare.OnEnter -= FloatCompare_OnEnter;
        On.HutongGames.PlayMaker.Actions.SetGravity2dScale.OnEnter -= SetGravity2dScale_OnEnter;
    }

    #endregion

    #region Private Methods

    private IEnumerator Overload()
    {
        HeroController.instance.TakeDamage(HeroController.instance.gameObject, GlobalEnums.CollisionSide.top, 2, 0);
        PlayerData.instance.StartSoulLimiter();
        PlayMakerFSM.BroadcastEvent("SOUL LIMITER UP");
        yield return new WaitForSeconds(LoreMaster.Instance.Generator.Next(30, 121));
        // The shade spawn is determined by the soul limiter, which means to prevent the shade from spawning until the effect is over.
        if (string.Equals(PlayerData.instance.shadeScene, "None")
            || (PowerManager.HasObtainedPower("ABYSS_TUT_TAB_01")))
        {
            PlayerData.instance.EndSoulLimiter();
            PlayMakerFSM.BroadcastEvent("SOUL LIMITER DOWN");
        }
    }

    #endregion
}
