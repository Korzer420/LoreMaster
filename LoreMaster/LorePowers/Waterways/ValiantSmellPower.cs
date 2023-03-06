using HutongGames.PlayMaker.Actions;
using LoreMaster.Enums;
using LoreMaster.LorePowers;

public class EternalSentinelPower : Power
{
    #region Constructors

    public EternalSentinelPower() : base("Eternal Sentinel", Area.WaterWays) { }

    #endregion

    #region Event Handler

    private void OnSetPositionAction(On.HutongGames.PlayMaker.Actions.SetPosition.orig_OnEnter orig, SetPosition self)
    {
        if (string.Equals(self.Fsm.GameObjectName, "Knight Dung Trail(Clone)") && string.Equals(self.Fsm.Name, "Control")
            && string.Equals(self.State.Name, "Init"))
        {
            self.Fsm.FsmComponent.gameObject.transform.localPosition = HeroController.instance.transform.position;
            self.Fsm.FsmComponent.gameObject.transform.localScale = State == PowerState.Active ? new(2.5f, 2.5f) : new(1f, 1f);
            if (self.Fsm.FsmComponent.gameObject.GetComponent<DamageEffectTicker>() is DamageEffectTicker dET)
                dET.SetDamageInterval(State == PowerState.Active ? 0.15f : 0.3f);
        }

        orig(self);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        On.HutongGames.PlayMaker.Actions.SetPosition.OnEnter += OnSetPositionAction;
    }

    /// <inheritdoc/>
    protected override void Terminate()
    {
        On.HutongGames.PlayMaker.Actions.SetPosition.OnEnter += OnSetPositionAction;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {

    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        
    }

    #endregion
}
