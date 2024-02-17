using HutongGames.PlayMaker;
using KorzUtils.Helper;
using LoreMaster.Enums;

using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace LoreMaster.LorePowers.Greenpath;

public class GiftOfUnnPower : Power
{
    #region Member

    private ILHook _updateHook;

    #endregion
    
    #region Constructors

    public GiftOfUnnPower() : base("Gift of Unn", Area.Greenpath) { }

    #endregion

    #region Properties

    public override PowerRank Rank => PowerRank.Medium;

    #endregion

    #region Event Handler

    private void OnPlayerDataBoolTestAction(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, HutongGames.PlayMaker.Actions.PlayerDataBoolTest self)
    {
        if (string.Equals(self.Fsm.GameObjectName, "Knight") && string.Equals(self.Fsm.Name, "Spell Control"))
            if (string.Equals(self.State.Name, "Start Slug Anim"))
                self.isFalse = State == PowerState.Active ? null : FsmEvent.GetFsmEvent("FINISHED");
            else if (string.Equals(self.State.Name, "Slug?"))
                self.isFalse = State == PowerState.Active ? FsmEvent.GetFsmEvent("SLUG") : null;
        orig(self);
    }

    private void OnSpawnObjectFromGlobalPoolAction(On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool self)
    {
        if (State == PowerState.Active && string.Equals(self.Fsm.GameObjectName, "Knight") && string.Equals(self.Fsm.Name, "Spell Control")
            && string.Equals(self.State.Name, "Focus Heal 2") && PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_28)))
            HeroController.instance.AddMPCharge(15);
        orig(self);
    }

    private void TakeAdditionalMP(ILContext context)
    {
        ILCursor cursor = new(context);
        cursor.Goto(0);

        if (cursor.TryGotoNext(MoveType.After,
            x => x.MatchLdfld<HeroController>("drainMP_time"),
            x => x.MatchSub(),
            x => x.MatchStfld<HeroController>("drainMP_timer"),
            x => x.MatchLdarg(0),
            x => x.MatchLdcI4(1)))
        {
            cursor.EmitDelegate<Func<int, int>>(i => PlayerData.instance.GetBool("equippedCharm_28") ? 1 : 2);
        }
    }

    private void GetPlayerDataInt_OnEnter(On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.orig_OnEnter orig, HutongGames.PlayMaker.Actions.GetPlayerDataInt self)
    {
        orig(self);
        if ((self.IsCorrectContext("Spell Control", "Knight", "Full HP?") || self.IsCorrectContext("Spell Control", "Knight", "Full HP? 2")
            || self.IsCorrectContext("Spell Control", "Knight", "Can Focus?")) && string.Equals(self.intName.Value, "focusMP_amount"))
            self.storeValue.Value *= PlayerData.instance.GetBool("equippedCharm_28") ? 1 : 2;
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += OnPlayerDataBoolTestAction;
        On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.OnEnter += OnSpawnObjectFromGlobalPoolAction;
    }

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        _updateHook = new ILHook(typeof(HeroController).GetMethod("orig_Update", BindingFlags.NonPublic | BindingFlags.Instance), TakeAdditionalMP);
        On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.OnEnter += GetPlayerDataInt_OnEnter;
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        _updateHook?.Dispose();
        _updateHook = null;
        On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.OnEnter -= GetPlayerDataInt_OnEnter;
    }

    /// <inheritdoc/>
    protected override void Terminate()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter -= OnPlayerDataBoolTestAction;
        On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.OnEnter -= OnSpawnObjectFromGlobalPoolAction;
    }

    #endregion
}
