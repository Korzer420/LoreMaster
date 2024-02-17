using LoreMaster.Enums;
using Modding;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using static BossSequenceController;

namespace LoreMaster.LorePowers.WhitePalace;

public class ShiningBoundPower : Power
{
    #region Member

    private ILHook _fakeBinding;

    #endregion
    
    #region Constructors

    public ShiningBoundPower() : base("Shining Bound", Area.WhitePalace)
    {
        CustomText = "When the Wyrm arrised, all kinds of powerful relics were shattered around the world. If we could assemble them again, the energy would flood through us again.<br>";
    }

    #endregion

    #region Properties

    public override PowerRank Rank => PowerRank.Medium;

    #endregion

    #region Event handler

    private void BossSequenceController_ApplyBindings(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);

        if (cursor.TryGotoNext(MoveType.After,
            x => x.MatchCall(typeof(BossSequenceController), "get_BoundCharms")))
            cursor.EmitDelegate<Func<bool, bool>>(x =>
            {
                if (ReflectionHelper.GetField<BossSequenceData>(typeof(BossSequenceController), "currentData") == null)
                    ReflectionHelper.SetField(typeof(BossSequenceController), "currentData", new BossSequenceData());
                return true;
            });
        else
            LoreMaster.Instance.LogError("Couldn't find modification point");
    }

    private void FakeBinding(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);

        if (cursor.TryGotoNext(MoveType.After,
            x => x.MatchCall(typeof(BossSequenceController), "get_BoundCharms")))
            cursor.EmitDelegate<Func<bool, bool>>(x => true);
        else
            LoreMaster.Instance.LogError("Couldn't find modification point");
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable() => _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(GatherShiningSoul());

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        IL.BossSequenceController.ApplyBindings += BossSequenceController_ApplyBindings;
        try
        {
            BossSequenceController.ApplyBindings();
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError(exception.Message);
            LoreMaster.Instance.LogError(exception.StackTrace);
        }
        _fakeBinding = new ILHook(typeof(GGCheckBoundCharms).GetProperty("IsTrue", BindingFlags.Public | BindingFlags.Instance).GetMethod, FakeBinding);
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        if (_fakeBinding != null)
        {
            _fakeBinding.Dispose();
            _fakeBinding = null;
        }
        IL.BossSequenceController.ApplyBindings -= BossSequenceController_ApplyBindings;
        RestoreBindings();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gather soul over time.
    /// </summary>
    private IEnumerator GatherShiningSoul()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            if (PlayerData.instance.equippedCharms.Count > 0)
                HeroController.instance.AddMPCharge(PlayerData.instance.equippedCharms.Count);
        }
    }

    #endregion
}
