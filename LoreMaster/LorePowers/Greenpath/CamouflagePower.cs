using LoreMaster.Enums;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.Greenpath;

public class CamouflagePower : Power
{
    #region Members

    private tk2dSprite _heroSprite;

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
    
    protected override void Enable() => LoreMaster.Instance.Handler.StartCoroutine(WaitForCamouflage());

    protected override void Disable()
    {
        LoreMaster.Instance.Handler.StopCoroutine(WaitForCamouflage());
        PlayerData.instance.isInvincible = false;
        _heroSprite.color = Color.white;
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
                PlayerData.instance.isInvincible = true;
            }
            else
            {
                _heroSprite.color = Color.white;
                PlayerData.instance.isInvincible = false;
            }
        }
    }

    #endregion
}
