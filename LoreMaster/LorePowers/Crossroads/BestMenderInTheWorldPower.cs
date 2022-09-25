using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LoreMaster.LorePowers.Crossroads;

public class BestMenderInTheWorldPower : Power
{
    #region Members

    private Dictionary<GameObject, int> _signsHit = new();

    private int _menderbugHits;

    private MethodInfo _trackJournal;

    #endregion

    #region Constructors

    public BestMenderInTheWorldPower() : base("Best Mender in the World", Area.Crossroads)
    {
        // Pole
        // Breakable or BreakablePoleSimple
        // Direction Pole Tram (Crossroads_27)
        On.HutongGames.PlayMaker.Actions.RandomInt.OnEnter += RandomInt_OnEnter;
        On.PlayMakerFSM.OnEnable += Pacify;
        On.HealthManager.Hit += HealthManager_Hit;
        On.HealthManager.ApplyExtraDamage += HealthManager_ApplyExtraDamage;
        _trackJournal = ReflectionHelper.GetMethodInfo(typeof(EnemyDeathEffects), "RecordKillForJournal");
        Hint += "DON'T EVEN DARE KILLING THEM! I'LL END YOUR GAME IF YOU DO THAT!";
        Description += "DON'T EVEN DARE KILLING THEM! I'LL END YOUR GAME IF YOU DO THAT!";
    }

    #endregion

    #region Properties

    public override Action SceneAction => () =>
    {
        _signsHit.Clear();
        _menderbugHits = 0;
    };

    #endregion

