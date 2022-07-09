using LoreMaster.Enums;

namespace LoreMaster.LorePowers;

/// <summary>
/// Class for the power to cast focus 30% faster.
/// </summary>
public class WellFocusedPower : Power
{
    #region Members

    private float _baseUnFocusSpeed;

    private float _baseFocusSpeed;

    #endregion

    #region Constructors

    public WellFocusedPower() : base("Well Focused", Area.Dirtmouth)
    {
        Hint = "You gain pure focus faster.";
        Description = "Focus is cast 30% faster.";
    }

    #endregion

    #region Protected Methods

    protected override void Enable()
    {
        PlayMakerFSM playMakerFSM = HeroController.instance.spellControl;
        _baseUnFocusSpeed = playMakerFSM.Fsm.GetFsmFloat("Time Per MP Drain CH").Value;
        _baseFocusSpeed = playMakerFSM.Fsm.GetFsmFloat("Time Per MP Drain CH").Value;

        playMakerFSM.Fsm.GetFsmFloat("Time Per MP Drain UnCH").Value *= 0.7f;
        playMakerFSM.Fsm.GetFsmFloat("Time Per MP Drain CH").Value *= 0.7f;

    }

    protected override void Disable()
    {
        PlayMakerFSM playMakerFSM = HeroController.instance.spellControl;

        playMakerFSM.Fsm.GetFsmFloat("Time Per MP Drain UnCH").Value = _baseUnFocusSpeed;
        playMakerFSM.Fsm.GetFsmFloat("Time Per MP Drain CH").Value = _baseFocusSpeed;
    }

    #endregion
}
