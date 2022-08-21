using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using System.Collections.Generic;
using UnityEngine;
using LoreMaster.Enums;
using LoreMaster.UnityComponents;
using LoreMaster.Extensions;
using ItemChanger.FsmStateActions;
using System.Collections;

namespace LoreMaster.LorePowers.Deepnest;

public class InfestedPower : Power
{
    #region Members

    private GameObject _weaverPrefab;

    private List<GameObject> _weavers = new();

    #endregion

    #region Constructors

    public InfestedPower() : base("Infested!", Area.Deepnest) { }

    #endregion

    #region Event Handler

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
                GameObject weaver = GameObject.Instantiate(_weaverPrefab, new Vector3(HeroController.instance.transform.GetPositionX(), HeroController.instance.transform.GetPositionY()), Quaternion.identity);
                _weavers.Add(weaver);
                LoreMaster.Instance.Handler.StartCoroutine(WeaverLife(weaver));
            }
            Object.Destroy(infested);
        }
        orig(self, attackDirection, attackType, ignoreEvasion);
    }

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (string.Equals(self.FsmName,"Attack") && string.Equals(self.transform.parent?.name, "Weaverling(Clone)"))
        {
            self.GetState("Hit").ReplaceAction(new Lambda(() =>
            {
                if (Active)
                    Infest(self.FsmVariables.FindFsmGameObject("Enemy").Value);
                self.FsmVariables.FindFsmInt("Enemy HP").Value -= self.FsmVariables.FindFsmInt("Damage").Value;
            })
            { Name = "Infest" }, 4);
        }
        orig(self);
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
       => _weaverPrefab = GameObject.Find("Knight/Charm Effects").LocateMyFSM("Weaverling Control").GetState("Spawn").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        On.HealthManager.Die += HealthManager_Die;
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
        On.HealthManager.Die -= HealthManager_Die;
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
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

    #endregion
}
