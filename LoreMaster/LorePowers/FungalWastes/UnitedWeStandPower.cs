using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public int CompanionAmount => _companions.Count(x => x != null && x.activeSelf);

    #endregion

    #region Event Handler

    private void HatchlingSpawn(On.KnightHatchling.orig_OnEnable orig, KnightHatchling self)
    {
        orig(self);
        self.normalDetails.damage = State == PowerState.Twisted 
            ? Math.Max(1, 10 - CompanionAmount) 
            : 10 + CompanionAmount * 2;
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.KnightHatchling.OnEnable += HatchlingSpawn;
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(UpdateCompanions());
    }

    /// <inheritdoc/>
    protected override void Disable() => On.KnightHatchling.OnEnable -= HatchlingSpawn;

    /// <inheritdoc/>
    protected override void TwistEnable() => Enable();

    /// <inheritdoc/>
    protected override void TwistDisable() => Disable();

    #endregion

    #region Private Methods

    /// <summary>
    /// Updates the companions continious.
    /// </summary>
    private IEnumerator UpdateCompanions()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            ModifyCompanion();
        }
    }

    /// <summary>
    /// Updates the companions amount and fsm.
    /// </summary>
    private void ModifyCompanion()
    {
        _companions.RemoveAll(x => x == null);
        List<GameObject> foundCompanions = GameObject.FindGameObjectsWithTag("Grimmchild").ToList();
        foundCompanions.AddRange(GameObject.FindGameObjectsWithTag("Weaverling"));
        foundCompanions.AddRange(GameObject.FindGameObjectsWithTag("Knight Hatchling"));

        try
        {
            foreach (GameObject companion in foundCompanions)
            {
                if (_companions.Contains(companion))
                    continue;

                PlayMakerFSM companionFsm;
                FsmState fsmState;
                if (companion.tag.Equals("Grimmchild"))
                {
                    companionFsm = companion.LocateMyFSM("Control");
                    fsmState = companionFsm.GetState("Antic");
                    fsmState.ReplaceAction(new Lambda(() =>
                    {
                        companionFsm.FsmVariables.FindFsmFloat("Attack Timer").Value = State == PowerState.Twisted 
                        ? 1.5f + (CompanionAmount * .1f)
                        : (Active ? Mathf.Max(0.3f, 1.5f - (CompanionAmount * 0.1f)) : 1.5f);
                    }), 3);
                }
                else if (companion.tag.Equals("Weaverling"))
                {
                    companionFsm = companion.LocateMyFSM("Control");
                    fsmState = companionFsm.GetState("Run Dir");
                    fsmState.ReplaceAction(new Lambda(() =>
                    {
                        float weaverScale = State == PowerState.Twisted 
                        ? (CompanionAmount > 9 ? .1f : 1 - CompanionAmount * 0.1f)
                        : 1f + (CompanionAmount * 0.1f);

                        companionFsm.FsmVariables.FindFsmFloat("Scale").Value = State == PowerState.Twisted 
                        ? Mathf.Min(.1f, weaverScale)
                        : (Active ? Mathf.Min(2.2f, weaverScale) : 1f);

                        companionFsm.FsmVariables.FindFsmFloat("Neg Scale").Value = State == PowerState.Twisted
                        ? Mathf.Max(- .1f, weaverScale * -1f) 
                        : (Active ? Mathf.Max(-2.2f, weaverScale * -1f) : -1f);

                        companion.transform.localScale = new Vector3(weaverScale, weaverScale);
                        companion.transform.SetScaleMatching(weaverScale);

                        // Imitates the send random event which we removed.
                        companionFsm.SendEvent(LoreMaster.Instance.Generator.Next(0, 2) == 0 ? "L" : "R");
                    }), 7);
                }
                _companions.Add(companion);
            }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error: " + exception.Message);
        }
    }

    #endregion
}
