using LoreMaster.Enums;
using LoreMaster.Helper;
using Modding;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes;

public class BagOfMushroomsPower : Power
{
    #region Members

    private bool _pressed;
    private int _selectedEffect;
    private int _activeEffect;
    private GameObject _mushroomBag;
    private Sprite _mushroomSprite;
    private Color[] _colors = new Color[4] { Color.white, Color.yellow, Color.red, Color.green };
    private int[] _lastMushrooms = new int[2] { -1, -1 };

    // Needed for mega mushroom
    private readonly string[] _nailInstances = new string[] { "Slash", "DownSlash", "UpSlash", "WallSlash", "Great Slash", "Dash Slash" };

    // Needed for mini mushroom
    private float _baseGravity = 0.79f;

    private Coroutine _recoverSoul;

    #endregion

    #region Constructors

    public BagOfMushroomsPower() : base("Bag of Mushrooms", Area.FungalWastes)
    {
        Hint = "Allows you to consume a yummy mushroom snack occasionly. The saturation may power you up. Caution: Can cause throw up if you eat too much of the same ones. Press quick map to select another and cdash + dash to consume the mushroom. " +
            "WARNING: The yellow one causes a nausea effect! If you don't want to use that, you can turn off the effect in the mod settings. Eating the yellow mushroom then, will only give a small effect.";
        Description = "Allows you to pick a mushroom to consume each 180 seconds. White shroom: Increases the speed of the game by 40%. Yellow shroom: Generates 20 soul each second, " +
            "but causes nausea. Red shroom: Gives you 4 extra health, heals you fully and increases your nail damage by 20%, but you can't dash. Green shroom: Makes you small, decrease the gravity by 50%" +
            " and doubles all damage taken. Taking the same mushroom twice in a row nerfs it's positive effect by 50%. Taking the same mushroom three times in a row, deals 2 damage to you instead. Press quick map to select another and cdash + dash to consume the mushroom. " +
            "WARNING: The yellow one causes a nausea effect! If you don't want to use that, you can turn off the effect in the mod settings. Eating the yellow mushroom then, will only give a small effect.";
        _mushroomSprite = SpriteHelper.CreateSprite("MushroomChoice");
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the flag that indicates if the player has eaten the same mushroom twice in row, which causes a weaker effect.
    /// </summary>
    public bool HasEatenTwice => _lastMushrooms[1] == _activeEffect;

    #endregion

    #region Event Handler

    /// <summary>
    /// Handles the hero updates event to update and taking the shrooms.
    /// </summary>
    private void ShroomControl()
    {
        if (_activeEffect == 0 && _mushroomBag.activeSelf && !_pressed && InputHandler.Instance.inputActions.quickMap.IsPressed)
        {
            _pressed = true;
            LoreMaster.Instance.Handler.StartCoroutine(ChangeChoice());
        }

        if (_selectedEffect != 0 && InputHandler.Instance.inputActions.superDash.IsPressed && InputHandler.Instance.inputActions.dash.IsPressed)
            _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(Saturation());
    }

    /// <summary>
    /// Event handler for the adrenaline mushroom.
    /// </summary>
    private void AdjustTimeScale()
    {
        if (Time.timeScale != 0)
            Time.timeScale = HasEatenTwice ? 1.2f : 1.4f;
    }

    /// <summary>
    /// Event handler for the cleansing mushroom.
    /// </summary>
    private void NauseaEffect(On.tk2dCamera.orig_UpdateCameraMatrix orig, tk2dCamera self)
    {
        orig(self);

        Camera cam = ReflectionHelper.GetField<tk2dCamera, Camera>(GameCameras.instance.tk2dCam, "_unityCamera");
        if (cam == null)
            return;

        Matrix4x4 matrix = cam.projectionMatrix;
        matrix.m01 += Mathf.Sin(Time.time * 1.25f) * 1f;
        matrix.m10 += Mathf.Sin(Time.time * 1.75f) * 1f;

        cam.projectionMatrix = matrix;
    }

    #region Mini Mushroom Handler

    /// <summary>
    /// Adjusts the gravity and scale.
    /// </summary>
    private void MiniMushroomAdjustments()
    {
        Rigidbody2D rigidbody2D = HeroController.instance.GetComponent<Rigidbody2D>();

        if (rigidbody2D.gravityScale == 0)
            return;

        // This is for jelly belly power
        if (_baseGravity == 0.64f)
            rigidbody2D.gravityScale = HeroController.instance.transitionState == GlobalEnums.HeroTransitionState.WAITING_TO_TRANSITION ? (HasEatenTwice ? .44f : .24f) : 0.79f;
        else
            rigidbody2D.gravityScale = HeroController.instance.transitionState == GlobalEnums.HeroTransitionState.WAITING_TO_TRANSITION ? (HasEatenTwice ? .59f : .39f) : 0.79f;
        float scale = HasEatenTwice ? .75f : 0.5f;
        HeroController.instance.transform.localScale = new Vector3(HeroController.instance.cState.facingRight ? scale * -1 : scale, scale, scale);
    }

    /// <summary>
    /// Doubles the taken damage.
    /// </summary>
    private int MiniMushroomDamage(int hazardType, int damageAmount) => damageAmount * 2;

    /// <summary>
    /// Makes sure the hero controller doesn't "fix" the scale back again.
    /// </summary>
    /// <param name="il"></param>
    private void ModifyScaleFix(ILContext il)
    {
        ILCursor cursor = new(il);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-1f) || instr.MatchLdcR4(1f)))
        {
            cursor.EmitDelegate(() => /* Used for mega mushroom _activeEffect == 3 ? (HasEatenTwice ? 1.1 : 1.2f) :*/ (HasEatenTwice ? 0.75f : 0.5f));
            cursor.Emit(OpCodes.Mul);
        }
    }

    #endregion

    #region Mega Mushroom Handler

    /// <summary>
    /// Increases the nail damage.
    /// </summary>
    private HitInstance BuffNail(HutongGames.PlayMaker.Fsm owner, HitInstance hit)
    {
        if (_nailInstances.Contains(hit.Source.name))
            hit.DamageDealt = Convert.ToInt16(hit.DamageDealt * (HasEatenTwice ? 1.1f : 1.2f));

        return hit;
    }

    /// <summary>
    /// Adjusts the max health.
    /// </summary>
    private int ModifyHealth(string name, int orig) => name.Equals(nameof(PlayerData.instance.maxHealthBase)) ? (HasEatenTwice ? orig + 2 : orig + 4) : orig;

    /// <summary>
    /// Prevents the player from c-dashing.
    /// </summary>
    private bool DisableCDash(On.HeroController.orig_CanSuperDash orig, HeroController self)
    {
        orig(self);
        return false;
    }

    /// <summary>
    /// Prevents the player from dashing.
    /// </summary>
    private bool DisableDash(On.HeroController.orig_CanDash orig, HeroController self)
    {
        orig(self);
        return false;
    }

    #endregion

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        _mushroomBag = new("Mushroom Bag");
        _mushroomBag.transform.SetParent(GameObject.Find("Knight").transform);
        _mushroomBag.transform.localScale = new(1f, 1f);
        _mushroomBag.transform.localPosition = new(.4f, .4f);
        _mushroomBag.AddComponent<SpriteRenderer>().sprite = _mushroomSprite;
        GameObject healthPrefab = GameObject.Find("_GameCameras/HudCamera/Hud Canvas/Health/Health 11");
        float space = healthPrefab.transform.localPosition.x - GameObject.Find("_GameCameras/HudCamera/Hud Canvas/Health/Health 10").transform.localPosition.x;

        for (int i = 1; i < 5; i++)
        {
            GameObject gameObject = GameObject.Instantiate(healthPrefab, GameObject.Find("_GameCameras/HudCamera/Hud Canvas/Health").transform);
            gameObject.name = "Health " + (i + 11);
            gameObject.LocateMyFSM("health_display").FsmVariables.FindFsmInt("Health Number").Value = i + 11;
            gameObject.transform.localPosition = new Vector3(healthPrefab.transform.localPosition.x + space * i, healthPrefab.transform.localPosition.y, healthPrefab.transform.localPosition.z);
        }
        _selectedEffect = 1;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        ModHooks.HeroUpdateHook += ShroomControl;
        _mushroomBag.SetActive(true);
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ModHooks.HeroUpdateHook -= ShroomControl;
        if (_activeEffect != 0)
            RevertMushroom();
        _activeEffect = 0;
        _selectedEffect = LoreMaster.Instance.Generator.Next(1, 5);
        _mushroomBag.GetComponent<SpriteRenderer>().color = _colors[_selectedEffect - 1];
        _mushroomBag.SetActive(false);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Changes the choice the player has made.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChangeChoice()
    {
        _selectedEffect++;
        if (_selectedEffect > 4)
            _selectedEffect = 1;
        _mushroomBag.GetComponent<SpriteRenderer>().color = _colors[_selectedEffect - 1];
        yield return new WaitForSeconds(.5f);
        _pressed = false;
    }

    /// <summary>
    /// Activate the mushroom effect.
    /// </summary>
    private void EatMushroom()
    {
        // Take damage instead, if you eat the same one thrice.
        if (_activeEffect == _lastMushrooms[0] && _activeEffect == _lastMushrooms[1])
        {
            HeroController.instance.TakeDamage(null, GlobalEnums.CollisionSide.top, 2, 1);
            _activeEffect = 8;
            return;
        }
        _pressed = true;
        try
        {
            switch (_activeEffect)
            {
                case 1:
                    AdrenalineMushroom(true);
                    break;
                case 2:
                    CleansingMushroom(true);
                    break;
                case 3:
                    MegaMushroom(true);
                    break;
                case 4:
                    MiniMushroom(true);
                    break;
                default:
                    break;
            }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Couldn't start saturation effect: " + exception.Message);
        }
    }

    /// <summary>
    /// Keeps the mushroom effect up and disables it after the time has passed.
    /// </summary>
    private IEnumerator Saturation()
    {
        _activeEffect = _selectedEffect;
        _mushroomBag.SetActive(false);
        _selectedEffect = 0;
        EatMushroom();
        float passedTime = 0f;
        while (passedTime <= 60f && _pressed)
        {
            yield return null;
            if (PlayerData.instance.GetBool(nameof(PlayerData.instance.atBench)))
                yield return new WaitUntil(() => !PlayerData.instance.GetBool(nameof(PlayerData.instance.atBench)));
            passedTime += Time.deltaTime;
        }

        RevertMushroom();

        yield return new WaitForSeconds(180f);
        _activeEffect = 0;
        _selectedEffect = LoreMaster.Instance.Generator.Next(1, 5);
        _mushroomBag.GetComponent<SpriteRenderer>().color = _colors[_selectedEffect - 1];
        _mushroomBag.SetActive(true);
    }

    /// <summary>
    /// Deactive the mushroom effect.
    /// </summary>
    private void RevertMushroom()
    {
        switch (_activeEffect)
        {
            case 1:
                AdrenalineMushroom(false);
                break;
            case 2:
                CleansingMushroom(false);
                break;
            case 3:
                MegaMushroom(false);
                break;
            case 4:
                MiniMushroom(false);
                break;
            default:
                return;
        }
        _lastMushrooms[0] = _lastMushrooms[1];
        _lastMushrooms[1] = _activeEffect;
        _pressed = false;
    }

    /// <summary>
    /// Toggles the adrenaline mushroom.
    /// </summary>
    /// <param name="active">If true, activates the effect.</param>
    private void AdrenalineMushroom(bool active)
    {
        if (active)
            ModHooks.HeroUpdateHook += AdjustTimeScale;
        else
        {
            ModHooks.HeroUpdateHook -= AdjustTimeScale;
            if (Time.timeScale != 0)
                Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Toggles the cleansing mushroom.
    /// </summary>
    /// <param name="active">If true, activates the effect.</param>
    private void CleansingMushroom(bool active)
    {
        if (active)
        {
            if (LoreMaster.Instance.DisableYellowMushroom)
                HeroController.instance.AddMPCharge(HasEatenTwice ? 5 : 10);
            else
            {
                _recoverSoul = LoreMaster.Instance.Handler.StartCoroutine(RecoverSoul());
                On.tk2dCamera.UpdateCameraMatrix += NauseaEffect;
            }
        }
        else
        {
            if (_recoverSoul != null)
                LoreMaster.Instance.Handler.StopCoroutine(_recoverSoul);
            On.tk2dCamera.UpdateCameraMatrix -= NauseaEffect;
        }
    }

    /// <summary>
    /// Toggles the mega mushroom.
    /// </summary>
    /// <param name="active">If true, activates the effect.</param>
    private void MegaMushroom(bool active)
    {
        if (active)
        {
            On.HeroController.CanDash += DisableDash;
            On.HeroController.CanSuperDash += DisableCDash;
            ModHooks.GetPlayerIntHook += ModifyHealth;
            ModHooks.HitInstanceHook += BuffNail;
        }
        else
        {
            On.HeroController.CanDash -= DisableDash;
            On.HeroController.CanSuperDash -= DisableCDash;
            ModHooks.GetPlayerIntHook -= ModifyHealth;
            ModHooks.HitInstanceHook -= BuffNail;
        }
        HeroController.instance.orig_CharmUpdate();
        PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
    }

    /// <summary>
    /// Toggles the mini mushroom.
    /// </summary>
    /// <param name="active">If true, activates the effect.</param>
    private void MiniMushroom(bool active)
    {
        if (active)
        {
            IL.HeroController.Update10 += ModifyScaleFix;
            ModHooks.AfterTakeDamageHook += MiniMushroomDamage;
            float scale = HasEatenTwice ? .75f : .5f;
            HeroController.instance.transform.localScale = new(HeroController.instance.cState.facingRight ? scale * -1 : scale, scale, scale);
            HeroController.instance.BIG_FALL_TIME += HasEatenTwice ? 33f : 66f;
            _baseGravity = HeroController.instance.GetComponent<Rigidbody2D>().gravityScale;
            // Tries to prevent being clipped in the ground.
            HeroController.instance.transform.localPosition += new Vector3(0f, 1f, 0f);
            ModHooks.HeroUpdateHook += MiniMushroomAdjustments;
        }
        else
        {
            IL.HeroController.Update10 -= ModifyScaleFix;
            ModHooks.AfterTakeDamageHook -= MiniMushroomDamage;
            ModHooks.HeroUpdateHook -= MiniMushroomAdjustments;
            HeroController.instance.transform.localScale = new(HeroController.instance.cState.facingRight ? -1f : 1f, 1f, 1f);
            HeroController.instance.BIG_FALL_TIME -= HasEatenTwice ? 33f : 66f;
            // To ensure the fall time is never under the default one.
            if (HeroController.instance.BIG_FALL_TIME < 3.3f)
                HeroController.instance.BIG_FALL_TIME = 3.3f;
            // Tries to prevent being clipped in the ground.
            HeroController.instance.transform.localPosition += new Vector3(0f, 1f, 0f);
            HeroController.instance.GetComponent<Rigidbody2D>().gravityScale = _baseGravity;
        }
    }

    /// <summary>
    /// Recover soul from the adrenaline mushroom effect.
    /// </summary>
    private IEnumerator RecoverSoul()
    {
        int soulGain = HasEatenTwice ? 10 : 20;
        while (_activeEffect == 2)
        {
            yield return new WaitForSeconds(1f);
            HeroController.instance.AddMPCharge(soulGain);
        }
    }

    #endregion
}
