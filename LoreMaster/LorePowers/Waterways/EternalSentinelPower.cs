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

    private bool _grantedBonusHealth;

    private tk2dSprite _baldurSprite;

    #endregion

    #region Constructors

    public EternalSentinelPower() : base ("Eternal Sentinel",Area.WaterWays)
    {
        Hint = "Increases your durablity while wearing the sign of the protector. The shield of ancient one consumes soul and is more restistance.";
        Description = "Defender's Crest increase your max health by 2. Baldur shell now takes 10 hits instead of four. When getting hit, while baldur shell is up, you gain 10 soul "+
            "or 20 if you also wearing Defender's Crest.";
    }

    #endregion

    #region Event Handler

    private void CharmUpdate(PlayerData data, HeroController controller)
    {
        if (data.GetBool(nameof(data.equippedCharm_10)))
        {
            if (!_grantedBonusHealth)
                HeroController.instance.AddToMaxHealth(1);
            _grantedBonusHealth = true;
            PlayerData.instance.SetInt(nameof(PlayerData.instance.blockerHits), 10);
            _baldurSprite.color = new(1f, 0.4f, 0f);
            return;
        }

        if (_grantedBonusHealth)
            PlayerData.instance.SetInt(nameof(PlayerData.instance.maxHealth), PlayerData.instance.maxHealth - 1);
        _grantedBonusHealth = false;
        _baldurSprite.color = Color.white;
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
            // Refunds soul on baldur hit on break
            if (Active)
                HeroController.instance.AddMPCharge(PlayerData.instance.GetBool("equippedCharm_10") ? 20 : 10);
            baldurFSM.SendEvent(baldurFSM.FsmVariables.FindFsmInt("Blocks").Value.ToString());
        }));

        _baldurSprite = baldurShield.GetComponentInChildren<tk2dSprite>();
    }

    protected override void Enable()
    {
        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_10)))
        {
            _grantedBonusHealth = true;
            HeroController.instance.AddToMaxHealth(2);
            _baldurSprite.color = new(1f, 0.4f, 0f);
        }
        ModHooks.CharmUpdateHook += CharmUpdate;
    }

    protected override void Disable()
    {
        if (_grantedBonusHealth)
            PlayerData.instance.SetInt(nameof(PlayerData.instance.maxHealth), PlayerData.instance.maxHealth - 2);
        _grantedBonusHealth = false;
        ModHooks.CharmUpdateHook -= CharmUpdate;
        _baldurSprite.color = Color.white;
    }

    #endregion
}
