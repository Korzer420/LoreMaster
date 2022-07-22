using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using UnityEngine;

namespace LoreMaster.LorePowers.QueensGarden;

public class FollowTheLightPower : Power
{
    #region Members

    private GameObject _dreamGate;

    private Vector3 _gatePosition;

    #endregion

    #region Constructors

    public FollowTheLightPower() : base("Follow the Light", Area.QueensGarden)
    {
        Hint = "Allows you to weave with the pure essence of light to form a portal which you can travel to. Moving to far away will destroy the unstable portal. " +
            "Hold left while swinging the artifact of the light to weave the portal and hold right to travel through it. As long as the artifact has not its full potential, " +
            "travelling will consume the energy of the dreams and destroy the portal once entered. If not, the portal will remain at your entry point.";
        Description = "When you hold left while casting dreamnail, it will spawn an orange portal by your side, which you can travel to from anywhere in the room. Hold right while casting " +
            "dreamnail to warp to the portal. Warping will consume 1 essence and the portal. Neither get's consumed if you have awoken Dreamnail (the portal will swap position with you instead).";
    }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        var fsm = HeroController.instance.gameObject.LocateMyFSM("Dream Nail");
        LoreMaster.Instance.Log("Try to find Dream Gate");
        GameObject dreamGate = fsm.GetState("Spawn Gate").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
        LoreMaster.Instance.Log("Add states");
        fsm.AddState(new(fsm.Fsm)
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
                        _dreamGate.transform.localPosition -= new Vector3(0f,1f,0f);
                        _gatePosition = HeroController.instance.transform.localPosition;
                    }
                })
            }
        });
        fsm.AddState(new(fsm.Fsm)
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
                            HeroController.instance.transform.localPosition = _gatePosition;
                            _dreamGate.transform.localPosition = HeroController.instance.transform.localPosition;
                            _gatePosition = HeroController.instance.transform.localPosition;
                        }
                    }
                })
            }
        });
        LoreMaster.Instance.Log("Insert Action transitions");
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
        LoreMaster.Instance.Log("Adjust transitions from Dream Gate?");
        fsm.GetState("Dream Gate?").AddTransition("PLACE", "Set Light Gate");
        fsm.GetState("Dream Gate?").AddTransition("MOVE", "Move to Light Gate");
        LoreMaster.Instance.Log("Set transition from Set Light");
        fsm.GetState("Set Light Gate").AddTransition("FINISHED", "Slash Antic");
        LoreMaster.Instance.Log("Set transition from Move To Light");
        fsm.GetState("Move to Light Gate").AddTransition("FINISHED", "Slash Antic");
    }

    #endregion
}
