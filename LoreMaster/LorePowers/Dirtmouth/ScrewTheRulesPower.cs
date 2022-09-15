using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
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
        if (string.Equals(self.Fsm.FsmComponent.gameObject.name, "Charm Effects") && string.Equals(self.Fsm.FsmComponent.FsmName, "Fury") && string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Activate"))
        {
            self.setValue.Value = Active ? 1.5f : 1.75f;
        }
        orig(self);
    }

    private void OnFloatMultiplyAction(On.HutongGames.PlayMaker.Actions.FloatMultiply.orig_OnEnter orig, FloatMultiply self)
    {
        if (string.Equals(self.Fsm.FsmComponent.FsmName, "nailart_damage") && string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Fury?"))
        {
            self.multiplyBy.Value = Active ? 1.5f : 1.75f;
        }
        else if (self.Fsm.FsmComponent.gameObject.name.Contains("Grubberfly Beam") && string.Equals(self.Fsm.FsmComponent.FsmName, "Control"))
        {
            self.multiplyBy.Value = Active ? 1.3f : 1.5f;
        }
        orig(self);
    }

    private void OnIntCompareAction(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
    {
        if (string.Equals(self.Fsm.FsmComponent.gameObject.name, "Charm Effects") && string.Equals(self.Fsm.FsmComponent.FsmName, "Fury") && (string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Check HP") || string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Recheck")) && PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) == 2 && Active)
        {
            if (self.Fsm.FsmComponent.ActiveStateName == "Check HP")
            {
                self.Fsm.FsmComponent.SendEvent("FURY");
            }
            else if (self.Fsm.FsmComponent.ActiveStateName == "Recheck")
            {
                self.Fsm.FsmComponent.SendEvent("RETURN");
            }
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
        catch (System.Exception exception)
        {
            LoreMaster.Instance.LogError("Couldn't modify grubberfly fury condition: " + exception);
        }
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        On.HutongGames.PlayMaker.Actions.FloatMultiply.OnEnter += OnFloatMultiplyAction;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += OnIntCompareAction;
        On.HutongGames.PlayMaker.Actions.SetFsmFloat.OnEnter += OnSetFsmFloatAction;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        IL.HeroController.Attack += HeroController_Attack;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        IL.HeroController.Attack -= HeroController_Attack;
        // Disable the fury effect if you leave with 2 hp.
        if (PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) == 2)
        {
            PlayMakerFSM fsm = GameObject.Find("Knight/Charm Effects").LocateMyFSM("Fury");
            fsm.SendEvent("HERO HEALED");
        }
    }

    #endregion
}
