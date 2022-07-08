using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using UnityEngine;

namespace LoreMaster.LorePowers.WhitePalace;

public class ShadowForgedPower : Power
{
    #region Members

    private GameObject _shadow;

    #endregion

    #region Constructors

    public ShadowForgedPower() : base("Shadow Forged",Area.WhitePalace)
    {
        Hint = "Your void energy return quicker to you.";
        Description = "Decrease the cooldown of shade cloak by 0.4 seconds and increases sharp shadow damage by 100%.";
    }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        _shadow = GameObject.Find("Knight/Effects").transform.Find("Shadow Recharge").gameObject;
        PlayMakerFSM fsm = _shadow.LocateMyFSM("Recharge Effect");

        fsm.GetState("Init").AddLastAction(new Lambda(() =>
        {
            if (Active)
                fsm.FsmVariables.GetFsmFloat("Wait time").Value -= .4f;
        }));

        fsm = GameObject.Find("Knight/Attacks").LocateMyFSM("Set Sharp Shadow Damage");
        fsm.GetState("Set").RemoveFirstActionOfType<SetFsmInt>();
        fsm.GetState("Set").AddLastAction(new Lambda(() =>
        {
            int damage = fsm.FsmVariables.FindFsmInt("nailDamage").Value;
            if (Active)
                damage *= 2;
            fsm.transform.Find("Sharp Shadow").gameObject.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt").Value = damage;

        }));
    }

    protected override void Enable()
    {
        HeroController.instance.SHADOW_DASH_COOLDOWN -= .4f;
        _shadow.GetComponent<tk2dSpriteAnimator>().ClipFps += 8f;
    }

    protected override void Disable()
    {
        HeroController.instance.SHADOW_DASH_COOLDOWN += .4f;
        _shadow.GetComponent<tk2dSpriteAnimator>().ClipFps -= 8f;
    }

    #endregion
}
