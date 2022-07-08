using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using System.Collections.Generic;
using UnityEngine;
using LoreMaster.Enums;

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
        Hint = "Plant spider eggs in the wounds of your victims, that burst open on death";
        Description = "Killing an enemy grants 1 to 5 weaverlings that assist you in the current room. (Capped at 50)";
    }

    #endregion

    #region Event Handler

    /// <summary>
    /// Event handler, when an enemy dies.
    /// </summary>
    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        _weavers.RemoveAll(x => x == null);

        // Just in case we limit the amount of weavers to prevent crashing the game... hopefully
        if (_weavers.Count < 100)
            for (int i = 0; i < UnityEngine.Random.Range(1, 6); i++)
                _weavers.Add(GameObject.Instantiate(_weaverPrefab, new Vector3(HeroController.instance.transform.GetPositionX(), HeroController.instance.transform.GetPositionY()), Quaternion.identity));
    }

    #endregion

    #region Public Methods

    protected override void Initialize()
    => _weaverPrefab = GameObject.Find("Knight/Charm Effects").LocateMyFSM("Weaverling Control").GetState("Spawn").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
    
    protected override void Enable() => On.HealthManager.Die += HealthManager_Die;
    
    protected override void Disable() => On.HealthManager.Die -= HealthManager_Die;
    
    #endregion
}