    #region Event handler

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if ((self.gameObject.name.Contains("Pole") || self.gameObject.name.Contains("Sign")) && string.Equals(self.FsmName, "FSM"))
        {
            FsmState state = self.GetState("Spider Egg?");
            if (state != null)
            {
                Component.Destroy(self.GetComponent<NonBouncer>());
                FsmState cancel = new(self.Fsm)
                {
                    Name = "Unbreakable",
                    Actions = new FsmStateAction[]
                    {
                        new Lambda(() =>
                        {
                            if(State == PowerState.Active)
                                LoreMaster.Instance.Handler.StartCoroutine(Shuckle(self.transform));
                            else if(PlayerData.instance.GetInt("geo") > 50)
                                HeroController.instance.TakeGeo(50);
                            else
                                HeroController.instance.TakeDamage(null, GlobalEnums.CollisionSide.top, 1, 1);

                        })
                    }
                };
                self.AddState(state);
                state.ClearTransitions();
                state.AddTransition("FINISHED", cancel);
                cancel.AddTransition("FINISHED", State == PowerState.Active ? "Idle" : "No Rotate Check");
            }
        }
        orig(self);
    }

    private void BreakablePole_Hit(On.BreakablePole.orig_Hit orig, BreakablePole self, HitInstance damageInstance)
    {
        if (self.gameObject.name.Contains("Pole"))
        {
            Component.Destroy(self.GetComponent<NonBouncer>());
            LoreMaster.Instance.Handler.StartCoroutine(Shuckle(self.transform));
            if (State == PowerState.Twisted)
                orig(self, damageInstance);
        }
        else
            orig(self, damageInstance);
    }

    private void Breakable_Break(On.Breakable.orig_Break orig, Breakable self, float flingAngleMin, float flingAngleMax, float impactMultiplier)
    {
        if (self.gameObject.name.Contains("Pole"))
        {
            Component.Destroy(self.GetComponent<NonBouncer>());
            LoreMaster.Instance.Handler.StartCoroutine(Shuckle(self.transform));
            if (State == PowerState.Twisted)
                orig(self, flingAngleMin, flingAngleMax, impactMultiplier);
        }
        else
            orig(self, flingAngleMin, flingAngleMax, impactMultiplier);
    }

    private void HealthManager_ApplyExtraDamage(On.HealthManager.orig_ApplyExtraDamage orig, HealthManager self, int damageAmount)
    {
        if (string.Equals(self.gameObject.name, "Mender Bug") && damageAmount > 0)
            damageAmount = 0;
        orig(self, damageAmount);
    }

    private void HealthManager_Hit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
    {
        if (string.Equals(self.gameObject.name, "Mender Bug") && hitInstance.DamageDealt > 0)
        {
            if (hitInstance.AttackType == AttackTypes.Nail)
            {
                hitInstance.DamageDealt = 1;
                _menderbugHits++;
                if (_menderbugHits >= 200)
                {
                    PlayerData.instance.SetInt(nameof(PlayerData.instance.permadeathMode), 2);
                    HeroController.instance.TakeDamage(null, GlobalEnums.CollisionSide.top, 100, 1);
                }
                else if (_menderbugHits < 2 || (_menderbugHits % 20 == 0 && _menderbugHits != 0))
                    WarnPlayer(self.gameObject);
            }
            else
                hitInstance.DamageDealt = 0;
        }
        orig(self, hitInstance);
    }

    private void Pacify(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (string.Equals(self.FsmName, "Mender Bug Ctrl"))
        {
            self.GetState("Idle").ClearTransitions();
            self.GetState("Init").ClearTransitions();
            self.GetState("Init").AddTransition("FINISHED", "Chance");
            self.GetComponent<HealthManager>().hp = 10000;
        }
        orig(self);
    }

    private void RandomInt_OnEnter(On.HutongGames.PlayMaker.Actions.RandomInt.orig_OnEnter orig, HutongGames.PlayMaker.Actions.RandomInt self)
    {
        if (string.Equals(self.Fsm.GameObjectName, "Mender Bug"))
            self.min.Value = State == PowerState.Active ? 50 : 40;
        orig(self);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.Breakable.Break += Breakable_Break;
        On.BreakablePole.Hit += BreakablePole_Hit;
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.Breakable.Break -= Breakable_Break;
        On.BreakablePole.Hit -= BreakablePole_Hit;
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
    }

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        On.Breakable.Break += Breakable_Break;
        On.BreakablePole.Hit += BreakablePole_Hit;
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        On.Breakable.Break -= Breakable_Break;
        On.BreakablePole.Hit -= BreakablePole_Hit;
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
    }


    #endregion

    #region Methods

    private IEnumerator Shuckle(Transform transform)
    {
        if (State == PowerState.Twisted)
        {
            if (PlayerData.instance.GetInt("geo") > 50)
                HeroController.instance.TakeGeo(50);
            else
                HeroController.instance.TakeDamage(null, GlobalEnums.CollisionSide.top, 1, 1);
            yield break;
        }
        if (!_signsHit.ContainsKey(transform.gameObject))
            _signsHit.Add(transform.gameObject, 0);
        if (_signsHit[transform.gameObject] <= 2)
            FlingGeoAction.SpawnGeo(LoreMaster.Instance.Generator.Next(3, 9), LoreMaster.Instance.Generator.Next(0, 4), 0, ItemChanger.FlingType.Everywhere, transform);
        Vector3 current = transform.localPosition;
        for (int i = 0; i < 4; i++)
        {
            transform.localPosition += new Vector3(0f, .2f);
            yield return new WaitForSeconds(0.02f);
            transform.localPosition -= new Vector3(0f, .2f);
            yield return new WaitForSeconds(0.02f);
            transform.localPosition -= new Vector3(0f, .2f);
            yield return new WaitForSeconds(0.02f);
            transform.localPosition += new Vector3(0f, .2f);
        }
        if (current.y <= transform.localPosition.y)
            transform.localPosition = current;
        _signsHit[transform.gameObject]++;
    }

    private void WarnPlayer(GameObject menderBug)
    {
        if (_menderbugHits < 2)
            _trackJournal.Invoke(menderBug.GetComponent<EnemyDeathEffectsUninfected>(), new object[] { });
        PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value, "Display");
        playMakerFSM.FsmVariables.GetFsmInt("Convo Amount").Value = 1;
        playMakerFSM.FsmVariables.GetFsmString("Convo Title").Value = _menderbugHits < 2 ? "Menderbug_Journal" : $"Menderbug_Warning_{_menderbugHits / 40}";
        playMakerFSM.SendEvent("DISPLAY ENEMY DREAM");
    }

    #endregion
}
