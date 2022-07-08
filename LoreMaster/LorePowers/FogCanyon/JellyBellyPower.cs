using LoreMaster.Enums;
using UnityEngine;

namespace LoreMaster.LorePowers.FogCanyon;

public class JellyBellyPower : Power
{
    #region Members

    private Rigidbody2D _playerRigidBody;

    #endregion

    #region Constructors

    public JellyBellyPower() : base("Belly of the Jelly(fish)", Area.FogCanyon)
    {
        CustomText = "Aren't my jelly fish cute little things? The way the float in the air and fall so slowly, it has something... calming to it. I wish I could navigate to the air like that.";
        Hint = "You are feeling light, like a feather";
        Description = "Decrease your falling speed by 25% and triples the time needed in air, for a hard fall.";
    }

    #endregion

    #region Protected Methods

    protected override void Initialize() => _playerRigidBody = HeroController.instance.gameObject.GetComponent<Rigidbody2D>();
    
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

