using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Helper;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LoreMaster.LorePowers.QueensGarden;

public class QueenThornsPower : Power
{
    #region Members

    private GameObject _thorns;

    private Sprite[] _sprites = new Sprite[2];  

    #endregion

    #region Constructors

    public QueenThornsPower() : base("Queen's Thorns", Area.QueensGarden)
    {
        Hint = "The thorns of agony have received the blessing of the queen.";
        Description = "Thorns of Agony are now \"Queen Thorns\", which removes the freeze on hit. restore soul if it hits an enemy and has a 33% chance to restore 1 hp if it kills an enemy.";
    }

    #endregion

    #region Properties

    public bool CanHeal => PlayerData.instance.GetBool("equippedCharm_27") 
        || (PlayerData.instance.GetInt("Health") < PlayerData.instance.GetInt("maxHealth"));

    #endregion

    #region Event Handler

    private void EnemyTakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        orig(self, hitInstance);
        if (_thorns.activeSelf)
            HeroController.instance.AddMPCharge(7);
    }

    private void EnemyDeath(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        // We assume that if an enemy dies while thorns is active, they killed them.
        if (_thorns.activeSelf && LoreMaster.Instance.Generator.Next(1, 3) == 1 && CanHeal)
        {
            if (PlayerData.instance.GetBool("equippedCharm_27"))
                EventRegister.SendEvent("ADD BLUE HEALTH");
            else
                HeroController.instance.AddHealth(1);
        }
    }

    #endregion

    #region Protected Methods

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
            FsmState currentWorkingState = fsm.GetState("Counter Start");
            currentWorkingState.AddFirstAction(new Lambda(() =>
            {
                // Prevent the freeze in the air
                currentWorkingState.GetFirstActionOfType<SetPosition>().Enabled = !Active;
                // Prevent the gravity from getting removed (this would cause the hero to float until the thorns despawn... probably).
                currentWorkingState.GetActionsOfType<SendMessage>().Take(2).ToList().ForEach(x => x.Enabled = !Active);

            }));

            currentWorkingState = fsm.GetState("Counter");
            currentWorkingState.AddFirstAction(new Lambda(() =>
            {
                // Prevent the freeze in the air
                currentWorkingState.GetFirstActionOfType<SetPosition>().Enabled = !Active;
                // I'm unsure if this is needed, but it can't hurt, right?
                HeroController.instance.RegainControl();
            }));

            _thorns = fsm.transform.Find("Thorn Hit").gameObject;

            // Adjust thorn damage
            foreach (Transform child in _thorns.transform)
            {
                fsm = child.gameObject.LocateMyFSM("set_thorn_damage");
                fsm.GetState("Set").InsertAction(new Lambda(() =>
                {
                    if (Active)
                        fsm.FsmVariables.GetFsmInt("Damage").Value *= 2;
                }), 1);
            }
        }
        catch (Exception error)
        {
            LoreMaster.Instance.LogError("Error in Initialize of Queen Thorns: " + error.Message);
        }
    }

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

    protected override void Disable()
    {
        On.HealthManager.TakeDamage -= EnemyTakeDamage;
        On.HealthManager.Die -= EnemyDeath;
        CharmIconList.Instance.spriteList[12] = _sprites[0];
    }

    #endregion
}
