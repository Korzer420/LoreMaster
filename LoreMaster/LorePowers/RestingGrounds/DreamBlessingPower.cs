using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Helper;
using LoreMaster.Manager;
using LoreMaster.UnityComponents;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoreMaster.LorePowers.RestingGrounds;

public class DreamBlessingPower : Power
{
    #region Members

    private GameObject _weaverlingPrefab;

    private List<GameObject> _spawnedWeavers = new();

    #endregion

    #region Constructors

    public DreamBlessingPower() : base("Dream Blessing", Area.RestingGrounds) { }

    #endregion

    #region Properties

    public GameObject WeaverPrefab => _weaverlingPrefab == null ? _weaverlingPrefab = GameObject.Find("Knight/Charm Effects").LocateMyFSM("Weaverling Control").GetState("Spawn").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value : _weaverlingPrefab;

    #endregion

    #region Event Handler

    private void EnemyDreamnailReaction_RecieveDreamImpact(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
    {
        orig(self);

        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.lurienDefeated)))
            if (self.GetComponent<EnemyBinding>() == null)
                self.gameObject.AddComponent<EnemyBinding>();

        // Herrah... don't ask.
        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.hegemolDefeated)) && !self.gameObject.name.Contains("Prayer Slug"))
            if (_spawnedWeavers.Count < 20)
                for (int i = 0; i < (PlayerData.instance.GetBool(nameof(PlayerData.instance.dreamNailUpgraded)) ? 4 : 2); i++)
                    _spawnedWeavers.Add(GameObject.Instantiate(WeaverPrefab, HeroController.instance.transform.position, Quaternion.identity));

        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.monomonDefeated)) && !self.gameObject.name.Contains("Prayer Slug"))
        {
            int essence = PlayerData.instance.GetInt(nameof(PlayerData.instance.dreamOrbs));

            if (essence > 2400)
                essence = 2400;
            essence /= 100;
            if (LoreMaster.Instance.Generator.Next(1, 101) <= essence)
            {
                // This assumes that the component is on the same object, if not we ignore it. (It isn't worth the hussle to account for that currently)
                HealthManager healthManager = self.GetComponent<HealthManager>();
                int damageAmount = PlayerData.instance.GetBool(nameof(PlayerData.instance.dreamNailUpgraded)) ? 250 : 125;
                if (healthManager != null && healthManager.hp <= damageAmount)
                    healthManager.ApplyExtraDamage(damageAmount);
            }
        }
    }

    private void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        HeroController.instance.GetComponent<tk2dSpriteAnimator>().GetClipByName("DN Slash Antic").fps = 6;
        HeroController.instance.transform.Find("Dream Effects/Slash").GetComponent<tk2dSpriteAnimator>().GetClipByName("DN Antic").fps = 6;
    }

    #endregion

    #region Public Methods

    public string GetExtraText(string key)
    {
        if (key.Equals("DREAMERS_INSPECT_RG2"))
            return LoreManager.Instance.UseHints ? " Through her knowledge she exposes the foes biggest weakness." : " Per 100 Essence you have a 1% chance to instant kill the enemy (capped at 175 damage).";
        else if (key.Equals("DREAMERS_INSPECT_RG3"))
            return LoreManager.Instance.UseHints ? " His gaze may freeze the enemy in place." : " Roots the target for 3 seconds (15 seconds cooldown)";
        else if (key.Equals("DREAMERS_INSPECT_RG4"))
            return LoreManager.Instance.UseHints ? " Invoking her children from her victim." : " Spawn 2 weavers for the current room.";
        else if (key.Equals("DREAMERS_INSPECT_RG5"))
            return " [" + PowerName + "] " + (LoreManager.Instance.UseHints ? "The dream artifact uses the power it absorbs from their powerful victims to use it's hidden power." : "Defeated Dreamers grant the dream nail an additional effect (doubled with awoken dreamnail).");
        return string.Empty;
    }

    #endregion

    #region Event handler

    private void NerfSoulAmount(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);

        if (cursor.TryGotoNext(MoveType.After,
            x => x.MatchLdcI4(33)))
        {
            cursor.EmitDelegate<Func<int, int>>(x =>
            {
                x = 10;
                if (PlayerData.instance.GetBool("monomonDefeated"))
                    x += 5;
                if (PlayerData.instance.GetBool("lurienDefeated"))
                    x += 5;
                if (PlayerData.instance.GetBool("hegemolDefeated"))
                    x += 5;
                return x;
            });

            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchLdcI4(66)))
                cursor.EmitDelegate<Func<int, int>>(x =>
                {
                    x = 20;
                    if (PlayerData.instance.GetBool("monomonDefeated"))
                        x += 10;
                    if (PlayerData.instance.GetBool("lurienDefeated"))
                        x += 10;
                    if (PlayerData.instance.GetBool("hegemolDefeated"))
                        x += 10;
                    return x;
                });
            else
                LoreMaster.Instance.Log("Couldn't find soul amount 2");
        }
        else
            LoreMaster.Instance.Log("Couldn't find soul amount");
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
           => On.EnemyDreamnailReaction.RecieveDreamImpact += EnemyDreamnailReaction_RecieveDreamImpact;

    /// <inheritdoc/>
    protected override void Disable()
           => On.EnemyDreamnailReaction.RecieveDreamImpact -= EnemyDreamnailReaction_RecieveDreamImpact;

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        HeroController.instance.GetComponent<tk2dSpriteAnimator>().GetClipByName("DN Slash Antic").fps = 6;
        HeroController.instance.transform.Find("Dream Effects/Slash").GetComponent<tk2dSpriteAnimator>().GetClipByName("DN Antic").fps = 6;
        On.HeroController.Start += HeroController_Start;
        IL.EnemyDreamnailReaction.RecieveDreamImpact += NerfSoulAmount;
    }

    protected override void TwistDisable()
    {
        HeroController.instance.GetComponent<tk2dSpriteAnimator>().GetClipByName("DN Slash Antic").fps = 12;
        HeroController.instance.transform.Find("Dream Effects/Slash").GetComponent<tk2dSpriteAnimator>().GetClipByName("DN Antic").fps = 12;
        On.HeroController.Start -= HeroController_Start;
        IL.EnemyDreamnailReaction.RecieveDreamImpact -= NerfSoulAmount;
    }

    #endregion
}
