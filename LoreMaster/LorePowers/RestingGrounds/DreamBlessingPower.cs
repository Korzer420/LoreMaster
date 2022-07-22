using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using LoreMaster.Enums;
using LoreMaster.UnityComponents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoreMaster.LorePowers.RestingGrounds;

public class DreamBlessingPower : Power
{
    #region Members

    private GameObject _weaverlingPrefab;

    private List<GameObject> _spawnedWeavers = new();

    private Coroutine _weaverRoutine;

    #endregion

    #region Constructors

    public DreamBlessingPower() : base("Dream Blessing", Area.RestingGrounds)
    {
        Hint = "The dream artifact uses the power it absorbs from their powerful victims to use it's hidden power.<br>Monomon: Through her knowledge she exposes the foes biggest weakness.<br>" +
            "Lurien: His gaze may freeze the enemy in place.<br>Herrah: Invoking her children from the victim.";
        Description = "Defeated Dreamers grant the dream nail an additional effect.<br/>Lurien: Roots the target for 3 seconds (15 seconds cooldown)<br/>Herrah: Spawn 2 weavers for 30 seconds." +
            "<br/>Monomon: Per 100 Essence you have a 1% chance to instant kill the enemy (capped at 200 damage). Capped at 2400 Essence for 24%.";
    }

    #endregion

    #region Event Handler

    private void EnemyDreamnailReaction_RecieveDreamImpact(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
    {
        orig(self);

        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.lurienDefeated)))
            if (self.GetComponent<EnemyBinding>() == null)
                self.gameObject.AddComponent<EnemyBinding>();

        // Herrah... don't ask.
        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.hegemolDefeated)))
        {
            _spawnedWeavers.Add(GameObject.Instantiate(_weaverlingPrefab, HeroController.instance.transform.position, Quaternion.identity));
            _spawnedWeavers.Add(GameObject.Instantiate(_weaverlingPrefab, HeroController.instance.transform.position, Quaternion.identity));
            if (_weaverRoutine == null)
                _weaverRoutine = LoreMaster.Instance.Handler.StartCoroutine(WeaverLifeTime());
        }

        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.monomonDefeated)))
        {
            int essence = PlayerData.instance.GetInt(nameof(PlayerData.instance.dreamOrbs));

            if (essence > 2400)
                essence = 2400;
            essence /= 100;
            if (LoreMaster.Instance.Generator.Next(1, 101) <= essence)
            {
                // This assumes that the component is on the same object, if not we ignore it. (It isn't worth the hussle to account for that currently)
                HealthManager healthManager = self.GetComponent<HealthManager>();
                if (healthManager != null)
                    healthManager.ApplyExtraDamage(200);
            }
        }
    }

    #endregion

    #region Public Methods

    public string GetExtraText(string key)
    {
        if (key.Equals("DREAMERS_INSPECT_RG2"))
            return LoreMaster.Instance.UseHints ? " Through her knowledge she exposes the foes biggest weakness." : " Per 100 Essence you have a 1% chance to instant kill the enemy (capped at 200 damage).";
        else if (key.Equals("DREAMERS_INSPECT_RG3"))
            return LoreMaster.Instance.UseHints ? " His gaze may freeze the enemy in place." : " Roots the target for 3 seconds (15 seconds cooldown)";
        else if (key.Equals("DREAMERS_INSPECT_RG4"))
            return LoreMaster.Instance.UseHints ? " Invoking her children from the victim." : " Spawn 2 weavers for 30 seconds.";
        else if (key.Equals("DREAMERS_INSPECT_RG5"))
            return " ["+PowerName+"] "+ (LoreMaster.Instance.UseHints ? "The dream artifact uses the power it absorbs from their powerful victims to use it's hidden power." : "Defeated Dreamers grant the dream nail an additional effect.");
        return string.Empty;
    }

    #endregion

    #region Protected Methods

    protected override void Initialize() 
        =>_weaverlingPrefab = GameObject.Find("Knight/Charm Effects").LocateMyFSM("Weaverling Control").GetState("Spawn").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
    
    protected override void Enable() 
        => On.EnemyDreamnailReaction.RecieveDreamImpact += EnemyDreamnailReaction_RecieveDreamImpact;
    
    protected override void Disable()
        => On.EnemyDreamnailReaction.RecieveDreamImpact -= EnemyDreamnailReaction_RecieveDreamImpact;
    
    #endregion

    #region Private Methods

    private IEnumerator WeaverLifeTime()
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
