using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.FsmStateActions;
using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.UnityComponents;
using Modding;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace LoreMaster.LorePowers.Dirtmouth;

public class TrueFormPower : Power
{
    #region Members

    private int _shadeState = 0;

    private GameObject[] _attachObjects = new GameObject[5];

    private int _playerGeo = 0;

    private Transform _exit;

    #endregion

    #region Constructors

    public TrueFormPower() : base("True Form", Area.Dirtmouth) { }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public override Action SceneAction => () =>
    {
        if (!PlayerData.instance.GetString(nameof(PlayerData.instance.shadeScene)).Equals("None"))
        {
            float multiplier = .25f;
            // If we are going in the shade room, we increase the range buff
            if (PlayerData.instance.GetString(nameof(PlayerData.instance.shadeScene)).Equals(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
            {
                // If we have been already in the shade room (like dying in a room with the save bench), we ignore the multiplier.
                if (_shadeState == 2)
                    return;
                multiplier *= _shadeState == 1 ? 1 : 2;
                _shadeState = 2;
            }
            else if (_shadeState != 1)
            {
                // Depending where we are getting from, we lower or increase the range
                multiplier *= _shadeState == 2 ? -1 : 1;
                _shadeState = 1;
            }
            else
                return;
            ModifyNailLength(multiplier);
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            return;
        }
    };

    public override PowerRank Rank => PowerRank.Medium;

    #endregion

    #region Event Handler

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (self.gameObject.name.Contains("Hollow Shade Death") && string.Equals(self.FsmName, "Shade Control"))
            self.GetState("Blow").AddActions(() =>
            {
                ModifyNailLength(-.5f);
                _shadeState = 0;
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            });
        orig(self);
    }

    private int NailDamageUpdate(string name, int orig)
    {
        if (name.Equals("nailDamage"))
        {
            int dmgIncrease = 0;
            if (_shadeState != 0)
                dmgIncrease = Convert.ToInt32(orig * (_shadeState == 1 ? .3f : .6f));
            orig += dmgIncrease;
        }

        return orig;
    }

    private void CreateObject_OnEnter(On.HutongGames.PlayMaker.Actions.CreateObject.orig_OnEnter orig, CreateObject self)
    {
        orig(self);
        if (self.IsCorrectContext("Shade Control", "Fleeing Shade", "Killed"))
        {
            PlayMakerFSM deathFsm = self.Fsm.Variables.FindFsmGameObject("Corpse").Value.LocateMyFSM("Shade Control");
            FsmState state = deathFsm.GetState("Death Start");
            state.RemoveFirstAction<SendMessage>(); // Remove soul limiter handling
            state.ReplaceAction(1, () => deathFsm.FsmVariables.FindFsmInt("Geo").Value = _playerGeo);
        }
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        ModHooks.GetPlayerIntHook += NailDamageUpdate;
        SceneAction.Invoke();
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
        ModHooks.GetPlayerIntHook -= NailDamageUpdate;
        if (_shadeState != 0)
            ModifyNailLength(_shadeState == 1 ? -.25f : -.5f);
        _shadeState = 0;
        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(FleeingShade());
        On.HutongGames.PlayMaker.Actions.CreateObject.OnEnter += CreateObject_OnEnter;
    }

    /// <inheritdoc/>
    protected override void TwistDisable() 
    { 
        On.HutongGames.PlayMaker.Actions.CreateObject.OnEnter -= CreateObject_OnEnter;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Change the nail hit ranges.
    /// </summary>
    /// <param name="multiplier"></param>
    private void ModifyNailLength(float multiplier)
    {
        try
        {
            for (int index = 0; index < 4; index++)
            {
                Vector3 currentScale = _attachObjects[index].GetComponent<NailSlash>().scale;
                _attachObjects[index].GetComponent<NailSlash>().scale = new Vector3(currentScale.x + multiplier, currentScale.y + multiplier, currentScale.z + multiplier);
            }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Couldn't update nail length: " + exception.Message);
        }
    }

    private IEnumerator FleeingShade()
    {
        float passedTime = 0f;
        GameObject shade;
        while (true)
        {
            if (PlayerData.instance.GetBool("atBench") || !HeroController.instance.acceptingInput)
                yield return new WaitUntil(() => !PlayerData.instance.GetBool("atBench") && HeroController.instance.acceptingInput);
            passedTime += Time.deltaTime;
            if (passedTime >= 120f)
            {
                passedTime = 0f;
                _exit = HeroController.instance?.sceneEntryGate?.transform;
                if (_exit == null)
                    continue;
                _playerGeo = PlayerData.instance.GetInt("geo");
                HeroController.instance.TakeGeo(PlayerData.instance.GetInt("geo"));
                shade = GameObject.Instantiate(GameManager.instance.sm.hollowShadeObject);
                shade.SetActive(true);
                shade.name = "Fleeing Shade";
                shade.transform.position = HeroController.instance.transform.position + new Vector3(0f, 2f, 0f);

                FsmState state = shade.LocateMyFSM("Shade Control").GetState("Fly");
                state.ClearTransitions();
                state.RemoveFirstAction<FaceObject>();
                state.RemoveFirstAction<ChaseObject>();
                state.RemoveFirstAction<ChaseObjectV2>();
                shade.transform.SetScaleX(_exit.transform.position.x < shade.transform.position.x ? 1 : -1);
                shade.AddComponent<IgnoreTerrain>();
                LoreMaster.Instance.Handler.StartCoroutine(MoveShadeToExit(shade.transform, _exit, 2f));
                yield return new WaitUntil(() => shade == null);
            }
            yield return null;
        }
    }

    private IEnumerator MoveShadeToExit(Transform shade, Transform exit, float speed)
    {
        if (speed > 2f)
            speed = 2f;
        while (shade != null)
        {
            shade.transform.position = Vector3.MoveTowards(shade.transform.position, exit.position, speed * Time.deltaTime);
            if(Vector3.Distance(shade.transform.position, exit.position) < 1f)
            {
                float fleeTimer = 0f;
                while(shade != null && fleeTimer < 5f)
                {
                    fleeTimer += Time.deltaTime;
                    yield return null;
                }
                if (fleeTimer >= 5f)
                    GameObject.Destroy(shade.gameObject);
            }
            yield return null;
        }
    }

    #endregion
}
