using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

public class OverwhelmingPower : Power
{
    #region Members

    private bool _hasFullSoulMeter;

    #endregion

    #region Constructors

    public OverwhelmingPower() : base("Overwhelming Power", Area.CityOfTears)
    {
        Hint = "Casting spells with full capacity will grant your spell much more power";
        Description = "When you cast a spell while your soul vessel is full (not counting additional soul vessels), they deal twice as much damage and are twice as big.";
    }

    #endregion

    #region Event Handler

    /// <summary>
    /// Modifies the fireball if we catch the fireball
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    private void CheckFireball(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (!self.FsmName.Equals("Fireball Control"))
        {
            orig(self);
            return;
        }

        FsmState setDamageState = self.GetState("Init");
        if (setDamageState.Actions[0].Name.Equals("Overpower Fireball"))
            LoreMaster.Instance.Log("Already added action");
        else
            setDamageState.InsertAction(new Lambda(() =>
            {
                if (_hasFullSoulMeter && Active)
                {
                    self.gameObject.LocateMyFSM("damages_enemy").FsmVariables.GetFsmInt("damageDealt").Value *= 2;
                    self.transform.localScale = new(self.transform.localScale.x + .8f, self.transform.localScale.y + .8f);
                    self.GetComponent<tk2dSprite>().color = Color.cyan;
                }
                else
                    self.GetComponent<tk2dSprite>().color = Color.white;
            })
            {
                IsAutoNamed = false,
                Name = "Overpower Fireball"
            }, 0);
        orig(self);
    }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        GameObject spell = GameObject.Find("Knight/Spells");

        PlayMakerFSM spellControl = FsmHelper.GetFSM("Knight", "Spell Control");
        spellControl.GetState("Spell Choice").ReplaceAction(new Lambda(() =>
        {
            _hasFullSoulMeter = PlayerData.instance.MPCharge >= 99;
            if (spellControl.FsmVariables.FindFsmBool("Pressed Up").Value)
                spellControl.SendEvent("SCREAM");
        }) { Name = "Full Soul Check"}, 0);
        spellControl.GetState("Can Cast? QC").ReplaceAction(new Lambda(() =>
        {
            _hasFullSoulMeter = PlayerData.instance.MPCharge >= 99;
            spellControl.FsmVariables.FindFsmInt("MP").Value = PlayerData.instance.GetInt("MPCharge");
        })
        { Name = "Full Soul Check" }, 1);

        List<GameObject> spells = new();

        // Get all spell objects (all objects with the hero spell tag, should have the Set Damage fsm)
        foreach (Transform child in spell.transform)
        {
            if (child.tag.Equals("Hero Spell"))
                spells.Add(child.gameObject);
            foreach (Transform subChild in child)
                if (subChild.tag.Equals("Hero Spell"))
                    spells.Add(subChild.gameObject);

        }

        // Modify the damage fsm of each spell
        foreach (GameObject spellObject in spells)
        {
            PlayMakerFSM spellDamageFSM = spellObject.LocateMyFSM("Set Damage");
            if (spellDamageFSM == null)
                continue;
            spellDamageFSM.GetState("Finished").AddFirstAction(new Lambda(() =>
            {
                if (_hasFullSoulMeter && Active)
                {
                    spellObject.LocateMyFSM("damages_enemy").FsmVariables.GetFsmInt("damageDealt").Value *= 2;
                    spell.transform.localScale = new(2, 2);
                    spell.GetComponent<tk2dSprite>().color = Color.cyan;
                }
                else
                {
                    spell.transform.localScale = new(1, 1);
                    spell.GetComponent<tk2dSprite>().color = Color.white;
                }
            }));
        }
    }

    protected override void Enable() => On.PlayMakerFSM.OnEnable += CheckFireball;

    protected override void Disable() => On.PlayMakerFSM.OnEnable -= CheckFireball; 

    #endregion
}
