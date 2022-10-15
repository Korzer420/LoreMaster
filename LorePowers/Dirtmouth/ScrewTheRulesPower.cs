using HutongGames.PlayMaker.Actions;
using LoreMaster.Enums;
using LoreMaster.Helper;
using MonoMod.Cil;
using System;
using UnityEngine;

namespace LoreMaster.LorePowers.Dirtmouth;

/// <summary>
/// Fury of the Fallen is active, while you have two health, but only increases your damage by 50%.
/// </summary>
public class ScrewTheRulesPower : Power
{
    #region Constructors

    public ScrewTheRulesPower() : base("Screw the Rules", Area.Dirtmouth) { }

    #endregion

    #region Event Handler

    private void OnSetFsmFloatAction(On.HutongGames.PlayMaker.Actions.SetFsmFloat.orig_OnEnter orig, SetFsmFloat self)
    {
        if (string.Equals(self.Fsm.GameObjectName, "Charm Effects") && string.Equals(self.Fsm.Name, "Fury") && string.Equals(self.State.Name, "Activate"))
            self.setValue.Value = State == PowerState.Active ? 1.5f : 1.75f;
        orig(self);
    }

    private void OnFloatMultiplyAction(On.HutongGames.PlayMaker.Actions.FloatMultiply.orig_OnEnter orig, FloatMultiply self)
    {
        if (string.Equals(self.Fsm.Name, "nailart_damage") && string.Equals(self.State.Name, "Fury?"))
            self.multiplyBy.Value = State == PowerState.Active ? 1.5f : 1.75f;
        else if (self.Fsm.GameObjectName.Contains("Grubberfly Beam") && string.Equals(self.Fsm.Name, "Control"))
            self.multiplyBy.Value = State == PowerState.Active ? 1.3f : 1.5f;
        orig(self);
    }

    private void OnIntCompareAction(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
    {
        if (string.Equals(self.Fsm.GameObjectName, "Charm Effects") && string.Equals(self.Fsm.Name, "Fury") && (string.Equals(self.State.Name, "Check HP") || string.Equals(self.State.Name, "Recheck")) && PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) == 2)
        {
            if (string.Equals(self.State.Name, "Check HP"))
                self.Fsm.FsmComponent.SendEvent("FURY");
            else if (string.Equals(self.State.Name, "Recheck"))
                self.Fsm.FsmComponent.SendEvent("RETURN");
        }
        orig(self);
    }

    private void HeroController_Attack(ILContext il)
    {
        ILCursor cursor = new(il);
        try
        {
            // Modifies right/left slash, up slash and down slash (the first health check is for full health, which we ignore here)
            for (int i = 0; i < 3; i++)
            {
                cursor.GotoNext(MoveType.After,
                x => x.MatchLdfld<HeroController>("playerData"),
                x => x.MatchLdstr("health"),
                x => x.MatchCallvirt<PlayerData>("GetInt"));

                cursor.GotoNext(MoveType.After,
                x => x.MatchLdfld<HeroController>("playerData"),
                x => x.MatchLdstr("health"),
                x => x.MatchCallvirt<PlayerData>("GetInt"));

                cursor.EmitDelegate<Func<int, int>>((x) => x <= 2 ? 1 : x);
            }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Couldn't modify grubberfly fury condition: " + exception);
        }
    }

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (hitInstance.AttackType == AttackTypes.Nail)
            hitInstance.Multiplier = 100 / ((float)PlayerData.instance.GetInt("maxHealth") / PlayerData.instance.GetInt("health")) / 100;
        orig(self, hitInstance);
    }

    private void PreventFury(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, PlayerDataBoolTest self)
    {
        orig(self);
        if (self.IsCorrectContext("Fury", "Charm Effects", "Check HP"))
            self.Fsm.FsmComponent.SendEvent("CANCEL");
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        On.HutongGames.PlayMaker.Actions.FloatMultiply.OnEnter += OnFloatMultiplyAction;
        On.HutongGames.PlayMaker.Actions.SetFsmFloat.OnEnter += OnSetFsmFloatAction;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        IL.HeroController.Attack += HeroController_Attack;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += OnIntCompareAction;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        IL.HeroController.Attack -= HeroController_Attack;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter -= OnIntCompareAction;
        // Disable the fury effect if you leave with 2 hp.
        if (PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) == 2)
        {
            PlayMakerFSM fsm = GameObject.Find("Knight/Charm Effects").LocateMyFSM("Fury");
            fsm.SendEvent("HERO HEALED");
        }
    }

    /// <inheritdoc/>
    protected override void Terminate()
    {
        On.HutongGames.PlayMaker.Actions.FloatMultiply.OnEnter -= OnFloatMultiplyAction;
        On.HutongGames.PlayMaker.Actions.SetFsmFloat.OnEnter -= OnSetFsmFloatAction;
    }

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        // This should remove the fury buff if it is active at the time.
        if (PlayerData.instance.GetInt("health") == 1 && !PlayerData.instance.GetBool("equippedCharm_27"))
            HeroController.instance.AddHealth(1);
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += PreventFury;
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter -= PreventFury;
    }

    #endregion
}
