using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using Modding;
using UnityEngine;

namespace LoreMaster.LorePowers.KingdomsEdge;

public class WisdomOfTheSagePower : Power
{
    #region Members

    private int _soulBonus;

    #endregion

    #region Constructors

    public WisdomOfTheSagePower() : base("Wisdom of the Sage", Area.KingdomsEdge)
    {
        Hint = "Guide the Sage through his journey to learn how to use your spells more efficient.";
        Description = "For each Mr. Mushroom stage that you completed, spells cost 1 soul less.";
    }

    #endregion

    #region Event handler

    private void ModHooks_CharmUpdateHook(PlayerData data, HeroController controller) => UpdateSpellCost();

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        // If the player is using spell twister (enable or disable) it triggers this fsm AFTER the charm update hook, which would negate the effect.
        // Therefore we have to add a clauses here too.
        PlayMakerFSM fsm = GameObject.Find("Knight/Charm Effects").LocateMyFSM("Set Spell Cost");
        fsm.GetState("Idle").AddFirstAction(new Lambda(() =>
        {
            if (Active)
                UpdateSpellCost();
        }));
    }

    protected override void Enable() 
    {
        ModHooks.CharmUpdateHook += ModHooks_CharmUpdateHook;
        UpdateSpellCost();
    }

    protected override void Disable() 
    { 
        ModHooks.CharmUpdateHook -= ModHooks_CharmUpdateHook;
        if (_soulBonus != 0)
            GameObject.Find("Knight").LocateMyFSM("Spell Control").FsmVariables.FindFsmInt("MP Cost").Value += _soulBonus;
        _soulBonus = 0;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Updates the spell costs.
    /// </summary>
    private void UpdateSpellCost()
    {
        _soulBonus = PlayerData.instance.GetInt(nameof(PlayerData.instance.mrMushroomState));
        GameObject.Find("Knight").LocateMyFSM("Spell Control").FsmVariables.FindFsmInt("MP Cost").Value -= _soulBonus;
    }

    #endregion
}
