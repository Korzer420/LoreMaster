using LoreMaster.Enums;
using Modding;
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
        Hint = "You are feeling light, like a feather.";
        Description = "Decrease your falling speed by about 20% and triples the time needed in air, for a hard fall.";
    }

    #endregion

    #region Event Handler

    private void Float()
    {
        if (_playerRigidBody.gravityScale == 0)
            return;
        _playerRigidBody.gravityScale = HeroController.instance.transitionState == GlobalEnums.HeroTransitionState.WAITING_TO_TRANSITION ? 0.64f : 0.79f;
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize() => _playerRigidBody = HeroController.instance.gameObject.GetComponent<Rigidbody2D>();

    /// <inheritdoc/>
    protected override void Enable()
    {
        HeroController.instance.BIG_FALL_TIME += 2.2f;
        ModHooks.HeroUpdateHook += Float;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ModHooks.HeroUpdateHook -= Float;
        HeroController.instance.BIG_FALL_TIME -= 2.2f;
        if (HeroController.instance.BIG_FALL_TIME < 1.1f)
            HeroController.instance.BIG_FALL_TIME = 1.1f;
        _playerRigidBody.gravityScale = .79f;
    }

    #endregion
}

