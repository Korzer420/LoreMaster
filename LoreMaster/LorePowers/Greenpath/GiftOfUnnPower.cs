using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;

namespace LoreMaster.LorePowers.Greenpath;

public class GiftOfUnnPower : Power
{
    #region Constructors

    public GiftOfUnnPower() : base("Gift of Unn", Area.Greenpath)
    {
        Hint = "Grants you the power of Unn.";
        Description = "Gain the shape of Unn effect for focusing. If you're wearing Shape of Unn, focus restores 15 soul on a successful cast.";
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
 protected override void Initialize()
    {
        PlayMakerFSM spellFsm = FsmHelper.GetFSM("Knight", "Spell Control");

        // This is the check for shape of unn
        FsmHelper.GetState(spellFsm, "Start Slug Anim").ReplaceAction(new Lambda(() =>
        {
            if (!Active && !PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_28)))
                spellFsm.SendEvent("FINISHED");
        })
        { Name = "Gift of Unn"}, 1);

        FsmHelper.GetState(spellFsm, "Slug?").ReplaceAction(new Lambda(() =>
        {
            if (Active || PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_28)))
                spellFsm.SendEvent("SLUG");
            else
                spellFsm.SendEvent("FINISHED");
            
        })
        { Name = "Force Unn"},0);
        
        // If the player has shape of unn equipped, it gives 15 mp on a successful cast (this is added, to prevent making the charm useless)
        FsmHelper.GetState(spellFsm, "Focus Heal 2").ReplaceAction(new Lambda(() =>
        {
            if (Active && PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_28)))
                HeroController.instance.AddMPCharge(15);
            spellFsm.FsmVariables.FindFsmInt("Max HP").Value = PlayerData.instance.GetInt(nameof(PlayerData.instance.maxHealth));
        })
        { Name = "Soul Recover"}, 14);
    }

    #endregion
}
