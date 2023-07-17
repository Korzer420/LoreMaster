using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KorzUtils.Helper;
using LoreMaster.Enums;


using System;
using UnityEngine;

namespace LoreMaster.LorePowers.QueensGarden;

public class QueenThornsPower : Power
{
    #region Members

    private GameObject _thorns;

    private Sprite[] _sprites = new Sprite[2];

    #endregion

    #region Constructors

    public QueenThornsPower() : base("Queen's Thorns", Area.QueensGarden) { }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the value which indicates if the player can heal (current health smaller than max health)
    /// </summary>
    public bool CanHeal => PlayerData.instance.GetBool("equippedCharm_27")
        || (PlayerData.instance.GetInt("Health") < PlayerData.instance.GetInt("maxHealth"));

    /// <summary>
    /// Gets the thorn object-
    /// </summary>
    public GameObject Thorns => _thorns == null ? _thorns = HeroController.instance.transform.Find("Charm Effects/Thorn Hit").gameObject : _thorns;

    #endregion

    #region Event Handler

    private void EnemyTakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        orig(self, hitInstance);
        if (Thorns.activeSelf)
            HeroController.instance.AddMPCharge(7);
    }

    private void EnemyDeath(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        // We assume that if an enemy dies while thorns is active, they killed them.
        if (Thorns.activeSelf && LoreMaster.Instance.Generator.Next(1, 3) == 1 && CanHeal)
        {
            if (PlayerData.instance.GetBool("equippedCharm_27"))
                EventRegister.SendEvent("ADD BLUE HEALTH");
            else
                HeroController.instance.AddHealth(1);
        }
    }

    private void ActivateGameObject_OnEnter(On.HutongGames.PlayMaker.Actions.ActivateGameObject.orig_OnEnter orig, HutongGames.PlayMaker.Actions.ActivateGameObject self)
    {
        orig(self);
        if (self.IsCorrectContext("Thorn Counter", "Charm Effects", "Counter"))
            self.Fsm.Variables.FindFsmGameObject("Thorn Hit").Value.SetActive(false);
    }

    private void PlayerDataBoolTest_OnEnter(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, HutongGames.PlayMaker.Actions.PlayerDataBoolTest self)
    {
        if (self.IsCorrectContext("Thorn Counter", "Charm Effects", "Check Equipped"))
            self.Fsm.FsmComponent.SendEvent("EQUIPPED");
        orig(self);
    }

    private void SetFsmInt_OnEnter(On.HutongGames.PlayMaker.Actions.SetFsmInt.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetFsmInt self)
    {
        if (self.IsCorrectContext("set_thorn_damage", null, "Set") && string.Equals(self.Fsm.FsmComponent.transform.parent?.name, "Thorn Hit"))
            self.setValue.Value *= 2;
        orig(self);
    }

    #endregion

    #region Internal Methods

    /// <summary>
    /// Modify thorns.
    /// </summary>
    internal void ModifyThorns(PlayMakerFSM fsm)
    {
        // Add branch state to differ between normal and modified execution
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Check for Queen",
            Actions = new FsmStateAction[]
            {
                new Lambda(() => fsm.SendEvent(State == PowerState.Active ? "QUEEN" : "FINISHED"))
            }
        });

        // Create a copy of the counter start for the queen variant.
        FsmState currentWorkingState = fsm.GetState("Counter Start");
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Queen Counter Start",
            Actions = new FsmStateAction[]
            {
                currentWorkingState.Actions[0], //Get position from thorns
                // currentWorkingState.Actions[1], Set position of hero
                currentWorkingState.Actions[2], //Set velocity
                // currentWorkingState.Actions[3], Remove gravity
                // currentWorkingState.Actions[4], Relinquish control
                currentWorkingState.Actions[5], // Stop Animation Control
                currentWorkingState.Actions[6], // Play Audio
                currentWorkingState.Actions[7], // Play Thorn Attack
                currentWorkingState.Actions[8], // Wait
            }
        });

        // Create a copy of the thorn scale for the queen variant.
        // (This is an exact copy which is just implemented to prevent the need of adding an extra state or transition + action)
        currentWorkingState = fsm.GetState("Set Thorn Scale");
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Queen Set Thorn Scale",
            Actions = currentWorkingState.Actions
        });

        // Create a copy of the counter for the queen variant.
        currentWorkingState = fsm.GetState("Counter");
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Queen Counter",
            Actions = new FsmStateAction[]
            {
                currentWorkingState.Actions[0], // Wait for animation
                // currentWorkingState.Actions[1], Set position of hero
                currentWorkingState.Actions[2], // Screen shake
                currentWorkingState.Actions[3], //Set Velocity
                currentWorkingState.Actions[4], //Activate thorn hit
                currentWorkingState.Actions[5], // Wait
            }
        });

        fsm.GetState("Check Equipped").AdjustTransition("EQUIPPED", "Check for Queen");
        fsm.GetState("Check for Queen").AddTransition("QUEEN", "Queen Counter Start");
        fsm.GetState("Check for Queen").AddTransition("FINISHED", "Counter Start");
        fsm.GetState("Queen Counter Start").AddTransition("FINISHED", "Queen Set Thorn Scale");
        fsm.GetState("Queen Set Thorn Scale").AddTransition("FINISHED", "Queen Counter");
        fsm.GetState("Queen Counter").AddTransition("FINISHED", "Counter End");
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        // Save the old thorns image and create a new one which can be used anytime.
        if (_sprites[1] == null)
        {
            _sprites[0] = CharmIconList.Instance.spriteList[12];
            _sprites[1] = SpriteHelper.CreateSprite<LoreMaster>("Base.Queens_Thorns");
        }
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.HealthManager.TakeDamage += EnemyTakeDamage;
        On.HealthManager.Die += EnemyDeath;
        CharmIconList.Instance.spriteList[12] = _sprites[1];
        On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter += SetFsmInt_OnEnter;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.HealthManager.TakeDamage -= EnemyTakeDamage;
        On.HealthManager.Die -= EnemyDeath;
        CharmIconList.Instance.spriteList[12] = _sprites[0];
        On.HutongGames.PlayMaker.Actions.SetFsmInt.OnEnter -= SetFsmInt_OnEnter;
    }

    /// <inheritdoc/>
    protected override void TwistEnable() 
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += PlayerDataBoolTest_OnEnter;
        On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter += ActivateGameObject_OnEnter;
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter -= PlayerDataBoolTest_OnEnter;
        On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter -= ActivateGameObject_OnEnter;
    }

    #endregion
}
