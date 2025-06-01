using ItemChanger;
using ItemChanger.FsmStateActions;
using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.Manager;
using System.Linq;

namespace LoreMaster.ItemChangerData.Locations.SpecialLocations;

/// <summary>
/// Again, a stitched location from a dream nail and traveller location.
/// </summary>
internal class TisoCorpseLocation : DreamNailLocation
{
    protected override void OnLoad()
    {
        base.OnLoad();
        Events.AddFsmEdit(sceneName, new("tiso_corpse", "FSM"), ControlCorpseSpawn);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        Events.RemoveFsmEdit(sceneName, new("tiso_corpse", "FSM"), ControlCorpseSpawn);
    }

    private void ControlCorpseSpawn(PlayMakerFSM fsm)
    {
        fsm.GetState("Check").Actions =
        [
            new Lambda(() => 
            {
                if (LoreManager.Instance.Traveller[Traveller.Tiso].CurrentStage < 4 || Placement.Items.All(x => x.IsObtained()))
                    fsm.SendEvent("DEACTIVATE");
            })
        ];
    }
}
