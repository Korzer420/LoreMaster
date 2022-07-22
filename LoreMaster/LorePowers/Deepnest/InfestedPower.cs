using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using System.Collections.Generic;
using UnityEngine;
using LoreMaster.Enums;
using LoreMaster.UnityComponents;
using LoreMaster.Extensions;
using ItemChanger.FsmStateActions;

namespace LoreMaster.LorePowers.Deepnest;

public class InfestedPower : Power
{
    #region Members

    private GameObject _weaverPrefab;

    private List<GameObject> _weavers = new();

    #endregion

    #region Constructors

    public InfestedPower() : base("Infested!", Area.Deepnest)
    {
        Hint = "Plant spider eggs in the wounds of your victims, that burst open on death.";
        Description = "Hitting an enemy with the nail plants an egg to the enemy (Capped at 5), which spawns a weaver once the enemy died. Weavers also apply eggs. (Capped at 75)";
    }

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
        orig(self, attackDirection, attackType, ignoreEvasion);
        _weavers.RemoveAll(x => x == null);
        LoreMaster.Instance.Log("Called Death");
        // Just in case we limit the amount of weavers to prevent crashing the game... hopefully
        Infested infested = self.GetComponent<Infested>();
        if (infested != null)
        {
            for (int i = 0; i < infested.Eggs; i++)
            {
                if (_weavers.Count >= 25)
                    break;
                _weavers.Add(GameObject.Instantiate(_weaverPrefab, new Vector3(HeroController.instance.transform.GetPositionX(), HeroController.instance.transform.GetPositionY()), Quaternion.identity));
            }
            Object.Destroy(infested);
        }
    }

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (self.FsmName.Equals("Attack") && self.transform.parent.name.Equals("Weaverling(Clone)"))
        {
            self.GetState("Hit").ReplaceAction(new Lambda(() =>
            {
                LoreMaster.Instance.Log("Called attack");
                if (Active)
                    Infest(self.gameObject);
                self.FsmVariables.FindFsmInt("Enemy HP").Value -= self.FsmVariables.FindFsmInt("Damage").Value;
            })
            { Name = "Infest" }, 4);
        }
        orig(self);
    }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    => _weaverPrefab = GameObject.Find("Knight/Charm Effects").LocateMyFSM("Weaverling Control").GetState("Spawn").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;

    protected override void Enable()
    {
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        On.HealthManager.Die += HealthManager_Die;
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
    }

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
        if (!gameObject.name.Equals("Hollow Knight Boss") && !gameObject.name.Contains("Collector"))
        {
            Infested infested = gameObject.GetComponent<Infested>();
            if (infested == null)
                gameObject.gameObject.AddComponent<Infested>();
            else
                infested.Eggs++;
        }
    }

    #endregion
}
