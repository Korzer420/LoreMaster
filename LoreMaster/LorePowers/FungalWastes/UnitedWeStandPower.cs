using HutongGames.PlayMaker;
using LoreMaster.Enums;
using LoreMaster.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes;

public class UnitedWeStandPower : Power
{
    #region Members

    private List<GameObject> _companions = new();

    #endregion

    #region Constructors

    public UnitedWeStandPower() : base("United we Stand", Area.FungalWastes) { }

    #endregion

    #region Properties

    /// <summary>
    /// Get the amount of active companions
    /// </summary>
    public int CompanionAmount
    {
        get
        {
            _companions.RemoveAll(x => x is null || !x.activeSelf);
            return _companions.Count;
        }
    }

    #endregion

    #region Event Handler

    private void HatchlingSpawn(On.KnightHatchling.orig_OnEnable orig, KnightHatchling self)
    {
        orig(self);
        self.normalDetails.damage = State == PowerState.Twisted
            ? Math.Max(1, 10 - CompanionAmount)
            : Math.Max(34, 10 + CompanionAmount * 2);
    }

    private void Tk2dPlayAnimation_OnEnter(On.HutongGames.PlayMaker.Actions.Tk2dPlayAnimation.orig_OnEnter orig, HutongGames.PlayMaker.Actions.Tk2dPlayAnimation self)
    {
        orig(self);
        if (self.IsCorrectContext("Control", null, "Run Dir") && self.Fsm.FsmComponent.gameObject.name.Contains("Weaverling"))
            ModifyWeaverSize(self);
    }

    private void SetScale_OnEnter(On.HutongGames.PlayMaker.Actions.SetScale.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetScale self)
    {
        orig(self);
        if (self.IsCorrectContext("Control", null, null) && self.Fsm.FsmComponent.gameObject.name.Contains("Weaverling"))
            ModifyWeaverSize(self);
    }

    private void RandomFloat_OnEnter(On.HutongGames.PlayMaker.Actions.RandomFloat.orig_OnEnter orig, HutongGames.PlayMaker.Actions.RandomFloat self)
    {
        orig(self);
        if (self.IsCorrectContext("Control", null, "Antic") && self.Fsm.GameObjectName.Contains("Grimmchild"))
            self.Fsm.Variables.FindFsmFloat("Attack Timer").Value = State == PowerState.Twisted
                        ? 1.5f + (CompanionAmount * .1f)
                        : (State == PowerState.Active
                        ? Mathf.Max(0.3f, 1.5f - (CompanionAmount * 0.1f))
                        : 1.5f);
    }

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        orig(self);
        if ((self.Fsm.GameObjectName.Contains("Grimmchild") || self.Fsm.GameObjectName.Contains("Weaverling") && string.Equals(self.FsmName, "Control"))
            || (string.Equals(self.FsmName, "ProxyFSM") && self.Fsm.GameObjectName.Contains("Knight Hatchling")))
            _companions.Add(self.Fsm.GameObject);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.KnightHatchling.OnEnable += HatchlingSpawn;
        On.HutongGames.PlayMaker.Actions.SetScale.OnEnter += SetScale_OnEnter;
        On.HutongGames.PlayMaker.Actions.Tk2dPlayAnimation.OnEnter += Tk2dPlayAnimation_OnEnter;
        On.HutongGames.PlayMaker.Actions.RandomFloat.OnEnter += RandomFloat_OnEnter;
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        StartRoutine(() => UpdateCompanions());
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.KnightHatchling.OnEnable -= HatchlingSpawn;
        On.HutongGames.PlayMaker.Actions.SetScale.OnEnter -= SetScale_OnEnter;
        On.HutongGames.PlayMaker.Actions.Tk2dPlayAnimation.OnEnter -= Tk2dPlayAnimation_OnEnter;
        On.HutongGames.PlayMaker.Actions.RandomFloat.OnEnter -= RandomFloat_OnEnter;
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
        _companions.Clear();
    }

    /// <inheritdoc/>
    protected override void TwistEnable() => Enable();

    /// <inheritdoc/>
    protected override void TwistDisable() => Disable();

    #endregion

    #region Private Methods

    private IEnumerator UpdateCompanions()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            _companions.RemoveAll(x => x == null);
        }
    }

    private void ModifyWeaverSize(FsmStateAction self)
    {
        float weaverScale = State == PowerState.Twisted
                            ? (CompanionAmount > 9 ? .1f : 1 - CompanionAmount * 0.1f)
                            : 1f + (CompanionAmount * 0.1f);

        self.Fsm.Variables.FindFsmFloat("Scale").Value = State == PowerState.Twisted
        ? Mathf.Min(.1f, weaverScale)
        : (State == PowerState.Active ? Mathf.Min(2.2f, weaverScale) : 1f);

        self.Fsm.Variables.FindFsmFloat("Neg Scale").Value = State == PowerState.Twisted
        ? Mathf.Max(-.1f, weaverScale * -1f)
        : (State == PowerState.Active ? Mathf.Max(-2.2f, weaverScale * -1f) : -1f);

        self.Fsm.FsmComponent.transform.localScale = new Vector3(weaverScale, weaverScale);
        self.Fsm.FsmComponent.transform.SetScaleMatching(weaverScale);
    }

    #endregion
}
