using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Manager;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        _trackJournal = ReflectionHelper.GetMethodInfo(typeof(EnemyDeathEffects), "RecordKillForJournal");
        Hint += " DON'T EVEN DARE KILLING THEM! I'LL END YOUR GAME IF YOU DO THAT!";
        Description += " DON'T EVEN DARE KILLING THEM! I'LL END YOUR GAME IF YOU DO THAT!";
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
        // Fail save for Menderbug items.
        if (_menderbugHits > 10 && ItemChanger.Internal.Ref.Settings.Placements.ContainsKey("Journal_Entry-Menderbug"))
        {
            try
            {
                if (ItemChanger.Internal.Ref.Settings.Placements["Journal_Entry-Menderbug"].Items.Any(x => !x.IsObtained()))
                {
                    ItemChanger.Internal.Ref.Settings.Placements["Journal_Entry-Menderbug"].GiveAll(new()
                    {
                        FlingType = FlingType.DirectDeposit,
                        MessageType = MessageType.Any,
                    });
                    ItemChanger.Internal.Ref.Settings.Placements["Hunter's_Notes-Menderbug"].GiveAll(new()
                    {
                        FlingType = FlingType.DirectDeposit,
                        MessageType = MessageType.Any,
                    });
                }
            }
            catch (Exception ex)
            {
                LoreMaster.Instance.LogError("An error occured with giving the items for Menderbug: " + ex.StackTrace);
            }
        }
        if (_menderbugHits >= 200)
            return;
        PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value, "Display");
        playMakerFSM.FsmVariables.GetFsmInt("Convo Amount").Value = 1;
        playMakerFSM.FsmVariables.GetFsmString("Convo Title").Value = _menderbugHits < 2 ? "Menderbug_Journal" : $"Menderbug_Warning_{_menderbugHits / 40}";
        playMakerFSM.SendEvent("DISPLAY ENEMY DREAM");
    }

    #endregion
}
