using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Placements;
using LoreMaster.ItemChangerData.Other;
using LoreMaster.Manager;
using System.Linq;

namespace LoreMaster.ItemChangerData.Locations.SpecialLocations;

/// <summary>
/// A location placed on the shade golem dream dialogue. (The giant corpse which holds shade cloak)
/// </summary>
internal class ShadeGolemDreamLocation : DreamNailLocation
{
    protected override void OnLoad()
    {
        base.OnLoad();
        Events.AddFsmEdit(sceneName, new(GameObjectName, "FSM"), CheckGolemState);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        Events.RemoveFsmEdit(sceneName, new(GameObjectName, "FSM"), CheckGolemState);
    }

    private void CheckGolemState(PlayMakerFSM fsm)
    {
        // Prevent the first shade location from being deactivated if the player has obtained void heart, but not all items at the location
        if(GameObjectName.Contains("01") && fsm.GetState("Destroy") is FsmState state && !Placement.Items.All(x => x.IsObtained()))
            state.Actions = new FsmStateAction[0];
        // Despawn the second location if the first one still has items left. Normally void heart would enable this.
        else if (GameObjectName.Contains("02") && fsm.FsmVariables.FindFsmString("playerData bool")?.Value == "gotShadeCharm"
            && !ItemManager.GetPlacementByName<AutoPlacement>(LocationList.Shade_Golem_Dream_Normal).Items.All(x => x.IsObtained()))
            fsm.GetState("Check").AddLastAction(new Lambda(() => fsm.SendEvent("DEACTIVATE")));
        
    }
}
