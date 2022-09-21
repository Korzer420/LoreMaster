using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
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

    public bool CanHeal => PlayerData.instance.GetBool("equippedCharm_27")
        || (PlayerData.instance.GetInt("Health") < PlayerData.instance.GetInt("maxHealth"));

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

    #endregion

    #region Internal Methods

    /// <summary>
    /// Modify thorns.
    /// </summary>
    internal void ModifyThorns(PlayMakerFSM fsm)
    {
        // Add transition to queen variant.
        fsm.GetState("Check Equipped").ReplaceAction(new Lambda(() =>
        {
            if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_12)))
                fsm.SendEvent(Active ? "QUEEN" : "EQUIPPED");
            else
                fsm.SendEvent("CANCEL");
        })
        {
            Name = "Check for Queen"
        }, 0);

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
        fsm.GetState("Check Equipped").AddTransition("QUEEN", "Queen Counter Start");
        fsm.GetState("Queen Counter Start").AddTransition("FINISHED", "Set Thorn Scale");

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

        // Add a state to control if the normal or queen variant should be executed.
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Queen?",
            Actions = new FsmStateAction[]
            {
                new Lambda(() => fsm.SendEvent(Active ? "QUEEN" : "FINISHED"))
            }
        });
        fsm.GetState("Set Thorn Scale").RemoveTransitionsTo("Counter");
        fsm.GetState("Set Thorn Scale").AddTransition("FINISHED", "Queen?");
        fsm.GetState("Queen?").AddTransition("QUEEN", "Queen Counter");
        fsm.GetState("Queen?").AddTransition("FINISHED", "Counter");
        fsm.GetState("Queen Counter").AddTransition("FINISHED", "Counter End");
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        try
        {
            // Save the old thorns image and create a new one which can be used anytime.
            if (_sprites[1] == null)
            {
                _sprites[0] = CharmIconList.Instance.spriteList[12];
                _sprites[1] = SpriteHelper.CreateSprite("Queens_Thorns");
            }

            PlayMakerFSM fsm = GameObject.Find("Knight/Charm Effects").LocateMyFSM("Thorn Counter");
            // Adjust thorn damage
            _thorns = fsm.transform.Find("Thorn Hit").gameObject;
            foreach (Transform child in _thorns.transform)
            {
                fsm = child.gameObject.LocateMyFSM("set_thorn_damage");
                fsm.GetState("Set").ReplaceAction(new Lambda(() =>
                {
                    fsm.FsmVariables.GetFsmInt("Damage").Value = PlayerData.instance.GetInt(nameof(PlayerData.instance.nailDamage));
                    if (Active)
                        fsm.FsmVariables.GetFsmInt("Damage").Value *= 2;
                }), 0);
            }
        }
        catch (Exception error)
        {
            LoreMaster.Instance.LogError("Error in Initialize of Queen Thorns: " + error.Message);
        }
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.HealthManager.TakeDamage += EnemyTakeDamage;
        On.HealthManager.Die += EnemyDeath;
        try
        {
            CharmIconList.Instance.spriteList[12] = _sprites[1];
        }
        catch (Exception error)
        {
            LoreMaster.Instance.LogError("Error in enable of Queen Thorns: " + error.Message);
        }
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.HealthManager.TakeDamage -= EnemyTakeDamage;
        On.HealthManager.Die -= EnemyDeath;
        CharmIconList.Instance.spriteList[12] = _sprites[0];
    }

    #endregion
}
