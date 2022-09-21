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

    public EternalSentinelPower() : base("Eternal Sentinel", Area.Waterways) { }

    #endregion

    #region Properties

    public tk2dSprite BaldurSprite => _baldurSprite == null ? _baldurSprite = GameObject.Find("Knight/Charm Effects").transform.Find("Blocker Shield").gameObject.GetComponentInChildren<tk2dSprite>() : _baldurSprite;

    #endregion

    #region Event Handler

    private void CharmUpdate(PlayerData data, HeroController controller)
    {
        if (data.GetBool(nameof(data.equippedCharm_10)))
        {
            PlayerData.instance.SetInt(nameof(PlayerData.instance.blockerHits), 10);
            BaldurSprite.color = new(1f, 0.4f, 0f);
            return;
        }

        PlayerData.instance.SetInt(nameof(PlayerData.instance.blockerHits), 7);
        BaldurSprite.color = Color.white;
    }

    private void OnSetPositionAction(On.HutongGames.PlayMaker.Actions.SetPosition.orig_OnEnter orig, SetPosition self)
    {
        if (string.Equals(self.Fsm.GameObjectName, "Knight Dung Trail(Clone)") && string.Equals(self.Fsm.Name, "Control") 
            && string.Equals(self.State.Name, "Init"))
        {
            self.Fsm.FsmComponent.gameObject.transform.localPosition = HeroController.instance.transform.position;
            self.Fsm.FsmComponent.gameObject.transform.localScale = Active ? new(2.5f, 2.5f) : new(1f, 1f);
            self.Fsm.FsmComponent.gameObject.GetComponent<DamageEffectTicker>().SetDamageInterval(Active ? 0.15f : 0.3f);
        }

        orig(self);
    }

    private void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        ModifyHero();
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        ModifyHero();
        On.HutongGames.PlayMaker.Actions.SetPosition.OnEnter += OnSetPositionAction;
        On.HeroController.Start += HeroController_Start;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_10)))
            BaldurSprite.color = new(1f, 0.4f, 0f);
        ModHooks.CharmUpdateHook += CharmUpdate;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ModHooks.CharmUpdateHook -= CharmUpdate;
        BaldurSprite.color = Color.white;
        if (PlayerData.instance.GetInt(nameof(PlayerData.instance.blockerHits)) > 4)
            PlayerData.instance.SetInt(nameof(PlayerData.instance.blockerHits), 4);
    }

    /// <inheritdoc/>
    protected override void Terminate()
    {
        On.HutongGames.PlayMaker.Actions.SetPosition.OnEnter -= OnSetPositionAction;
        On.HeroController.Start -= HeroController_Start;
    }

    #endregion

    #region Methods

    private void ModifyHero()
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
    }

    #endregion
}
