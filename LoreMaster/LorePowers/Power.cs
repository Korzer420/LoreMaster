using LoreMaster.Enums;
using LoreMaster.Manager;
using System;
using UnityEngine;

namespace LoreMaster.LorePowers;

public abstract class Power
{
    #region Members

    protected bool _initialized;
    protected Coroutine _runningCoroutine;

    #endregion

    #region Constructors

    public Power(string powerName, Area area)
    {
        PowerName = powerName;
        Location = area;
        string powerType = GetType().Name;
        Hint = Properties.PowerHints.ResourceManager.GetString(powerType.Substring(0, powerType.Length - 5));
        Description = Properties.PowerDescriptions.ResourceManager.GetString(powerType.Substring(0, powerType.Length - 5));
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or set location in which the power is. This is used for the tracker and the logic regarding the <see cref="Tag"/>.
    /// </summary>
    public Area Location { get; protected set; }

    /// <summary>
    /// Gets or sets the name of the power.
    /// </summary>
    public string PowerName { get; set; }

    /// <summary>
    /// Gets or set the clear description of the power.
    /// <para/>Only used if <see cref="LoreMaster.UseHints"/> is <see langword="false"/>.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the hint of the power. It gets displayed after the lore text (or <see cref="CustomText"/> if set) ingame.
    /// <para/>Only used if <see cref="LoreMaster.UseHints"/> is <see langword="true"/>.
    /// </summary>
    public string Hint { get; protected set; }

    /// <summary>
    /// Gets or sets the custom text. If this is filled, it replaces the original text of the source.
    /// </summary>
    public string CustomText { get; set; }

    /// <summary>
    /// Gets or set the flag that indicates if this power is active.
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the tag, to determine how this power activation behave.
    /// </summary>
    public PowerTag Tag { get; set; } = PowerTag.Local;

    /// <summary>
    /// Gets or sets the default tag. This is used to toggle the power on/off in the menu.
    /// </summary>
    public PowerTag DefaultTag { get; set; } = PowerTag.Local;

    /// <summary>
    /// Gets or sets the action the power should execute on a scene change (gameplay scene only)
    /// </summary>
    public virtual Action SceneAction => () => { };

    /// <summary>
    /// Gets the action that should be executed on a scene change if the power is twisted. (gameplay scene only)
    /// </summary>
    public virtual Action TwistedSceneAction => () => { };

    /// <summary>
    /// Gets or sets the indicator, if fake damage is applied. This is used to prevent some powers to break the flower.
    /// </summary>
    public static bool FakeDamage { get; set; }

    /// <summary>
    /// Gets or sets the flag that indicates whether the power should stay twisted even if it is obtained.
    /// </summary>
    public bool StayTwisted { get; set; }

    /// <summary>
    /// Gets or sets the current state of powers.
    /// </summary>
    public PowerState State { get; set; }

    #endregion

    #region Control Methods

    /// <summary>
    /// Initialize the power. (Modifies fsm and get prefabs). This get's called the first time this power calls<see cref="Enable"/> once you entered a save file.
    /// </summary>
    protected virtual void Initialize() => _initialized = true;

    /// <summary>
    /// Called when powers disable themself entirely (so that <see cref="Initialize"/> will be called upon next activation).
    /// </summary>
    protected virtual void Terminate() { }

    /// <summary>
    /// Enables this power in the overworld.
    /// </summary>
    protected virtual void Enable() { }

    /// <summary>
    /// Disables this power in the overworld.
    /// </summary>
    protected virtual void Disable() { }

    /// <summary>
    /// Enables the twisted version of this power.
    /// </summary>
    protected virtual void TwistEnable() { }

    /// <summary>
    /// Disables the twisted version of this power.
    /// </summary>
    protected virtual void TwistDisable() { }

    #endregion

    #region Wrapper Methods

    /// <summary>
    /// Wrapper to initialize the power
    /// </summary>
    private bool InitializePower()
    {
        try
        {
            if (_initialized)
                return true;
            Initialize();
            _initialized = true;
            return true;
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error when initializing " + PowerName + ": " + exception.Message + exception.StackTrace);
        }
        return false;
    }

    /// <summary>
    /// Wrapper to activate the power and control the needed values.
    /// </summary>
    internal void EnablePower()
    {
        if (Active || Tag == PowerTag.Disable || Tag == PowerTag.Remove || !PowerManager.CanPowersActivate)
            return;
        try
        {
            TwistEnable();
            State = PowerState.Twisted;
            InitializePower();
            //if (InitializePower())
            //{
            //    Active = true;
            //    //Enable();
            //    //LoreMaster.Instance.LogDebug("Enabled " + PowerName);
            //}
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error while loading " + PowerName + ": " + exception.Message);
            LoreMaster.Instance.LogError(exception.StackTrace);
            Active = false;
        }
    }

    /// <summary>
    /// Wrapper to disable the power.
    /// </summary>
    /// <param name="backToMenu">This is used to tell <see cref="EnablePower"/> to do the initialize again, if you reload the game.</param>
    internal void DisablePower(bool backToMenu = false)
    {
        if (backToMenu)
            Terminate();
        _initialized = !backToMenu;
        if (!Active)
            return;
        try
        {
            if (_runningCoroutine != null)
                LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
            Disable();
            LoreMaster.Instance.LogDebug("Disabled " + PowerName);
            Active = false;
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error while disabling " + PowerName + ": " + exception.Message);
            LoreMaster.Instance.LogError("Error while loading " + PowerName + ": " + exception.Source);
            LoreMaster.Instance.LogError("Error while loading " + PowerName + ": " + exception.StackTrace);
        }
    } 

    #endregion
}
