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

    #endregion

    #region Constructors

    public BagOfMushroomsPower() : base("Bag of Mushrooms", Area.FungalWastes)
    {
        Hint = "[BETA] Allows you to consume a yummy mushroom snack occasionly. The saturation may power you up. Caution: Can cause throw up if you eat too much of the same ones. Press CDash to select another and quick map to consume the mushroom.";
        Description = "[BETA] Allows you to pick a mushroom to consume each 180 seconds. White shroom: Increases the speed of the game by 40%. Yellow shroom: Generates 8 soul each second, " +
            "but causes nausea. Red shroom: Gives you 4 extra health, heals you fully and increases your nail damage by 20%, but you can't dash. Green shroom: Makes you small, decrease the gravity by 50%" +
            " and doubles all damage taken. Taking the same mushroom twice in a row nerfs it's positive effect by 50%. Taking the same mushroom three times in a row, deals 2 damage to you instead. Press CDash to select another and quick map to consume the mushroom.";
        _mushroomSprite = SpriteHelper.CreateSprite("MushroomChoice");
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the flag that indicates if the player has eaten the same mushroom twice in row, which causes a weaker effect.
    /// </summary>
    public bool HasEatenTwice => _lastMushrooms[1] == _activeEffect;

    #endregion

    #region Private Methods

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
    }

    protected override void Enable()
    {
        ModHooks.HeroUpdateHook += ShroomControl;
        _mushroomBag.SetActive(true);
    }

    protected override void Disable()
    {
        HeroController.instance.orig_CharmUpdate();
        ModHooks.HeroUpdateHook -= ShroomControl;
        LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
        RevertMushroom();
        _activeEffect = 0;
        _selectedEffect = 0;
        _pressed = false;
        _lastMushrooms[0] = -1;
        _lastMushrooms[1] = -1;
        _mushroomBag.SetActive(false);
    }

    private void ShroomControl()
    {

        if (_activeEffect == 0 && !_pressed && InputHandler.Instance.inputActions.superDash.IsPressed)
        {
            _pressed = true;
            LoreMaster.Instance.Handler.StartCoroutine(ChangeChoice());
        }

        if (_selectedEffect != 0 && InputHandler.Instance.inputActions.quickMap.IsPressed)
            _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(Saturation());
    }

    private IEnumerator ChangeChoice()
    {
        _selectedEffect++;
        if (_selectedEffect > 4)
            _selectedEffect = 1;
        _mushroomBag.GetComponent<SpriteRenderer>().color = _colors[_selectedEffect - 1];
        yield return new WaitForSeconds(.5f);
        _pressed = false;
    }

    private void EatMushroom()
    {
        if (_activeEffect == _lastMushrooms[0] && _activeEffect == _lastMushrooms[1])
        {
            HeroController.instance.TakeDamage(null, GlobalEnums.CollisionSide.top, 2, 1);
            _activeEffect = 8;
            return;
        }
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

    private IEnumerator Saturation()
    {
        _activeEffect = _selectedEffect;
        _mushroomBag.SetActive(false);
        _selectedEffect = 0;
        EatMushroom();
        float passedTime = 0f;
        float playerScale = 1f;

        //if (_activeEffect == 3)
        //    playerScale = HasEatenTwice ? 1.25f : 1.5f;
        //else
        if (_activeEffect == 4)
            playerScale = HasEatenTwice ? 0.75f : 0.5f;
        while (passedTime <= 60f)
        {
            yield return null;
            if (PlayerData.instance.GetBool(nameof(PlayerData.instance.atBench)))
                yield return new WaitUntil(() => !PlayerData.instance.GetBool(nameof(PlayerData.instance.atBench)));
            if (playerScale != 1)
                HeroController.instance.transform.localScale = new Vector3(HeroController.instance.cState.facingRight ? playerScale * -1 : playerScale, playerScale, playerScale);
            passedTime += Time.deltaTime;
        }

        RevertMushroom();

        yield return new WaitForSeconds(180f);
    }

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
                break;
        }
        _lastMushrooms[0] = _lastMushrooms[1];
        _lastMushrooms[1] = _activeEffect;
        _activeEffect = 0;
        _selectedEffect = LoreMaster.Instance.Generator.Next(1, 5);
        _mushroomBag.GetComponent<SpriteRenderer>().color = _colors[_selectedEffect - 1];
        _mushroomBag.SetActive(true);
    }

    private void AdrenalineMushroom(bool active)
    {
        float timeScaleValue = HasEatenTwice ? .2f : .4f;
        Time.timeScale += active ? timeScaleValue : timeScaleValue * -1f;
    }

    private void CleansingMushroom(bool active)
    {
        if (active)
        {
            LoreMaster.Instance.Handler.StartCoroutine(RecoverSoul());
            On.tk2dCamera.UpdateCameraMatrix += NauseaEffect;
        }
        else
        {
            LoreMaster.Instance.Handler.StopCoroutine("RecoverSoul");
            On.tk2dCamera.UpdateCameraMatrix -= NauseaEffect;
        }
    }

    private void MegaMushroom(bool active)
    {
        if (active)
        {
            //IL.HeroController.Update10 += ModifyScalePatch;
            On.HeroController.CanDash += DisableDash;
            On.HeroController.CanSuperDash += DisableCDash;
            ModHooks.GetPlayerIntHook += ModifyHealth;
            ModHooks.HitInstanceHook += ModHooks_HitInstanceHook;
            // Small try to prevent clipping into the ground
            //HeroController.instance.transform.localPosition += new Vector3(0f, .25f);
            //float scale = HasEatenTwice ? 1.1f : 1.2f;
            //HeroController.instance.transform.localScale = new(HeroController.instance.cState.facingRight ? -scale: scale, scale, scale);
        }
        else
        {
            //IL.HeroController.Update10 -= ModifyScalePatch;
            On.HeroController.CanDash -= DisableDash;
            On.HeroController.CanSuperDash -= DisableCDash;
            ModHooks.GetPlayerIntHook -= ModifyHealth;
            ModHooks.HitInstanceHook -= ModHooks_HitInstanceHook;
            //HeroController.instance.transform.localScale = new(HeroController.instance.cState.facingRight ? -1f : 1f, 1f, 1f);
        }
        HeroController.instance.orig_CharmUpdate();
        PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
    }

    private HitInstance ModHooks_HitInstanceHook(HutongGames.PlayMaker.Fsm owner, HitInstance hit)
    {
        if (_nailInstances.Contains(hit.Source.name))
            hit.DamageDealt = Convert.ToInt16(hit.DamageDealt * (HasEatenTwice ? 1.1f : 1.2f));
        return hit;
    }

    private void MiniMushroom(bool active)
    {
        if (active)
        {
            IL.HeroController.Update10 += ModifyScalePatch;
            ModHooks.AfterTakeDamageHook += ModHooks_AfterTakeDamageHook;
            float scale = HasEatenTwice ? .75f : .5f;
            HeroController.instance.transform.localScale = new(HeroController.instance.cState.facingRight ? scale * -1 : scale, scale, scale);
            HeroController.instance.BIG_FALL_TIME *= HasEatenTwice ? 10 : 20;
            HeroController.instance.GetComponent<Rigidbody2D>().gravityScale -= HasEatenTwice ? .25f : .5f;
        }
        else
        {
            IL.HeroController.Update10 -= ModifyScalePatch;
            ModHooks.AfterTakeDamageHook -= ModHooks_AfterTakeDamageHook;
            HeroController.instance.transform.localScale = new(HeroController.instance.cState.facingRight ? -1f : 1f, 1f, 1f);
            HeroController.instance.BIG_FALL_TIME /= HasEatenTwice ? 10 : 20;
            HeroController.instance.GetComponent<Rigidbody2D>().gravityScale += HasEatenTwice ? .25f : .5f;
        }
    }

    private IEnumerator RecoverSoul()
    {
        int soulGain = HasEatenTwice ? 4 : 8;
        while (_activeEffect == 2)
        {
            yield return new WaitForSeconds(1f);
            HeroController.instance.AddMPCharge(soulGain);
        }
    }

    private int ModHooks_AfterTakeDamageHook(int hazardType, int damageAmount) => damageAmount * 2;

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

    private int ModifyHealth(string name, int orig) => name.Equals(nameof(PlayerData.instance.maxHealthBase)) ? (HasEatenTwice ? orig + 2 : orig + 4) : orig;

    private bool DisableCDash(On.HeroController.orig_CanSuperDash orig, HeroController self)
    {
        orig(self);
        return false;
    }

    private bool DisableDash(On.HeroController.orig_CanDash orig, HeroController self)
    {
        orig(self);
        return false;
    }

    private void ModifyScalePatch(ILContext il)
    {
        ILCursor cursor = new(il);

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-1f) || instr.MatchLdcR4(1f)))
        {
            cursor.EmitDelegate(() => /* Used for mega mushroom _activeEffect == 3 ? (HasEatenTwice ? 1.1 : 1.2f) :*/ (HasEatenTwice ? 0.75f : 0.5f));
            cursor.Emit(OpCodes.Mul);
        }
    }

    #endregion
}
