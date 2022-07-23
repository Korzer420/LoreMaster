using LoreMaster.Enums;
using Modding;

namespace LoreMaster.LorePowers.WhitePalace;

public class DiminishingCursePower : Power
{
    #region Members

    private int _takenHits = 15;

    #endregion

    #region Constructors

    public DiminishingCursePower() : base("Diminishing Curse", Area.WhitePalace)
    {
        Hint = "If you suffer from the curse of greed, it will vanish once you experienced enough pain after resting.";
        Description = "If you take 15 hits, you will no longer count as overcharmed, resets if you sit on a bench. The UI only updates if you open the charm screen.";
    }

    #endregion

    #region Event Handler

    private void HeroController_TakeHealth(On.PlayerData.orig_TakeHealth orig, PlayerData self, int amount)
    {
        orig(self, amount);
        if (amount > 0)
            _takenHits++;
    }

    private bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name.Equals(nameof(PlayerData.instance.overcharmed)) && _takenHits >= 15)
            return false;
        return orig;
    }

    private void HeroController_CharmUpdate(On.HeroController.orig_CharmUpdate orig, HeroController self)
    {
        _takenHits = 0;
        orig(self);
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Enable()
    {
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        On.HeroController.CharmUpdate += HeroController_CharmUpdate;
        On.PlayerData.TakeHealth += HeroController_TakeHealth;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
        On.HeroController.CharmUpdate -= HeroController_CharmUpdate;
        On.PlayerData.TakeHealth -= HeroController_TakeHealth;
    }

    #endregion
}
