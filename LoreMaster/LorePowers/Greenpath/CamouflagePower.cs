using LoreMaster.Enums;
using Modding;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.Greenpath;

public class CamouflagePower : Power
{
    #region Members

    private tk2dSprite _heroSprite;
    private bool _isCamouflaged;

    #endregion

    #region Constructors

    public CamouflagePower() : base("Camouflage", Area.Greenpath) 
    {
        Hint = "While doing nothing, your mind slowly ascent to Unn's dream, while your shell is shielded by Unn's power.";
        Description = "After standing still for 5 seconds, you gain invincibility until you do something. If you are wearing Shape of Unn, you keep the invincibility while focusing and moving as a slug.";
    }

    #endregion

    #region Protected Methods

    protected override void Initialize() => _heroSprite = GameObject.Find("Knight").GetComponent<tk2dSprite>();

    protected override void Enable() 
    { 
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(WaitForCamouflage());
        On.HeroController.CanTakeDamage += HeroController_CanTakeDamage;
    }

    private bool HeroController_CanTakeDamage(On.HeroController.orig_CanTakeDamage orig, HeroController self)
    {
        bool result = orig(self);

        if (_isCamouflaged)
            return false;
        return result;
    }

    protected override void Disable()
    {
        LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
        _isCamouflaged = false;
        PlayerData.instance.SetBool(nameof(PlayerData.instance.isInvincible), false);
        _heroSprite.color = Color.white;
        On.HeroController.CanTakeDamage -= HeroController_CanTakeDamage;
    }

    #endregion

    #region Private Methods

    private IEnumerator WaitForCamouflage()
    {
        while (true)
        {
            float passedTime = 0f;
            while (passedTime < 5f)
            {
                yield return null;
                passedTime += Time.deltaTime;
                if (HeroController.instance.hero_state != GlobalEnums.ActorStates.idle || InputHandler.Instance.inputActions.attack.IsPressed
                    || InputHandler.Instance.inputActions.dash.IsPressed || InputHandler.Instance.inputActions.superDash.IsPressed
                    || InputHandler.Instance.inputActions.dreamNail.IsPressed || InputHandler.Instance.inputActions.quickCast.IsPressed
                    || ((InputHandler.Instance.inputActions.focus.IsPressed 
                    || InputHandler.Instance.inputActions.cast.IsPressed) && !PlayerData.instance.GetBool("equippedCharm_28")))
                    break;
            }
            if (passedTime >= 5f)
            {
                _heroSprite.color = Color.green;
                _isCamouflaged = true;
            }
            else
            {
                _heroSprite.color = Color.white;
                _isCamouflaged = false;
            }
        }
    }

    #endregion
}
