using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
using Modding;
using UnityEngine;
using static UnityEngine.ParticleSystem;

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
            self.Fsm.FsmComponent.gameObject.transform.localScale = State == PowerState.Active ? new(2.5f, 2.5f) : new(1f, 1f);
            self.Fsm.FsmComponent.gameObject.GetComponent<DamageEffectTicker>().SetDamageInterval(State == PowerState.Active ? 0.15f : 0.3f);
        }

        orig(self);
    }

    private void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        ModifyHero();
    }

    private void Wait_OnEnter(On.HutongGames.PlayMaker.Actions.Wait.orig_OnEnter orig, Wait self)
    {
        if (self.IsCorrectContext("Control", null, "Wait") && self.Fsm.FsmComponent.gameObject.name.Contains("Knight Dung Trail"))
        { 
            self.time.Value = State == PowerState.Twisted ? (PlayerData.instance.GetBool("equippedCharm_10") ? 10f : 5f) : 1.1f;
            if (State == PowerState.Twisted)
            {
                Component.Destroy(self.Fsm.FsmComponent.gameObject.GetComponent<AutoRecycleSelf>());
                Component.Destroy(self.Fsm.FsmComponent.gameObject.GetComponent<DamageEffectTicker>());
                MainModule module = self.Fsm.FsmComponent.transform.GetChild(0).GetComponent<ParticleSystem>().main;
                module.loop = true;
            }
        }
        orig(self);
    }

    private void PlayerDataBoolTest_OnEnter(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, PlayerDataBoolTest self)
    {
        if (self.IsCorrectContext("Control", "Dung", "Check"))
            self.Fsm.FsmComponent.SendEvent("EQUIPPED");
        orig(self);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        ModifyHero();
        On.HutongGames.PlayMaker.Actions.SetPosition.OnEnter += OnSetPositionAction;
        On.HeroController.Start += HeroController_Start;
        On.HutongGames.PlayMaker.Actions.Wait.OnEnter += Wait_OnEnter;
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

    protected override void TwistEnable()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += PlayerDataBoolTest_OnEnter;
        PlayMakerFSM.BroadcastEvent("CHARM EQUIP CHECK");
    }

    protected override void TwistDisable()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter -= PlayerDataBoolTest_OnEnter;
        PlayMakerFSM.BroadcastEvent("CHARM EQUIP CHECK");
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
