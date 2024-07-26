using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.UnityComponents;
using Modding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

/// <summary>
/// Spawns minions or revek based on if you have killed Marissa. The revek logic is taken from Hollow Twitch: https://github.com/Sid-003/HKTwitch/blob/master/HollowTwitch/Commands/Enemies.cs#L167
/// </summary>
public class MarissasAudiencePower : Power
{
    #region Members

    private GameObject[] _companions = new GameObject[3];

    private List<GameObject> _extraCompanions = new();

    private GameObject _revek;

    private int[] _minCompanionAmount = new int[3] { 1, 2, 4 };

    private int[] _maxCompanionAmounts = new int[3] { 3, 6, 12 };

    #endregion

    #region Constructors

    public MarissasAudiencePower() : base("Marissas Audience", Area.CityOfTears) { }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the flag, that indicates if the player has killed Marissa.
    /// </summary>
    public bool IsMarissaDead => SceneData.instance.FindMyState(new PersistentBoolData()
    {
        sceneName = "Ruins_Bathhouse",
        id = "Ghost NPC",
        semiPersistent = false
    })?.activated ?? false;

    public GameObject[] Companions
    {
        get
        {
            if (_companions.Any(x => x == null))
                Initialize();
            return _companions;
        }
    }

    public override PowerRank Rank => PowerRank.Greater;

    #endregion

    #region Event handler

    private string ModHooks_BeforeSceneLoadHook(string newScene)
    {
        if (_extraCompanions.Any())
            foreach (GameObject companion in _extraCompanions)
                GameObject.Destroy(companion);
        _extraCompanions.Clear();
        return newScene;
    }

    //private GameObject GrimmEnemyRange_GetTarget(On.GrimmEnemyRange.orig_GetTarget orig, GrimmEnemyRange self)
    //{
    //    if (self.gameObject.GetComponent<EnemyBuff>()?.PowerName == PowerName)
    //    {
    //        LogHelper.Write<LoreMaster>("GetRange");
    //        return Vector2.Distance(self.transform.position, HeroController.instance.transform.position) < 20
    //            ? HeroController.instance.gameObject
    //            : null;
    //    }
    //    return orig(self);
    //}

    //private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    //{
    //    if (self.FsmName == "Control" && self.Fsm.GameObjectName.Contains("Grimmchild") && self.GetComponent<EnemyBuff>()?.PowerName == PowerName)
    //    {
    //        GameObject enemy = self.GetComponent<EnemyBuff>().Enemy;
    //        // Set grimm level (affects damage and attack speed)
    //        int grimmLevel = Random.Range(2, 5);
    //        self.GetState("Init").AdjustTransitions("Level " + grimmLevel);
    //        // Force quick spawn.
    //        self.FsmVariables.GetFsmBool("Scene Appear").Value = true;
    //        // Make follow target the enemy.
    //        self.GetState("Change").GetFirstAction<GetScale>().gameObject.GameObject = enemy;
    //        FsmState followState = self.GetState("Follow");
    //        followState.GetFirstAction<GrimmChildFly>().objectB = enemy;
    //        followState.GetFirstAction<GetDistance>().gameObject.GameObject = enemy;
    //        followState.GetFirstAction<DistanceFlySmooth>().gameObject.GameObject = enemy;
    //        self.GetState("Tele").GetFirstAction<GetPosition>().gameObject.GameObject = enemy;
    //        self.GetState("Antic").GetFirstAction<DistanceFlySmooth>().target = enemy;

    //        // 50% extra attack speed when on level four
    //        if (self.FsmVariables.GetFsmInt("Damage").Value == 11)
    //            followState.GetFirstAction<FloatSubtract>().subtract = 2f;
    //        // Ignore the bench check.
    //        self.GetState("Idle Anim").AdjustTransitions("Change");
    //    }
    //    orig(self);
    //}

