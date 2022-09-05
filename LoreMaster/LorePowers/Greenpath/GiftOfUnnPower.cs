using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
using HutongGames.PlayMaker;

namespace LoreMaster.LorePowers.Greenpath;

public class GiftOfUnnPower : Power
{
    #region Constructors

    public GiftOfUnnPower() : base("Gift of Unn", Area.Greenpath) { }

    #endregion

    #region Event Handler

    private void OnPlayerDataBoolTestAction(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, HutongGames.PlayMaker.Actions.PlayerDataBoolTest self)
    {
        if (string.Equals(self.Fsm.FsmComponent.gameObject.name, "Knight") && string.Equals(self.Fsm.FsmComponent.FsmName, "Spell Control"))
        {
            if (string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Start Slug Anim"))
            {
                self.isFalse = Active ? null : FsmEvent.GetFsmEvent("FINISHED");
            }
            else if (string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Slug?"))
            {
                self.isFalse = Active ? FsmEvent.GetFsmEvent("SLUG") : null;
            }
        }
        orig(self);
    }

    // If the player has shape of unn equipped, it gives 15 mp on a successful cast (this is added, to prevent making the charm useless)
    private void OnSpawnObjectFromGlobalPoolAction(On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool self)
    {
        if (string.Equals(self.Fsm.FsmComponent.gameObject.name, "Knight") && string.Equals(self.Fsm.FsmComponent.FsmName, "Spell Control") && string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Focus Heal 2") && PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_28)) && Active)
        {
            HeroController.instance.AddMPCharge(15);
        }

        orig(self);
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += OnPlayerDataBoolTestAction;
        On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.OnEnter += OnSpawnObjectFromGlobalPoolAction;
    }

    #endregion
}
