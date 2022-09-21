using HutongGames.PlayMaker;
using LoreMaster.Enums;

namespace LoreMaster.LorePowers.Greenpath;

public class GiftOfUnnPower : Power
{
    #region Constructors

    public GiftOfUnnPower() : base("Gift of Unn", Area.Greenpath) { }

    #endregion

    #region Event Handler

    private void OnPlayerDataBoolTestAction(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, HutongGames.PlayMaker.Actions.PlayerDataBoolTest self)
    {
        if (string.Equals(self.Fsm.GameObjectName, "Knight") && string.Equals(self.Fsm.Name, "Spell Control"))
            if (string.Equals(self.State.Name, "Start Slug Anim"))
                self.isFalse = Active ? null : FsmEvent.GetFsmEvent("FINISHED");
            else if (string.Equals(self.State.Name, "Slug?"))
                self.isFalse = Active ? FsmEvent.GetFsmEvent("SLUG") : null;
        orig(self);
    }

    private void OnSpawnObjectFromGlobalPoolAction(On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool self)
    {
        if (Active && string.Equals(self.Fsm.GameObjectName, "Knight") && string.Equals(self.Fsm.Name, "Spell Control")
            && string.Equals(self.State.Name, "Focus Heal 2") && PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_28)))
            HeroController.instance.AddMPCharge(15);
        orig(self);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += OnPlayerDataBoolTestAction;
        On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.OnEnter += OnSpawnObjectFromGlobalPoolAction;
    }

    protected override void Terminate()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter -= OnPlayerDataBoolTestAction;
        On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.OnEnter -= OnSpawnObjectFromGlobalPoolAction;
    }

    #endregion
}
