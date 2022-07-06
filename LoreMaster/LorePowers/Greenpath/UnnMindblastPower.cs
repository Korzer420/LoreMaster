using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.Greenpath;

public class UnnMindblastPower : Power
{
    #region Members

    private tk2dSprite[] _dreamNailSprites; 

    #endregion

    #region Constructors

    public UnnMindblastPower() : base("", Area.Greenpath)
    {
        _dreamNailSprites = GameObject.Find("Knight/Dream Effects").GetComponentsInChildren<tk2dSprite>(true);
    }

    #endregion

    #region Event handler

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        MindBlast mindBlast = self.gameObject.GetComponent<MindBlast>();
        if (mindBlast != null && hitInstance.DamageDealt > 0)
            hitInstance.DamageDealt += mindBlast.ExtraDamage;
        orig(self, hitInstance);
    }

    #endregion

    #region Public Methods

    public override void Enable()
    {
        On.EnemyDreamnailReaction.RecieveDreamImpact += EnemyDreamnailReaction_RecieveDreamImpact;
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        ModHooks.CharmUpdateHook += ModHooks_CharmUpdateHook;
    }

    /// <summary>
    /// Updates the dream nail color
    /// </summary>
    /// <param name="data"></param>
    /// <param name="controller"></param>
    private void ModHooks_CharmUpdateHook(PlayerData data, HeroController controller)
    {
        string colorCode = string.Empty;

        colorCode += PlayerData.instance.GetBool("equippedCharm_30") ? 1 : 0;
        colorCode += PlayerData.instance.GetBool("equippedCharm_38") ? 1 : 0;
        colorCode += PlayerData.instance.GetBool("equippedCharm_28") ? 1 : 0;

        Color dreamNailColor;
        switch (colorCode)
        {
            case "001":
                dreamNailColor = Color.green;
                break;
            case "010":
                dreamNailColor = Color.red;
                break;
            case "011":
                dreamNailColor = Color.blue;
                break;
            case "100":
                dreamNailColor = Color.yellow;
                break;
            case "101":
                dreamNailColor = Color.cyan;
                break;
            case "110":
                // Orange
                dreamNailColor = new(1f, 0.4f, 0f);
                break;
            case "111":
                // Purple
                dreamNailColor = new(1f, 0f, 1f);
                break;
            default:
                dreamNailColor = Color.white;
                break;
        }

        // Color all dream nail components accordingly (just for fun)
        foreach (tk2dSprite dreamNailComponent in _dreamNailSprites)
            dreamNailComponent.color = dreamNailColor;
    }

    public override void Disable()
    {
        On.EnemyDreamnailReaction.RecieveDreamImpact -= EnemyDreamnailReaction_RecieveDreamImpact;
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
        ModHooks.CharmUpdateHook -= ModHooks_CharmUpdateHook;

        foreach (tk2dSprite dreamNailComponent in _dreamNailSprites)
            dreamNailComponent.color = Color.white;
    }

    #endregion

    #region Private Methods

    private void EnemyDreamnailReaction_RecieveDreamImpact(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
    {
        orig(self);
        MindBlast mindBlast = self.GetComponent<MindBlast>();
        if (mindBlast == null)
            mindBlast = self.gameObject.AddComponent<MindBlast>();

        mindBlast.ExtraDamage += 2;
        // 1 extra damage if dream wielder is equipped
        if (PlayerData.instance.GetBool("equippedCharm_30"))
            mindBlast.ExtraDamage++;
        // 2 extra damage if dream shield is equipped (like if this is ever going to happen)
        if (PlayerData.instance.GetBool("equippedCharm_38"))
            mindBlast.ExtraDamage += 2;
        // 3 extra damage if shape of unn is equipped
        if(PlayerData.instance.GetBool("equippedCharm_28"))
            mindBlast.ExtraDamage += 3;
    }

    #endregion
}


