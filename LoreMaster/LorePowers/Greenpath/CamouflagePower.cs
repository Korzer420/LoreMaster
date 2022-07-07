using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.Greenpath;

public class CamouflagePower : Power
{
    #region Members

    private tk2dSprite _heroSprite;

    #endregion

    #region Constructors

    public CamouflagePower() : base("", Area.Greenpath) { }

    #endregion

    #region Public Methods

    protected override void Initialize()
    {
        _heroSprite = GameObject.Find("Knight").GetComponent<tk2dSprite>();
    }

    protected override void Enable() => LoreMaster.Instance.Handler.StartCoroutine(WaitForCamouflage());

    protected override void Disable()
    {
        HeroController.instance.StopCoroutine(WaitForCamouflage());
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
