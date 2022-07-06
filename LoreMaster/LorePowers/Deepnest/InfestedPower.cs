using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.Deepnest;

public class InfestedPower : Power
{
    private GameObject _weaverPrefab;

    private List<GameObject> _weavers = new();

    #region Constructors

    public InfestedPower() : base("", Area.Deepnest)
    {
        _weaverPrefab = GameObject.Find("Knight/Charm Effects").LocateMyFSM("Weaverling Control").GetState("Spawn").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
    }

    #endregion

    #region Public Methods

    public override void Enable() => On.HealthManager.Die += HealthManager_Die;
    

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        _weavers.RemoveAll(x => x == null);

        // Just in case we limit the amount of weavers to prevent crashing the game... hopefully
        if (_weavers.Count < 100)
            for (int i = 0; i < UnityEngine.Random.Range(1, 6); i++)
                _weavers.Add(GameObject.Instantiate(_weaverPrefab, new Vector3(HeroController.instance.transform.GetPositionX(), HeroController.instance.transform.GetPositionY()), Quaternion.identity));
    }

    public override void Disable() => On.HealthManager.Die -= HealthManager_Die;
    
    #endregion
}
