using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using Modding;
using UnityEngine;

namespace LoreMaster.LorePowers.Waterways;

public class EternalSentinelPower : Power
{
    #region Members

    private tk2dSprite _baldurSprite;

    #endregion

    #region Constructors

    public EternalSentinelPower() : base("Eternal Sentinel", Area.WaterWays) { }

    #endregion

    #region Event Handler

    private void CharmUpdate(PlayerData data, HeroController controller)
    {
        if (data.GetBool(nameof(data.equippedCharm_10)))
        {
            PlayerData.instance.SetInt(nameof(PlayerData.instance.blockerHits), 10);
            _baldurSprite.color = new(1f, 0.4f, 0f);
            return;
        }

        PlayerData.instance.SetInt(nameof(PlayerData.instance.blockerHits), 7);
        _baldurSprite.color = Color.white;
    }

    private void OnSetPositionAction(On.HutongGames.PlayMaker.Actions.SetPosition.orig_OnEnter orig, SetPosition self)
    {
        if (string.Equals(self.Fsm.FsmComponent.gameObject.name, "Knight Dung Trail(Clone)") && string.Equals(self.Fsm.FsmComponent.FsmName, "Control") && string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Init"))
        {
            self.Fsm.FsmComponent.gameObject.transform.localPosition = HeroController.instance.transform.position;
            self.Fsm.FsmComponent.gameObject.transform.localScale = Active ? new(2.5f, 2.5f) : new(1f, 1f);
            self.Fsm.FsmComponent.gameObject.GetComponent<DamageEffectTicker>().SetDamageInterval(Active ? 0.15f : 0.3f);
        }

        orig(self);
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        GameObject baldurShield = GameObject.Find("Knight/Charm Effects").transform.Find("Blocker Shield").gameObject;

        // Modify FSM
        PlayMakerFSM baldurFSM = baldurShield.LocateMyFSM("Control");
        FsmState blockerHit = baldurFSM.GetState("Blocker Hit");
        for (int i = 4; i < 10; i++)
            blockerHit.AddTransition(i.ToString(), "Impact End");

        blockerHit.RemoveFirstActionOfType<IntSwitch>();
        blockerHit.AddLastAction(new Lambda(() =>
        {
            // Refunds soul on baldur hit on break
            if (Active && PlayerData.instance.GetBool("equippedCharm_10"))
                HeroController.instance.AddMPCharge(15);
            baldurFSM.SendEvent(baldurFSM.FsmVariables.FindFsmInt("Blocks").Value.ToString());
        }));

        _baldurSprite = baldurShield.GetComponentInChildren<tk2dSprite>();

        On.HutongGames.PlayMaker.Actions.SetPosition.OnEnter += OnSetPositionAction;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_10)))
            _baldurSprite.color = new(1f, 0.4f, 0f);
        ModHooks.CharmUpdateHook += CharmUpdate;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ModHooks.CharmUpdateHook -= CharmUpdate;
        _baldurSprite.color = Color.white;
        if (PlayerData.instance.GetInt(nameof(PlayerData.instance.blockerHits)) > 4)
            PlayerData.instance.SetInt(nameof(PlayerData.instance.blockerHits), 4);
    }

    #endregion
}
