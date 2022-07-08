using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;

namespace LoreMaster.LorePowers.Greenpath;

public class GiftOnUnnPower : Power
{
    #region Constructors

    public GiftOnUnnPower() : base("Gift of Unn", Area.Greenpath)
    {
        Hint = "Grants you the power of Unn.";
        Description = "Gain the shape of Unn effect for focusing. If you're wearing Shape of Unn, focus restores 15 soul on a successful cast.";
    }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        PlayMakerFSM spellFsm = FsmHelper.GetFSM("Knight", "Spell Control");

        // This is the check for shape of unn
        FsmHelper.GetState(spellFsm, "Start Slug Anim").RemoveAction(1);
        FsmHelper.GetState(spellFsm, "Start Slug Anim").InsertAction(new Lambda(() =>
        {
            if (!Active)
                spellFsm.SendEvent("FINISHED");
        }), 1);

        FsmHelper.GetState(spellFsm, "Slug?").AddFirstAction(new Lambda(() =>
        {
            if (Active)
                spellFsm.SendEvent("SLUG");
        }));

        // If the player has shape of unn equipped, it gives 15 mp on a successful cast (this is added, to prevent making the charm useless)
        FsmHelper.GetState(spellFsm, "Focus Heal 2").AddFirstAction(new Lambda(() =>
        {
            if (Active && PlayerData.instance.GetBool("equippedCharm_28"))
                HeroController.instance.AddMPCharge(15);
        }));
    }

    #endregion
}
