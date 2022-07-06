using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.LorePowers.WhitePalace;

public class DiminishingCursePower : Power
{
    #region Members

    private int _takenHits = 15;

    #endregion
    
    #region Constructors

    public DiminishingCursePower() : base("",Area.WhitePalace)
    {
        
    }

    #endregion

    #region Event handler

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

    #region Public Methods

    public override void Enable()
    {
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        On.HeroController.CharmUpdate += HeroController_CharmUpdate;
        On.PlayerData.TakeHealth += HeroController_TakeHealth;
    }

    public override void Disable()
    {
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
        On.HeroController.CharmUpdate -= HeroController_CharmUpdate;
        On.PlayerData.TakeHealth -= HeroController_TakeHealth;
    }

    #endregion
}
