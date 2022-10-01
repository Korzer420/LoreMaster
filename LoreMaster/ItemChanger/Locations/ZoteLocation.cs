using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.Locations;

namespace LoreMaster.ItemChanger.Locations;

public class ZoteLocation : AutoLocation
{
    protected override void OnLoad()
    {
        Events.AddFsmEdit(sceneName, new("Zote Death", "Activation"), ModifyFsm);
    }

    protected override void OnUnload()
    {
        Events.RemoveFsmEdit(sceneName, new("Zote Death", "Activation"), ModifyFsm);
    }

    private void ModifyFsm(PlayMakerFSM fsm)
    {
        if(string.Equals(fsm.FsmName, "Activation"))
            fsm.GetState("Init").ClearTransitions();
        //else
        //{
        //    fsm.GetState("Zote Trapped?").ClearTransitions();
        //    fsm.GetState("Zote Trapped?").AddTransition("DESTROY", "Cut");
        //}
    }
}
