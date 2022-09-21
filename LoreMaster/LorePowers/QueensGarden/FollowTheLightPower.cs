using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
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

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        var fsm = HeroController.instance.gameObject.LocateMyFSM("Dream Nail");
        GameObject dreamGate = fsm.GetState("Spawn Gate").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Set Light Gate",
            Actions = new FsmStateAction[]
            {
                new Lambda(() =>
                {
                    if(Active)
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
                    if(Active && _dreamGate != null && (PlayerData.instance.GetInt(nameof(PlayerData.instance.dreamOrbs)) > 0 || PlayerData.instance.GetBool(nameof(PlayerData.instance.dreamNailUpgraded))))
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
                        {
                            _dreamGate.transform.localPosition = HeroController.instance.transform.localPosition;
                            // Adjust to place the portal on the ground
                            _dreamGate.transform.localPosition -= new Vector3(0f,1.4f,0f);
                            HeroController.instance.transform.localPosition = _gatePosition;
                            _gatePosition = _dreamGate.transform.localPosition;
                        }
                    }
                })
            }
        });
        fsm.GetState("Dream Gate?").InsertAction(new Lambda(() =>
        {
            if (Active)
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
