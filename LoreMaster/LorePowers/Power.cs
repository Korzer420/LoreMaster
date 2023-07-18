using LoreMaster.Enums;
using LoreMaster.Manager;
using System;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers;

public abstract class Power
{
    #region Members

    protected bool _initialized = false;
    protected Coroutine _runningCoroutine;

    #endregion

    #region Constructors

    public Power(string powerName, Area area)
    {
        PowerName = powerName;
        Location = area;
        string powerType = GetType().Name;
        Hint = Properties.PowerHints.ResourceManager.GetString(powerType.Substring(0, powerType.Length - 5));
        TwistedHint = "<color=#c034eb>"+ Properties.TwistedPowerHints.ResourceManager.GetString(powerType.Substring(0, powerType.Length - 5)) + "</color>";
        Description = Properties.PowerDescriptions.ResourceManager.GetString(powerType.Substring(0, powerType.Length - 5));
        TwistedDescription = "<color=#c034eb>"+Properties.TwistedPowerDescriptions.ResourceManager.GetString(powerType.Substring(0, powerType.Length - 5)) + "</color>";
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
    /// Gets or sets the clear description of the twisted version of this power.
    /// </summary>
    public string TwistedDescription { get; set; }

    /// <summary>
    /// Gets or sets the hint of the power. It gets displayed after the lore text (or <see cref="CustomText"/> if set) ingame.
    /// <para/>Only used if <see cref="LoreMaster.UseHints"/> is <see langword="true"/>.
    /// </summary>
    public string Hint { get; protected set; }

    /// <summary>
    /// Gets or sets the twisted hint of the power.
    /// </summary>
    public string TwistedHint { get; set; }

    /// <summary>
    /// Gets or sets the custom text. If this is filled, it replaces the original text of the source.
    /// </summary>
    public string CustomText { get; set; }

    /// <summary>
    /// Gets or sets the tag, to determine how this power activation behave.
    /// </summary>
    public PowerRank Rank { get; set; } = PowerRank.Lower;

    /// <summary>
    /// Gets or sets the action the power should execute on a scene change (gameplay scene only)
    /// </summary>
    public virtual Action SceneAction => () => { };

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
    protected virtual void Initialize() { }

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

    /// <summary>
    /// Determines how the power might modify enemies.
    /// If left untouched, a random modifier is applied.
    /// </summary>
    protected virtual void EnemyBuff(HealthManager enemy) => ApplyRandomBuff(enemy);

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
        if (State == PowerState.Active || !PowerManager.CanPowersActivate)
            return;
        try
        {
            if (InitializePower())
            {
                if ((!PowerManager.ObtainedPowers.Contains(this) && State != PowerState.Twisted && (SettingManager.Instance.GameMode != GameMode.Normal && SettingManager.Instance.GameMode != GameMode.Disabled))
                    || (State == PowerState.Disabled && PowerManager.ObtainedPowers.Contains(this) && StayTwisted))
                {
                    TwistEnable();
                    State = PowerState.Twisted;
                }
                else if (State == PowerState.Twisted && PowerManager.ObtainedPowers.Contains(this) && !StayTwisted)
                {
                    TwistDisable();
                    if (SettingManager.Instance.GameMode == GameMode.Hard)
                        Enable();
                    State = PowerState.Active;
                }
                else if (State == PowerState.Disabled && PowerManager.ObtainedPowers.Contains(this) && !StayTwisted)
                {
                    if (SettingManager.Instance.GameMode != GameMode.Heroic && SettingManager.Instance.GameMode != GameMode.Disabled)
                        Enable();
                    State = PowerState.Active;
                }
                else
                    return;
                LoreMaster.Instance.LogDebug("Activated " + PowerName);
            }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error while loading " + PowerName + ": " + exception.Message);
            LoreMaster.Instance.LogError(exception.StackTrace);
            State = PowerState.Disabled;
        }
    }

    /// <summary>
    /// Wrapper to disable the power.
    /// </summary>
    /// <param name="backToMenu">This is used to tell <see cref="EnablePower"/> to do the initialize again, if you reload the game.</param>
    internal void DisablePower(bool backToMenu = false)
    {
        if (State != PowerState.Disabled)
        {
            try
            {
                if (_runningCoroutine != null)
                    LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
                // In heroic mode, powers fake to be active, which is why we ignore them in those cases.
                if (State == PowerState.Active && (SettingManager.Instance.GameMode != GameMode.Heroic && SettingManager.Instance.GameMode != GameMode.Disabled))
                    Disable();
                else if (State == PowerState.Twisted)
                    TwistDisable();
                LoreMaster.Instance.LogDebug("Disabled " + PowerName);
                State = PowerState.Disabled;
            }
            catch (Exception exception)
            {
                LoreMaster.Instance.LogError("Error while disabling " + PowerName + ": " + exception.Message);
                LoreMaster.Instance.LogError("Error while loading " + PowerName + ": " + exception.Source);
                LoreMaster.Instance.LogError("Error while loading " + PowerName + ": " + exception.StackTrace);
            }
        }
        if (backToMenu)
        { 
            Terminate();
            _initialized = false;
        }
    }

    internal void CastOnEnemy(HealthManager enemy) => EnemyBuff(enemy);

    #endregion

    #region Extras

    /// <summary>
    /// Start a coroutine on this power. This coroutine will be cancelled once the power disables itself.
    /// </summary>
    /// <param name="coroutine"></param>
    /// <param name="overwrite"></param>
    protected void StartRoutine(Func<IEnumerator> coroutine, bool overwrite = true)
    {
        if (_runningCoroutine != null)
            if (overwrite)
                LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
            else
            {
                LoreMaster.Instance.LogDebug($"Power {PowerName} tried starting a main coroutine while one is already active.");
                return;
            }
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(coroutine.Invoke());
    }

    /// <summary>
    /// Check if a random buff is applied to the enemy.
    /// </summary>
    /// <param name="healthManager"></param>
    private void ApplyRandomBuff(HealthManager healthManager)
    {
        int rolled = UnityEngine.Random.Range(1, 101);
        if (healthManager.hp <= 1 || rolled <= 30)
            return;
        else if (rolled <= 55)
            healthManager.hp = Convert.ToInt32(healthManager.hp * UnityEngine.Random.Range(1.25f, 3.5f));
        else if (rolled <= 80)
        {
            DamageHero damageComponent = healthManager.GetComponent<DamageHero>();
            if (damageComponent is not null)
                damageComponent.damageDealt *= 2;
        }
        else
            healthManager.transform.localScale = new Vector3(healthManager.transform.localScale.x * 1.2f, healthManager.transform.localScale.y * 1.2f, healthManager.transform.localScale.z);
    }

    #endregion
}
