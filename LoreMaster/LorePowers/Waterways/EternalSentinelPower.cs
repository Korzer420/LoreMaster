using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.Waterways;

internal class EternalSentinelPower : Power
{
    #region Members

    private bool _grantedBonusHealth;

    private tk2dSprite _baldurSprite;

    #endregion

    #region Constructors

    public EternalSentinelPower() :base ("",Area.WaterWays)
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
            if (IsCurrentlyActive())
                HeroController.instance.AddMPCharge(PlayerData.instance.GetBool("equippedCharm_10") ? 20 : 10);
            baldurFSM.SendEvent(baldurFSM.FsmVariables.FindFsmInt("Blocks").Value.ToString());
        }));

        _baldurSprite = baldurShield.GetComponentInChildren<tk2dSprite>();
    }

    #endregion

    #region Public Methods

    public override void Enable()
    {
        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_10)))
        {
            _grantedBonusHealth = true;
            HeroController.instance.AddToMaxHealth(1);
            _baldurSprite.color = new(1f, 0.4f, 0f);
        }
        ModHooks.CharmUpdateHook += ModHooks_CharmUpdateHook;
    }

    public override void Disable()
    {
        if (_grantedBonusHealth)
            PlayerData.instance.SetInt(nameof(PlayerData.instance.maxHealth), PlayerData.instance.maxHealth - 2);
        _grantedBonusHealth = false;
        ModHooks.CharmUpdateHook -= ModHooks_CharmUpdateHook;
        _baldurSprite.color = Color.white;
    }

    #endregion

    #region Private Methods

    private void ModHooks_CharmUpdateHook(PlayerData data, HeroController controller)
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
}
