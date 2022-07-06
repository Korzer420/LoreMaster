using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.LorePowers.Greenpath;

internal class GiftOnUnnPower : Power
{
    #region Constructors

    public GiftOnUnnPower() : base("", Area.Greenpath)
    {
        
    }

    #endregion

    #region Public Methods

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

    protected override void Enable() { }

    protected override void Disable() { }

    #endregion
}
