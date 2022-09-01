using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using Modding;
using UnityEngine;

namespace LoreMaster.LorePowers.KingdomsEdge;

public class WisdomOfTheSagePower : Power
{
    #region Members

    private int _soulBonus;

    #endregion

    #region Constructors

    public WisdomOfTheSagePower() : base("Wisdom of the Sage", Area.KingdomsEdge) { }

    #endregion

    #region Event handler

    private void ModHooks_CharmUpdateHook(PlayerData data, HeroController controller) => UpdateSpellCost();

    #endregion

    private void OnSetFsmIntAction(On.HutongGames.PlayMaker.Actions.SetFsmInt.orig_DoSetFsmInt orig, HutongGames.PlayMaker.Actions.SetFsmInt self)
    {
        orig(self);
        if (string.Equals(self.Fsm.FsmComponent.gameObject.name, "Charm Effects") && string.Equals(self.Fsm.FsmComponent.FsmName, "Set Spell Cost"))
        {
            GameObject.Find("Knight").LocateMyFSM("Spell Control").FsmVariables.FindFsmInt("MP Cost").Value -= _soulBonus;
        }
    }

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        On.HutongGames.PlayMaker.Actions.SetFsmInt.DoSetFsmInt += OnSetFsmIntAction;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        ModHooks.CharmUpdateHook += ModHooks_CharmUpdateHook;
        UpdateSpellCost();
    }

    /// <inheritdoc/>
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
