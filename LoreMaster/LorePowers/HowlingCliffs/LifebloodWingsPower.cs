using LoreMaster.Enums;
using Modding;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LoreMaster.LorePowers.HowlingCliffs;

public class LifebloodWingsPower : Power
{
    #region Members

    private tk2dSprite _wings;

    private int _extraJumps = 0;

    private readonly List<ILHook> _hooked = new List<ILHook>();

    private readonly string[] _coroutineHooks = new string[]
    {
            "<EnterScene>",
            "<HazardRespawn>",
            "<Respawn>"
    };

    #endregion

    #region Constructors

    public LifebloodWingsPower() : base("Lifeblood Wings", Area.Cliffs)
    {
        Hint = "Lifeblood floods through your wings. The more you have, the stronger they are amplified.";
        Description = "For each lifeblood that you have, you can jump an additional time. Requires Wings";
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the current blue health counter.
    /// </summary>
    public int PlayerBlueHealth => PlayerData.instance.GetInt("healthBlue");

    #endregion

    #region Event handler

    private void DoDoubleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
    {
        orig(self);
        if (ReflectionHelper.GetField<HeroController, bool>(HeroController.instance, "doubleJumped") && _extraJumps < PlayerBlueHealth)
        {
            _wings.color = Color.cyan;
            _extraJumps++;
            LoreMaster.Instance.Handler.StartCoroutine(RefreshWings());
        }
    }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        _wings = GameObject.Find("Knight/Effects").transform.Find("Double J Wings").GetComponent<tk2dSprite>();
    }

    protected override void Enable()
    {
        AddRefreshDoubleJumpHooks();
        On.HeroController.DoDoubleJump += DoDoubleJump;
    }

    protected override void Disable()
    {
        _wings.color = Color.white;
        RemoveRefreshDoubleJumpHooks();
        On.HeroController.DoDoubleJump += DoDoubleJump;
    }

    #endregion

    #region Private Methods

    private IEnumerator RefreshWings()
    {
        yield return new WaitUntil(() => !InputHandler.Instance.inputActions.jump.IsPressed);
        ReflectionHelper.SetField(HeroController.instance, "doubleJumped", false);
    }

    /// <summary>
    /// Adds hooks to reset the extra jump counter of the double jump.
    /// </summary>
    private void AddRefreshDoubleJumpHooks()
    {
        IL.HeroController.BackOnGround += RefreshDoubleJump;
        IL.HeroController.Bounce += RefreshDoubleJump;
        IL.HeroController.BounceHigh += RefreshDoubleJump;
        IL.HeroController.DoWallJump += RefreshDoubleJump;
        IL.HeroController.EnterSceneDreamGate += RefreshDoubleJump;
        IL.HeroController.ExitAcid += RefreshDoubleJump;
        IL.HeroController.LookForInput += RefreshDoubleJump;
        IL.HeroController.ResetAirMoves += RefreshDoubleJump;
        IL.HeroController.ShroomBounce += RefreshDoubleJump;

        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

        foreach (string nested in _coroutineHooks)
        {
            Type nestedType = typeof(HeroController).GetNestedTypes(flags).First(x => x.Name.Contains(nested));
            _hooked.Add(new(nestedType.GetMethod("MoveNext", flags), RefreshDoubleJump));
        }

        _hooked.Add(new(typeof(HeroController).GetMethod("orig_Update", flags), RefreshDoubleJump));
    }

    /// <summary>
    /// Removes all extra hooks for the double jump counter reset.
    /// </summary>
    private void RemoveRefreshDoubleJumpHooks()
    {
        IL.HeroController.BackOnGround -= RefreshDoubleJump;
        IL.HeroController.Bounce -= RefreshDoubleJump;
        IL.HeroController.BounceHigh -= RefreshDoubleJump;
        IL.HeroController.DoWallJump -= RefreshDoubleJump;
        IL.HeroController.EnterSceneDreamGate -= RefreshDoubleJump;
        IL.HeroController.ExitAcid -= RefreshDoubleJump;
        IL.HeroController.LookForInput -= RefreshDoubleJump;
        IL.HeroController.ResetAirMoves -= RefreshDoubleJump;
        IL.HeroController.ShroomBounce -= RefreshDoubleJump;

        foreach (ILHook hook in _hooked)
            hook?.Dispose();
        _hooked.Clear();
    }

    /// <summary>
    /// Refreshes the double jump.
    /// </summary>
    /// <param name="il"></param>
    private void RefreshDoubleJump(ILContext il)
    {
        ILCursor cursor = new(il);

        while (cursor.TryGotoNext(MoveType.After,
            i => i.MatchLdcI4(0),
            i => i.MatchStfld<HeroController>("doubleJumped")
        ))
        {
            cursor.EmitDelegate<Action>(() => _extraJumps = 0);
        }
        _wings.color = Color.white;
    }

    #endregion
}
