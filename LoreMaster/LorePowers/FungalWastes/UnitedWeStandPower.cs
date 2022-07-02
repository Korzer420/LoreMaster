using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vasi;

namespace LoreMaster.LorePowers.FungalWastes
{
    public class UnitedWeStandPower : Power
    {
        private List<GameObject> _companions = new List<GameObject>();

        private List<Coroutine> _runningCoroutines = new List<Coroutine>();

        /// <summary>
        /// Get the amount of active companions
        /// </summary>
        public int CompanionAmount
        {
            get => _companions.Count(x => x.activeSelf);
        }

        public static UnitedWeStandPower Instance { get; set; }

        public UnitedWeStandPower() : base("FUNG_TAB_03", Area.FungalWastes)
        => Description = "<br>[United we stand]<br>Your companions inspire each other.";


        public override void Enable()
        {
            On.KnightHatchling.OnEnable += HatchlingSpawn;
            _runningCoroutines.Add(HeroController.instance.StartCoroutine(UpdateCompanions()));
        }

        private void HatchlingSpawn(On.KnightHatchling.orig_OnEnable orig, KnightHatchling self)
        {
            orig(self);
            self.normalDetails.damage = 10 + CompanionAmount * 2;
        }

        public override void Disable()
        {
            _runningCoroutines.RemoveAll(x => x == null);
            On.KnightHatchling.OnEnable -= HatchlingSpawn;
        }

        IEnumerator UpdateCompanions()
        {
            while (true)
            {
                yield return new WaitForSeconds(3f);
                GetCompanions();
            }
        }

        private void GetCompanions()
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
                        fsmState.InsertMethod(3, () =>
                        {
                            float attackSpeedIncrease = CompanionAmount * 0.1f;
                            // To prevent some unwanted behavior the attack speed is capped at 0.3 seconds
                            if (attackSpeedIncrease > 1.2f)
                                attackSpeedIncrease = 1.2f;
                            companionFsm.FsmVariables.FindFsmFloat("Attack Timer").Value = 1.5f - attackSpeedIncrease;
                        });
                    }
                    else if (companion.tag.Equals("Weaverling"))
                    {
                        companionFsm = companion.LocateMyFSM("Control");
                        fsmState = companionFsm.GetState("Run Dir");
                        fsmState.AddMethod(() =>
                        {
                            float weaverScale = 1f + (CompanionAmount * 0.15f);
                            companionFsm.FsmVariables.FindFsmFloat("Scale").Value = weaverScale;
                            companionFsm.FsmVariables.FindFsmFloat("Neg Scale").Value = weaverScale * -1f;
                            companion.transform.localScale = new Vector3(weaverScale, weaverScale);
                            companion.transform.SetScaleMatching(weaverScale);
                        });
                    }
                    _companions.Add(companion);
                }
            }
            catch (Exception exception)
            {
                LoreMaster.Instance.Log("Error: " + exception.Message);
                throw;
            }
        }
    }
}
