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

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (string.Equals(self.FsmName,"nailart_damage"))
            self.GetState("Fury?").ReplaceAction(new Lambda(() => self.FsmVariables.FindFsmFloat("Damage Float").Value *= Active ? 1.5f : 1.75f) { Name = "Fury Nerf" }, 1);
        else if (self.gameObject.name.Contains("Grubberfly Beam") && string.Equals(self.FsmName,"Control"))
            self.GetState("Fury Multiplier").ReplaceAction(new Lambda(() => self.FsmVariables.FindFsmFloat("Dmg Float").Value *= Active ? 1.3f : 1.5f) { Name = "Fury Amplifier" }, 2);
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
        try
        {
            PlayMakerFSM fsm = GameObject.Find("Knight").transform.Find("Charm Effects").gameObject.LocateMyFSM("Fury");
            FsmState state = fsm.GetState("Check HP");

            // Remove "HP Compare" action and add new one, so fury is active while have two OR LESS health
            state.ReplaceAction(new Lambda(() =>
            {
                if (PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) == 1 || (PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) == 2 && Active))
                    fsm.SendEvent("FURY");
                else
                    fsm.SendEvent("CANCEL");
            })
            { Name = "HP Compare" }, 6);

            // Also changing the recheck for after getting hit or healed
            fsm.GetState("Recheck").ReplaceAction(new Lambda(() =>
            {
                if (PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) == 1 || (PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) == 2 && Active))
                    fsm.SendEvent("RETURN");
            })
            { Name = "HP Compare" }, 1);

            // We are nerfing the damage increase to 50% (The action 9 to 12 are setting the modifier in the nail fsm)
            fsm.GetState("Get Ref").ReplaceAction(new Lambda(() =>
            {
                foreach (SetFsmFloat action in state.GetActionsOfType<SetFsmFloat>())
                    action.setValue.Value = Active ? 1.5f : 1.75f;
                if (fsm.FsmVariables.FindFsmGameObject("Fury Vignette").Value != null)
                    fsm.SendEvent("FINISHED");
            })
            { Name = "Nerf damage" }, 0);
        }
        catch (Exception error)
        {
            LoreMaster.Instance.LogError("Couldn't modify fury fsm: " + error.Message);
        }
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        IL.HeroController.Attack += HeroController_Attack;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
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
