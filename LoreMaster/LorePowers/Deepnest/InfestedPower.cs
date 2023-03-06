using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.UnityComponents;
using Modding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoreMaster.LorePowers.Deepnest;

public class InfestedPower : Power
{
    #region Members

    private GameObject _weaverPrefab;

    private List<GameObject> _weavers = new();

    private float _passedTime = 0f;

    #endregion

    #region Constructors

    public InfestedPower() : base("Infested!", Area.Deepnest) { }

    #endregion

    #region Properties

    public GameObject WeaverPrefab => _weaverPrefab == null ? GameObject.Find("Knight/Charm Effects").LocateMyFSM("Weaverling Control").GetState("Spawn").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value : _weaverPrefab;

    public override System.Action SceneAction => () => 
    {
        if (_runningCoroutine != null)
            LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
        if (State == PowerState.Twisted)
        {
            _passedTime = 0f;
            StartRoutine(() => Infestation());
        }
    };

    #endregion

    #region Event handler

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (hitInstance.AttackType == AttackTypes.Nail)
            Infest(self.gameObject);

        orig(self, hitInstance);
    }

    /// <summary>
    /// Event handler, when an enemy dies.
    /// </summary>
    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        _weavers.RemoveAll(x => x == null);
        // Just in case we limit the amount of weavers to prevent crashing the game... hopefully
        Infested infested = self.GetComponent<Infested>();
        if (infested != null)
        {
            for (int i = 0; i < infested.Eggs; i++)
            {
                if (_weavers.Count >= 25)
                    break;
                GameObject weaver = GameObject.Instantiate(WeaverPrefab, new Vector3(HeroController.instance.transform.GetPositionX(), HeroController.instance.transform.GetPositionY()), Quaternion.identity);
                _weavers.Add(weaver);
                LoreMaster.Instance.Handler.StartCoroutine(WeaverLife(weaver));
            }
            Object.Destroy(infested);
        }
        orig(self, attackDirection, attackType, ignoreEvasion);
    }

    private void OnIntCompareAction(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
    {
        if (string.Equals(self.Fsm.Name, "Attack") && string.Equals(self.Fsm.FsmComponent.gameObject.transform.parent?.gameObject.name, "Weaverling(Clone)") 
            && string.Equals(self.State.Name, "Hit"))
            Infest(self.Fsm.Variables.FindFsmGameObject("Enemy").Value);
        orig(self);
    }

    private void FaceDirection_OnUpdate(On.HutongGames.PlayMaker.Actions.FaceDirection.orig_OnUpdate orig, FaceDirection self)
    {
        orig(self);
        if (self.IsCorrectContext("Control", "Big Spider", null) && (self.State.Name == "Chase - In Sight" || self.State.Name == "Chase - Out of Sight"))
            self.Fsm.FsmComponent.transform.localScale = new Vector3(self.Fsm.FsmComponent.transform.localScale.x < 0 ? -3f : 3f, 3f);
    }

    private void HealthManager_TakeDamage1(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (self.gameObject.name == "Big Spider")
            hitInstance.DamageDealt = 0;
        orig(self, hitInstance);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
     => _weaverPrefab = GameObject.Find("Knight/Charm Effects").LocateMyFSM("Weaverling Control").GetState("Spawn").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
    
    /// <inheritdoc/>
    protected override void Enable()
    {
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += OnIntCompareAction;
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        On.HealthManager.Die += HealthManager_Die;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter -= OnIntCompareAction;
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
        On.HealthManager.Die -= HealthManager_Die;
    }

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        On.HutongGames.PlayMaker.Actions.FaceDirection.OnUpdate += FaceDirection_OnUpdate;
        StartRoutine(() => Infestation());
        On.HealthManager.TakeDamage += HealthManager_TakeDamage1;
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        On.HutongGames.PlayMaker.Actions.FaceDirection.OnUpdate -= FaceDirection_OnUpdate;
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage1;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Adds eggs to the enemy.
    /// </summary>
    /// <param name="gameObject">The enemy game object</param>
    private void Infest(GameObject gameObject)
    {
        // The hollow knight can trigger it's death multiple times. To make true ending more accessable thk doesn't spawn weavers.
        if (!gameObject.name.Equals("Hollow Knight Boss"))
        {
            Infested infested = gameObject.GetComponent<Infested>();
            if (infested == null)
                gameObject.gameObject.AddComponent<Infested>();
            else
                infested.Eggs++;
        }
    }

    /// <summary>
    /// Handles the life time of weavers.
    /// </summary>
    private IEnumerator WeaverLife(GameObject weaver)
    {
        float passedTime = 0f;
        while(passedTime < 20f && weaver != null)
        {
            yield return null;
            passedTime += Time.deltaTime;
        }

        if (weaver != null)
            GameObject.Destroy(weaver);
    }

    private IEnumerator Infestation()
    {
        int mileStone = 0;
        while (mileStone < 10)
        {
            if (!HeroController.instance.acceptingInput || PlayerData.instance.GetBool("atBench"))
                yield return new WaitUntil(() => HeroController.instance.acceptingInput && !PlayerData.instance.GetBool("atBench"));
            _passedTime += Time.deltaTime;
            if(_passedTime >= 5)
            {
                _passedTime = 0f;
                mileStone++;
                if (mileStone < 6)
                    for (int i = 0; i < mileStone; i++)
                        SpawnSpider(false);
                else if (mileStone < 10)
                    for (int i = 0; i < 7; i++)
                        SpawnSpider(false);
                else
                    SpawnSpider(true);
            }
            yield return null;
        }
    }

    private void SpawnSpider(bool bigSpider)
    {
        GameObject spider = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Spider Flyer"]);
        spider.SetActive(true);
        spider.AddComponent<IgnoreTerrain>();
        Component.Destroy(spider.GetComponent<CircleCollider2D>());
        spider.LocateMyFSM("Control").GetState("Chase - In Sight").GetFirstActionOfType<FloatCompare>().float2.Value = 120f;
        if (bigSpider)
            spider.name = "Big Spider";
        Vector3 heroPosition = HeroController.instance.transform.position;
        Vector3 spiderPosition = heroPosition;
        do
        {
            spiderPosition.x = LoreMaster.Instance.Generator.Next((int)heroPosition.x - 5, (int)heroPosition.x + 5);
            spiderPosition.y = LoreMaster.Instance.Generator.Next((int)heroPosition.y, (int)heroPosition.y + 5);
        } 
        while (Vector3.Distance(heroPosition,spiderPosition) < 5);
        spider.transform.position = spiderPosition;
        spider.LocateMyFSM("Control").SendEvent("SPAWN");
        ReflectionHelper.SetField(spider.GetComponent<HealthManager>(), "smallGeoDrops", 0);
        ReflectionHelper.SetField(spider.GetComponent<HealthManager>(), "mediumGeoDrops", 0);
        ReflectionHelper.SetField(spider.GetComponent<HealthManager>(), "largeGeoDrops", 0);
    }

    #endregion
}
