using Modding;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

    public LifebloodWingsPower() : base("", Area.Cliffs)
    {
        _wings = GameObject.Find("Knight/Effects").transform.Find("Double J Wings").GetComponent<tk2dSprite>();
    }

    #endregion

    public int PlayerBlueHealth => PlayerData.instance.GetInt("healthBlue");

    #region Event handler

    private void HeroController_DoDoubleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
    {
        orig(self);
        if (_extraJumps < PlayerBlueHealth)
        {
            _extraJumps++;
            GameManager.instance.StartCoroutine(RefreshWings());
        }
    }

    #endregion

    #region Public Methods

    public override void Enable()
    {
        _wings.color = Color.cyan;
        AddRefreshHooks();
        On.HeroController.DoDoubleJump += HeroController_DoDoubleJump;
    }

    public override void Disable()
    {
        _wings.color = Color.white;
        RemoveRefreshHooks();
        On.HeroController.DoDoubleJump += HeroController_DoDoubleJump;
    }

    #endregion

    #region Private Methods

    private IEnumerator RefreshWings()
    {
        yield return new WaitUntil(() => !InputHandler.Instance.inputActions.jump.IsPressed);
        ReflectionHelper.SetField(HeroController.instance, "doubleJumped", false);
    }

    private void AddRefreshHooks()
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

    private void RemoveRefreshHooks()
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
    }

    #endregion
}
