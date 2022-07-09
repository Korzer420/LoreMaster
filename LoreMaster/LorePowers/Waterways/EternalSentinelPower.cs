using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using Modding;
using UnityEngine;

namespace LoreMaster.LorePowers.Waterways;

public class EternalSentinelPower : Power
{
    #region Members

    private tk2dSprite _baldurSprite;

    #endregion

    #region Constructors

    public EternalSentinelPower() : base("Eternal Sentinel", Area.WaterWays)
    {
        Hint = "Increases your durablity while wearing the sign of the protector. The shield of ancient one consumes soul and is more restistance.";
        Description = "Defender's Crest clouds are 150% bigger and tick twice as fast. Baldur shell now takes 10 hits instead of four. When getting hit, while baldur shell is up, you gain 15 soul " +
            "if you are also wearing Defender's Crest.";
    }

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

        _baldurSprite.color = Color.white;
    }

    #endregion

    #region Event Handler

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (self.gameObject.name.Equals("Knight Dung Trail(Clone)") && self.FsmName.Equals("Control"))
        {
            FsmState waitState = self.GetState("Init");
            if (waitState.GetFirstActionOfType<Lambda>() == null)
            {
                waitState.RemoveAction(1);
                waitState.AddLastAction(new Lambda(() =>
                {
                    self.transform.localPosition = HeroController.instance.transform.position;
                    if (Active)
                        self.transform.localScale = new(2.5f, 2.5f);
                    else
                        self.transform.localScale = new(1f, 1f);
                    self.GetComponent<DamageEffectTicker>().SetDamageInterval(Active ? .3f : .15f);
                }));
            }
        }

        orig(self);
    }

    #endregion

    #region Protected Methods

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
            LoreMaster.Instance.Log("Current hits: " + baldurFSM.FsmVariables.FindFsmInt("Blocks").Value);
            // Refunds soul on baldur hit on break
            if (Active && PlayerData.instance.GetBool("equippedCharm_10"))
                HeroController.instance.AddMPCharge(15);
            baldurFSM.SendEvent(baldurFSM.FsmVariables.FindFsmInt("Blocks").Value.ToString());
        }));

        _baldurSprite = baldurShield.GetComponentInChildren<tk2dSprite>();
    }

    protected override void Enable()
    {
        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_10)))
            _baldurSprite.color = new(1f, 0.4f, 0f);
        ModHooks.CharmUpdateHook += CharmUpdate;
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
    }

    protected override void Disable()
    {
        ModHooks.CharmUpdateHook -= CharmUpdate;
        _baldurSprite.color = Color.white;
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
    }

    #endregion
}
