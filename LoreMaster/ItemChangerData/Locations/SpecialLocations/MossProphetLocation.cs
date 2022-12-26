using ItemChanger;
using ItemChanger.Extensions;
using System.Linq;
using UnityEngine;

namespace LoreMaster.ItemChangerData.Locations.SpecialLocations;

internal class MossProphetLocation : DialogueLocation
{
    protected override void OnLoad()
    {
        base.OnLoad();
        Events.AddFsmEdit(new FsmID("Moss Cultist", "FSM"), PreventMossDeath);
        Events.AddFsmEdit(new FsmID("corpse set", "FSM"), DisableCorpse);
    }

    protected override void OnUnload()
    {
        Events.RemoveFsmEdit(new FsmID("Moss Cultist", "FSM"), PreventMossDeath);
        Events.RemoveFsmEdit(new FsmID("corpse set", "FSM"), DisableCorpse);
    }

    private void PreventMossDeath(PlayMakerFSM fsm)
    {
        if (!Placement.Items.Any() || Placement.Items.All(x => x.IsObtained()))
            return;
        fsm.GetState("Check").RemoveTransitionsTo("Destroy");
        fsm.gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }

    private void DisableCorpse(PlayMakerFSM fsm) => fsm.gameObject.FindChild("corpse0000").SetActive(false);
}
