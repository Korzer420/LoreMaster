using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.FogCanyon;

public class JellyBellyPower : Power
{
    #region Members

    private Rigidbody2D _playerRigidBody;

    #endregion

    #region Constructors

    public JellyBellyPower() : base("", Area.FogCanyon)
    {
        
        
    }

    #endregion

    #region Public Methods

    protected override void Initialize()
    {
        _playerRigidBody = HeroController.instance.gameObject.GetComponent<Rigidbody2D>();
    }

    protected override void Enable()
    {
        HeroController.instance.BIG_FALL_TIME *= 3;
        _playerRigidBody.gravityScale -= .25f;
    }

    protected override void Disable()
    {
        _playerRigidBody.gravityScale += .25f;
        HeroController.instance.BIG_FALL_TIME /= 3;
    }

    #endregion
}

