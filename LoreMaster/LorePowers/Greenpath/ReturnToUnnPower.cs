using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.LorePowers.Greenpath;

public class ReturnToUnnPower : Power
{
    #region Members

    private bool _movementSpeedBuff;

    #endregion

    #region Constructors

    public ReturnToUnnPower() : base("", Area.Greenpath)
    {
       
    }

    #endregion

    #region Public Methods

    public override void Enable()
    {
        HeroController.instance.StartCoroutine(AdjustMovementSpeed());
    }

    private IEnumerator AdjustMovementSpeed()
    {
        while(true)
        {
            yield return null;
            if (!HeroController.instance.cState.facingRight)
            {
                if (!_movementSpeedBuff)
                {
                    HeroController.instance.WALK_SPEED += 3f;
                    HeroController.instance.RUN_SPEED += 3f;
                    HeroController.instance.RUN_SPEED_CH += 3f;
                    HeroController.instance.RUN_SPEED_CH_COMBO += 3f;
                    HeroController.instance.DASH_COOLDOWN -= .5f;
                    HeroController.instance.DASH_COOLDOWN_CH -= .5f;
                }
                _movementSpeedBuff = true;
            }
            else if (_movementSpeedBuff)
            { 
                HeroController.instance.WALK_SPEED -= 3f;
                HeroController.instance.RUN_SPEED -= 3f;
                HeroController.instance.RUN_SPEED_CH -= 3f;
                HeroController.instance.RUN_SPEED_CH_COMBO -= 3f;
                HeroController.instance.DASH_COOLDOWN += .5f;
                HeroController.instance.DASH_COOLDOWN_CH += .5f;
                _movementSpeedBuff = false;
            }
        }
    }

    public override void Disable()
    {
        HeroController.instance.StopCoroutine(AdjustMovementSpeed());
        if (_movementSpeedBuff)
        { 
            HeroController.instance.WALK_SPEED -= 3f;
            HeroController.instance.RUN_SPEED -= 3f;
            HeroController.instance.RUN_SPEED_CH -= 3f;
            HeroController.instance.RUN_SPEED_CH_COMBO -= 3f;
            HeroController.instance.DASH_COOLDOWN += .5f;
            HeroController.instance.DASH_COOLDOWN_CH += .5f;
        }
    }

    #endregion
}

