using KorzUtils.Helper;
using LoreMaster.Enums;


namespace LoreMaster.LorePowers.Dirtmouth;

/// <summary>
/// Class for the power to cast focus 30% faster.
/// </summary>
public class WellFocusedPower : Power
{
    #region Constructors

    public WellFocusedPower() : base("Well Focused", Area.Dirtmouth) { }

    #endregion

    #region Event handler

    private void PlayerDataBoolTest_OnEnter(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, HutongGames.PlayMaker.Actions.PlayerDataBoolTest self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Slug?"))
            self.Fsm.Variables.FindFsmFloat("Time Per MP Drain").Value *= State == PowerState.Active ? 0.7f : (State == PowerState.Twisted ? 2f : 1f);
        orig(self);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize() => On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += PlayerDataBoolTest_OnEnter;

    /// <inheritdoc/>
    protected override void Terminate() => On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter -= PlayerDataBoolTest_OnEnter;

    #endregion
}
