using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ItemChanger.Extensions;
using System.Collections;

namespace LoreMaster.LorePowers.RestingGrounds;

internal class DreamBlessingPower : Power
{
    #region Members

    private GameObject _weaverlingPrefab;

    private List<GameObject> _spawnedWeavers = new();

    private Coroutine _weaverRoutine;

    #endregion

    #region Constructors

    public DreamBlessingPower() : base("", Area.RestingGrounds)
    {
        
    }

    #endregion

    #region Public Methods

    protected override void Initialize()
    {
        _weaverlingPrefab = GameObject.Find("Charm Effects").LocateMyFSM("Weaverling Control").GetState("Spawn").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
    }

    protected override void Enable()
    {
        On.EnemyDreamnailReaction.RecieveDreamImpact += EnemyDreamnailReaction_RecieveDreamImpact;
    }

    private void EnemyDreamnailReaction_RecieveDreamImpact(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
    {
        orig(self);

        if(PlayerData.instance.lurienDefeated)
            if (self.GetComponent<EnemyBinding>() == null)
                self.gameObject.AddComponent<EnemyBinding>();

        // Herrah... don't ask.
        if (PlayerData.instance.hegemolDefeated)
        {
            _spawnedWeavers.Add(GameObject.Instantiate(_weaverlingPrefab, HeroController.instance.transform.position, Quaternion.identity));
            _spawnedWeavers.Add(GameObject.Instantiate(_weaverlingPrefab, HeroController.instance.transform.position, Quaternion.identity));
            if (_weaverRoutine == null)
                _weaverRoutine = LoreMaster.Instance.Handler.StartCoroutine(SpawnWeavers());
        }

        if (PlayerData.instance.monomonDefeated)
        {
            int essence = PlayerData.instance.GetInt(nameof(PlayerData.instance.dreamOrbs));

            if (essence > 2400)
                essence = 2400;
            essence /= 100;
            if(LoreMaster.Instance.Generator.Next(1,101) <= essence)
            {
                // This assumes that the component is on the same object, if not we ignore it. (It isn't worth the hussle to account for that currently)
                HealthManager healthManager = self.GetComponent<HealthManager>();
                if (healthManager != null)
                    healthManager.ApplyExtraDamage(120);
            }
        }
    }

    protected override void Disable()
    {
        On.EnemyDreamnailReaction.RecieveDreamImpact -= EnemyDreamnailReaction_RecieveDreamImpact;
    }

    #endregion

    #region Private Methods

    private IEnumerator SpawnWeavers()
    {
        float passedTime = 0f;
        while (passedTime < 15f)
        {
            passedTime += Time.deltaTime;
            yield return null;
        }

        foreach (GameObject weaver in _spawnedWeavers)
            GameObject.Destroy(weaver);
        _spawnedWeavers.Clear();
    }

    #endregion
}
