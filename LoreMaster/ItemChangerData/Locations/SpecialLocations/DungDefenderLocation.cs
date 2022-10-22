using ItemChanger;
using ItemChanger.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.ItemChangerData.Locations.SpecialLocations;

internal class DungDefenderLocation : DialogueLocation
{
    protected override void OnLoad()
    {
        base.OnLoad();
        Events.AddFsmEdit(sceneName, new("Dung Defender NPC", "FSM"), (fsm) => fsm.GetState("Check").ClearTransitions());
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        Events.RemoveFsmEdit(sceneName, new("Dung Defender NPC", "FSM"), (fsm) => fsm.GetState("Check").ClearTransitions());
    }
}
