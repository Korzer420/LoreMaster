using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
using System;
using System.Collections;
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
        Description = "When you cast a spell while your soul vessel is full (not counting additional soul vessels), they deal twice as much damage and are twice as big. (Fireball damage is only increased by 40%)";
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
        if (self.gameObject.name.Contains("Spiral"))
            setDamageState.ReplaceAction(new Lambda(() =>
            {
                float direction = self.FsmVariables.FindFsmFloat("X Scale").Value > 0 ? -.8f : .8f;
                if (_hasFullSoulMeter && Active)
                {
                    self.gameObject.LocateMyFSM("damages_enemy").FsmVariables.GetFsmInt("damageDealt").Value *= 2;
                    self.transform.localScale = new(self.transform.localScale.x + 1f, self.transform.localScale.y + 1f);
                    self.GetComponent<tk2dSprite>().color = Color.cyan;
                    // Roll to determine if the player takes damage.
                    if (LoreMaster.Instance.Generator.Next(1, 21) == 1)
                        LoreMaster.Instance.Handler.StartCoroutine(Overload());
                }
                else
                    self.GetComponent<tk2dSprite>().color = Color.white;

                if (direction < 0f)
                    self.SendEvent("LEFT");
                else
                    self.SendEvent("RIGHT");
            })
            {
                Name = "Overpower Fireball"
            }, 3);
        else
            setDamageState.ReplaceAction(new Lambda(() =>
            {
                if (_hasFullSoulMeter && Active)
                {
                    self.gameObject.LocateMyFSM("damages_enemy").FsmVariables.GetFsmInt("damageDealt").Value *= 2;
                    self.transform.localScale = new(self.transform.localScale.x + 1f, self.transform.localScale.y + 1f);
                    self.GetComponent<tk2dSprite>().color = Color.cyan;
                    // Roll to determine if the player takes damage.
                    if (LoreMaster.Instance.Generator.Next(1, 21) == 1)
                        LoreMaster.Instance.Handler.StartCoroutine(Overload());
                }
                else
                    self.GetComponent<tk2dSprite>().color = Color.white;

                if (self.FsmVariables.FindFsmFloat("Velocity").Value < 0f)
                    self.SendEvent("LEFT");
                else
                    self.SendEvent("RIGHT");
            })
            {
                Name = "Overpower Fireball"
            }, 1);
        orig(self);
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        GameObject spell = GameObject.Find("Knight/Spells");

        PlayMakerFSM spellControl = FsmHelper.GetFSM("Knight", "Spell Control");
        spellControl.GetState("Spell Choice").ReplaceAction(new Lambda(() =>
        {
            _hasFullSoulMeter = PlayerData.instance.GetInt(nameof(PlayerData.instance.MPCharge)) >= 99;
            if (spellControl.FsmVariables.FindFsmBool("Pressed Up").Value)
                spellControl.SendEvent("SCREAM");
        })
        { Name = "Full Soul Check" }, 0);
        spellControl.GetState("Can Cast? QC").ReplaceAction(new Lambda(() =>
        {
            _hasFullSoulMeter = PlayerData.instance.GetInt(nameof(PlayerData.instance.MPCharge)) >= 99;
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
            spellDamageFSM.GetState("Finished").ReplaceAction(new Lambda(() =>
            {
                // The transform checks are for the case that this modifications get applied before acquiring Grasp of Life.
                if (_hasFullSoulMeter && Active && spellObject.transform.parent != null && spellObject.transform.parent.parent != null && spellObject.transform.parent.parent.name.Equals("Spells"))
                {
                    spellObject.LocateMyFSM("damages_enemy").FsmVariables.GetFsmInt("damageDealt").Value *= 2;
                    spell.transform.localScale = new(2, 2);
                    spellObject.transform.parent.GetComponent<tk2dSprite>().color = Color.cyan;
                    // Roll to determine if the player takes damage.
                    if (LoreMaster.Instance.Generator.Next(1, 21) == 1)
                        LoreMaster.Instance.Handler.StartCoroutine(Overload());
                }
                else if(spellObject.transform.parent != null && spellObject.transform.parent.parent != null && spellObject.transform.parent.parent.name.Equals("Spells"))
                {
                    spell.transform.localScale = new(1, 1);
                    spellObject.transform.parent.GetComponent<tk2dSprite>().color = Color.white;
                }

                if (!PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_19)))
                    spellDamageFSM.SendEvent("FINISHED");
            })
            { Name = "Empower Spell" });
        }
    }

    /// <inheritdoc/>
    protected override void Enable() => On.PlayMakerFSM.OnEnable += CheckFireball;

    /// <inheritdoc/>
    protected override void Disable() => On.PlayMakerFSM.OnEnable -= CheckFireball;

    #endregion

    #region Private Methods

    private IEnumerator Overload()
    {
        HeroController.instance.TakeDamage(HeroController.instance.gameObject, GlobalEnums.CollisionSide.top, 2, 0);
        PlayerData.instance.StartSoulLimiter();
        PlayMakerFSM.BroadcastEvent("SOUL LIMITER UP");
        yield return new WaitForSeconds(LoreMaster.Instance.Generator.Next(30, 121));
        // The shade spawn is determined by the soul limiter, which means to prevent the shade from spawning until the effect is over.
        if (string.Equals(PlayerData.instance.shadeScene, "None")
            || (LoreMaster.Instance.ActivePowers.ContainsKey("ABYSS_TUT_TAB_01") && LoreMaster.Instance.ActivePowers["ABYSS_TUT_TAB_01"].Active))
        { 
            PlayerData.instance.EndSoulLimiter();
            PlayMakerFSM.BroadcastEvent("SOUL LIMITER DOWN");
        }
    }

    #endregion
}
