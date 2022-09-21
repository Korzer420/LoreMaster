using LoreMaster.Enums;
using LoreMaster.Manager;
using LoreMaster.Randomizer;
using System;

namespace LoreMaster.LorePowers.Crossroads;

public class DramaticEntrancePower : Power
{
    /*
        Town DeactivateIfPlayerdataTrue + False
        Crossroads 47 -> FSM
        Crossroads 50 -> FSM
        Colo 02 -> FSM
        Deepnest_East_07
     */
    #region Members

    private bool _alreadyEntered;

    #endregion

    #region Constructors

    public DramaticEntrancePower() : base("Dramatic Entrance", Area.Crossroads)
    {
        CustomText = "Ah! I wouldn't have thought that we meet again. Maybe you're not so much of a pityful bug as I expected. But I know exactly why I thought this. "+
            "Your introduction is quite... lame. If you want to conquer in great battles, you have to show the enemy right on the start that they should be feared of you. "+
            "It doesn't matter how huge you are. All that counts is your stance. Let me teach you the great way of entering a fight, so that all know that YOU'RE the biggest threat.";
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += PlayerDataBoolTest_OnEnter;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public override Action SceneAction => () => _alreadyEntered = false;

    /// <summary>
    /// Gets or sets the locations where tiso appears and which tiso level it does require.
    /// </summary>
    //public Dictionary<string, int> TisoAppearances { get; set; } = new()
    //{
    //    {"Town", 1},
    //    {"Crossroads_47", 2},
    //    {"Crossroads_50", 3},
    //    {"Room_Colosseum_02", 4},
    //    {"Deepnest_East_07", 5 } // Tiso's corpse. It is always the last stage.
    //};

    #endregion

    #region Event handler

    private void SendEventByName_OnEnter(On.HutongGames.PlayMaker.Actions.SendEventByName.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SendEventByName self)
    {
        orig(self);
        if (string.Equals(self.sendEvent.Value, "FIGHT START"))
            PrepareForBattle();
    }

    private void SetFsmString_OnEnter(On.HutongGames.PlayMaker.Actions.SetFsmString.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetFsmString self)
    {
        orig(self);
        if (string.Equals(self.variableName.Value, "Area Event") && !string.Equals(self.Fsm.Name, "Conversation Control"))
            PrepareForBattle();
    }

    private void PlayerDataBoolTest_OnEnter(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, HutongGames.PlayMaker.Actions.PlayerDataBoolTest self)
    {
        if (self.Fsm.GameObjectName.StartsWith("Tiso ") && string.Equals(self.Fsm.Name, "FSM")
            && PlayerData.instance.GetBool(nameof(PlayerData.instance.tisoEncounteredTown)) && !PowerManager.ActivePowers.Contains(this)
            && string.Equals("Crossroads_47", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
            if (!RandomizerManager.PlayingRandomizer || !RandomizerManager.Settings.RandomizeNpc)
                self.isTrue = null;
        orig(self);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.HutongGames.PlayMaker.Actions.SendEventByName.OnEnter += SendEventByName_OnEnter;
        On.HutongGames.PlayMaker.Actions.SetFsmString.OnEnter += SetFsmString_OnEnter;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.HutongGames.PlayMaker.Actions.SendEventByName.OnEnter -= SendEventByName_OnEnter;
        On.HutongGames.PlayMaker.Actions.SetFsmString.OnEnter -= SetFsmString_OnEnter;
    }

    #endregion

    #region Methods

    private void PrepareForBattle()
    {
        if (_alreadyEntered)
            return;
        _alreadyEntered = true;
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
