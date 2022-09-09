using LoreMaster.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.LorePowers.Crossroads;

public class DramaticEntrancePower : Power
{
    #region Constructors

    public DramaticEntrancePower() : base("Dramatic Entrace", Area.Crossroads)
    {

    }

    #endregion

    #region Control

    protected override void Enable()
    {
        On.HutongGames.PlayMaker.Actions.SendEventByName.OnEnter += SendEventByName_OnEnter;
        On.GGCheckIfBossScene.OnEnter += GGCheckIfBossScene_OnEnter;
    }

    protected override void Disable()
    {
        On.HutongGames.PlayMaker.Actions.SendEventByName.OnEnter -= SendEventByName_OnEnter;
        On.GGCheckIfBossScene.OnEnter -= GGCheckIfBossScene_OnEnter;
    }

    private void GGCheckIfBossScene_OnEnter(On.GGCheckIfBossScene.orig_OnEnter orig, GGCheckIfBossScene self)
    {
        orig(self);
        PrepareForBattle();
    }

    private void SendEventByName_OnEnter(On.HutongGames.PlayMaker.Actions.SendEventByName.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SendEventByName self)
    {
        orig(self);
        if (string.Equals(self.sendEvent.Value, "BATTLE START"))
            PrepareForBattle();
    }

    #endregion

    #region Methods

    private void PrepareForBattle()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("GG_"))
        {
            if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_27)))
            {
                EventRegister.SendEvent("ADD BLUE HEALTH");
                EventRegister.SendEvent("ADD BLUE HEALTH");
            }
            else if (PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) < PlayerData.instance.GetInt(nameof(PlayerData.instance.maxHealth)))
                HeroController.instance.AddHealth(1);
            HeroController.instance.AddMPCharge(33);
        }
        else
        {
            if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_27)))
                for (int i = 0; i < 8; i++)
                {
                    if (PlayerData.instance.GetInt(nameof(PlayerData.instance.healthBlue)) >= 8)
                        break;
                    EventRegister.SendEvent("ADD BLUE HEALTH");
                }
            else
                HeroController.instance.MaxHealthKeepBlue();
            HeroController.instance.AddMPCharge(100);
        }
    }

    #endregion
}
