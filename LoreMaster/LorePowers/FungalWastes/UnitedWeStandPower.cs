using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
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

    public UnitedWeStandPower() : base("United we Stand", Area.FungalWastes)
    {
        Hint = "Your companions inspire each other.";
        Description = "Weavers are bigger, Grimmchild shoots faster and Hatchling deal more damage for each companion (of those three) you have.";
    }

    #endregion

    #region Properties

    /// <summary>
    /// Get the amount of active companions
    /// </summary>
    public int CompanionAmount => _companions.Count(x => x.activeSelf);

    #endregion

    #region Event Handler

    private void HatchlingSpawn(On.KnightHatchling.orig_OnEnable orig, KnightHatchling self)
    {
        orig(self);
        self.normalDetails.damage = 10 + CompanionAmount * 2;
    }

    #endregion

    #region Protected Methods

    protected override void Enable()
    {
        On.KnightHatchling.OnEnable += HatchlingSpawn;
        LoreMaster.Instance.Handler.StartCoroutine(UpdateCompanions());
    }

    protected override void Disable()
    {
        LoreMaster.Instance.Handler.StopCoroutine(UpdateCompanions());
        On.KnightHatchling.OnEnable -= HatchlingSpawn;
    } 

    #endregion

    #region Private Methods

    /// <summary>
    /// Updates the companions continious.
    /// </summary>
    /// <returns></returns>
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
                    fsmState.RemoveAction(3);
                    fsmState.InsertAction(new Lambda(() =>
                    {
                        float attackSpeedIncrease = CompanionAmount * 0.1f;
                        // To prevent some unwanted behavior the attack speed is capped at 0.3 seconds
                        if (attackSpeedIncrease > 1.2f)
                            attackSpeedIncrease = 1.2f;
                        companionFsm.FsmVariables.FindFsmFloat("Attack Timer").Value = 1.5f - attackSpeedIncrease;
                    }), 3);
                }
                else if (companion.tag.Equals("Weaverling"))
                {
                    companionFsm = companion.LocateMyFSM("Control");
                    fsmState = companionFsm.GetState("Run Dir");
                    fsmState.AddLastAction(new Lambda(() =>
                    {
                        float weaverScale = 1f + (CompanionAmount * 0.1f);
                        companionFsm.FsmVariables.FindFsmFloat("Scale").Value = weaverScale;
                        companionFsm.FsmVariables.FindFsmFloat("Neg Scale").Value = weaverScale * -1f;
                        companion.transform.localScale = new Vector3(weaverScale, weaverScale);
                        companion.transform.SetScaleMatching(weaverScale);
                    }));
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
