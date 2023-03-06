using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.Helper;
using UnityEngine;

namespace LoreMaster.LorePowers.QueensGarden;

public class FollowTheLightPower : Power
{
    #region Members

    private GameObject _dreamGate;

    private Vector3 _gatePosition;

    #endregion

    #region Constructors

    public FollowTheLightPower() : base("Follow the Light", Area.QueensGarden) { }

    #endregion

    #region Event handler

    private void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        AddDreamGateTeleport();
    }

    private void AudioPlayerOneShotSingle_OnEnter(On.HutongGames.PlayMaker.Actions.AudioPlayerOneShotSingle.orig_OnEnter orig, AudioPlayerOneShotSingle self)
    {
        orig(self);
        if (self.IsCorrectContext("Dream Nail", "Knight", "Slash"))
        {
            PlayerData.instance.SetInt(nameof(PlayerData.instance.dreamOrbs), PlayerData.instance.GetInt(nameof(PlayerData.instance.dreamOrbs)) -
                (!PlayerData.instance.GetBool("dreamNailUpgraded") ? 2 : 1));
            PlayMakerFSM.BroadcastEvent("DREAM ORB COLLECT");
        }
    }

    private bool HeroController_CanDreamNail(On.HeroController.orig_CanDreamNail orig, HeroController self) => orig(self)
        && PlayerData.instance.GetInt("dreamOrbs") > (!PlayerData.instance.GetBool("dreamNailUpgraded") ? 1 : 0);

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        AddDreamGateTeleport();
        On.HeroController.Start += HeroController_Start;
    }

    /// <inheritdoc/>
    protected override void Terminate() => On.HeroController.Start -= HeroController_Start;

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        On.HeroController.CanDreamNail += HeroController_CanDreamNail;
        On.HutongGames.PlayMaker.Actions.AudioPlayerOneShotSingle.OnEnter += AudioPlayerOneShotSingle_OnEnter;
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        On.HeroController.CanDreamNail -= HeroController_CanDreamNail;
        On.HutongGames.PlayMaker.Actions.AudioPlayerOneShotSingle.OnEnter -= AudioPlayerOneShotSingle_OnEnter;
    }

    #endregion

    #region Methods

    private void AddDreamGateTeleport()
    {
        PlayMakerFSM fsm = HeroController.instance.gameObject.LocateMyFSM("Dream Nail");
        GameObject dreamGate = fsm.GetState("Spawn Gate").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Set Light Gate",
            Actions = new FsmStateAction[]
            {
                new Lambda(() =>
                {
                    if(State == PowerState.Active)
                    {
                        if(_dreamGate != null)
                            GameObject.Destroy(_dreamGate);
                        _dreamGate = GameObject.Instantiate(dreamGate);
                        foreach (SpriteRenderer spriteRenderer in _dreamGate.GetComponentsInChildren<SpriteRenderer>(true))
                        spriteRenderer.color = new(1f, 0.4f, 0f);

                        _dreamGate.transform.localPosition = HeroController.instance.transform.localPosition;
                        // Adjust to place the portal on the ground
                        _dreamGate.transform.localPosition -= new Vector3(0f,1.4f,0f);
                        _gatePosition = HeroController.instance.transform.localPosition;
                    }
                })
            }
        });
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Move to Light Gate",
            Actions = new FsmStateAction[]
            {
                new Lambda(() =>
                {
                    if(State == PowerState.Active && _dreamGate != null && (PlayerData.instance.GetInt(nameof(PlayerData.instance.dreamOrbs)) > 0 || PlayerData.instance.GetBool(nameof(PlayerData.instance.dreamNailUpgraded))))
                    {
                        // Check for awoken dreamnail
                        if(!PlayerData.instance.GetBool(nameof(PlayerData.instance.dreamNailUpgraded)))
                        {
                            GameObject.Destroy(_dreamGate);
                            PlayerData.instance.SetInt(nameof(PlayerData.instance.dreamOrbs),PlayerData.instance.GetInt(nameof(PlayerData.instance.dreamOrbs)) - 1);
                            PlayMakerFSM.BroadcastEvent("DREAM ORB COLLECT");
                            HeroController.instance.transform.localPosition = _gatePosition;
                        }
                        else
                            HeroController.instance.transform.localPosition = _gatePosition;
                    }
                })
            }
        });
        fsm.GetState("Dream Gate?").InsertAction(new Lambda(() =>
        {
            if (State == PowerState.Active)
            {
                if (InputHandler.Instance.inputActions.left.IsPressed)
                    fsm.SendEvent("PLACE");
                else if (InputHandler.Instance.inputActions.right.IsPressed)
                    fsm.SendEvent("MOVE");
            }

        }), 3);
        fsm.GetState("Dream Gate?").AddTransition("PLACE", "Set Light Gate");
        fsm.GetState("Dream Gate?").AddTransition("MOVE", "Move to Light Gate");
        fsm.GetState("Set Light Gate").AddTransition("FINISHED", "Slash Antic");
        fsm.GetState("Move to Light Gate").AddTransition("FINISHED", "Slash Antic");
    }

    #endregion
}
