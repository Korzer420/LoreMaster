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

    public EternalSentinelPower() : base("Eternal Sentinel", Area.WaterWays)
    {
        Hint = "Increases your scent while wearing the sign of the protector. The shield of the ancient ones is more resistent and gather soul with the blessing of the protector.";
        Description = "Defender's Crest clouds are 150% bigger and tick twice as fast. Baldur shell now takes seven hits instead of four (ten with defender's Crest. When getting hit, while baldur shell is up, you gain 15 soul " +
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

        PlayerData.instance.SetInt(nameof(PlayerData.instance.blockerHits), 7);
        _baldurSprite.color = Color.white;
    }

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (self.gameObject.name.Equals("Knight Dung Trail(Clone)") && self.FsmName.Equals("Control"))
            self.GetState("Init").ReplaceAction(new Lambda(() =>
            {
                self.transform.localPosition = HeroController.instance.transform.position;
                if (Active)
                    self.transform.localScale = new(2.5f, 2.5f);
                else
                    self.transform.localScale = new(1f, 1f);
                self.GetComponent<DamageEffectTicker>().SetDamageInterval(Active ? .3f : .15f);
            })
            { Name = "Extend Cloud" });
        
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
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_10)))
            _baldurSprite.color = new(1f, 0.4f, 0f);
        ModHooks.CharmUpdateHook += CharmUpdate;
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ModHooks.CharmUpdateHook -= CharmUpdate;
        _baldurSprite.color = Color.white;
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
        if (PlayerData.instance.GetInt(nameof(PlayerData.instance.blockerHits)) > 4)
            PlayerData.instance.SetInt(nameof(PlayerData.instance.blockerHits), 4);
    }

    #endregion
}