    //private void SpawnObjectFromGlobalPool_OnEnter(On.HutongGames.PlayMaker.Actions.SpawnObjectFromGlobalPool.orig_OnEnter orig, SpawnObjectFromGlobalPool self)
    //{
    //    orig(self);
    //    if (self.IsCorrectContext("Control", null, "Shoot") && self.Fsm.GameObjectName.Contains("Grimmchild") && self.Fsm.GameObject.GetComponent<EnemyBuff>()?.PowerName == PowerName)
    //    {
    //        self.storeObject.Value.transform.Find("Enemy Damager").gameObject.SetActive(false);
    //        self.storeObject.Value.AddComponent<DamageHero>().damageDealt = self.Fsm.Variables.GetFsmInt("Damage").Value >= 8 ? 2 : 1;
    //        self.storeObject.Value.AddComponent<EnemyBuff>().PowerName = PowerName;
    //    }
    //}


    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        GameObject charmEffects = GameObject.Find("Charm Effects");
        _companions[0] = charmEffects.LocateMyFSM("Spawn Grimmchild").GetState("Spawn").GetFirstAction<SpawnObjectFromGlobalPool>().gameObject.Value;
        _companions[1] = charmEffects.LocateMyFSM("Weaverling Control").GetState("Spawn").GetFirstAction<SpawnObjectFromGlobalPool>().gameObject.Value;
        _companions[2] = charmEffects.LocateMyFSM("Hatchling Spawn").GetState("Hatch").GetFirstAction<SpawnObjectFromGlobalPool>().gameObject.Value;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        StartRoutine(GatherAudience);
        ModHooks.BeforeSceneLoadHook += ModHooks_BeforeSceneLoadHook;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ModHooks.BeforeSceneLoadHook -= ModHooks_BeforeSceneLoadHook;
        if (_extraCompanions.Any())
            foreach (GameObject companion in _extraCompanions)
                GameObject.Destroy(companion);
        _extraCompanions.Clear();
        if (_revek != null)
            GameObject.Destroy(_revek);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Spawns occasionally a crowd of companions or revek if marissa is dead.
    /// </summary>
    private IEnumerator GatherAudience()
    {
        while (true)
        {
            if (IsMarissaDead || State == PowerState.Twisted)
            {
                yield return new WaitForSeconds(LoreMaster.Instance.Generator.Next(45, 121));
                _revek = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Ghost Battle Revek"], HeroController.instance.transform.position, Quaternion.identity);
                _revek.SetActive(true);
                if (State == PowerState.Twisted)
                    GameObject.DontDestroyOnLoad(_revek);
                PlayMakerFSM revekFSM = _revek.LocateMyFSM("Control");
                yield return null;
                revekFSM.SetState("Appear Pause");
                float passedTime = 0f;
                float activeTime = LoreMaster.Instance.Generator.Next(20, 91);
                while (passedTime < activeTime && _revek != null)
                {
                    passedTime += Time.deltaTime;
                    yield return null;
                }
                if (_revek != null)
                    GameObject.Destroy(_revek);
            }
            else
            {
                yield return new WaitForSeconds(LoreMaster.Instance.Generator.Next(45, 121));
                for (int companionIndex = 0; companionIndex < 3; companionIndex++)
                {
                    for (int companionCopy = 0; companionCopy < LoreMaster.Instance.Generator.Next(_minCompanionAmount[companionIndex], _maxCompanionAmounts[companionIndex]); companionCopy++)
                    {
                        GameObject newCompanion = GameObject.Instantiate(Companions[companionIndex]
                            , new Vector3(HeroController.instance.transform.GetPositionX()
                            , HeroController.instance.transform.GetPositionY()), Quaternion.identity);
                        GameObject.DontDestroyOnLoad(newCompanion);
                        _extraCompanions.Add(newCompanion);
                    }
                }

                yield return new WaitForSeconds(LoreMaster.Instance.Generator.Next(30, 91));

                foreach (GameObject companion in _extraCompanions)
                    GameObject.Destroy(companion);
                _extraCompanions.Clear();
            }
        }
    }

    #endregion
}

