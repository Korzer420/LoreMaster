using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.ItemChangerData.Locations.SpecialLocations;

/// <summary>
/// The dream dialogue location of the grimm summoner bug (The bug which needs to be dream nailed to summon the grimm troupe)
/// </summary>
internal class GrimmSummonerDreamLocation : DreamNailLocation
{
    protected override void OnLoad()
    {
        base.OnLoad();
        Events.AddFsmEdit(sceneName, new(GameObjectName, "FSM"), SummonerSpawn);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        Events.RemoveFsmEdit(sceneName, new(GameObjectName, "FSM"), SummonerSpawn);
    }

    /// <summary>
    /// Stores the reference of the summoner.
    /// </summary>
    private void SummonerSpawn(PlayMakerFSM fsm)
    {
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Clear Repeat",
            Actions = new FsmStateAction[]
            {
                new Lambda(() => 
                {
                    if (!PlayerData.instance.GetBool(nameof(PlayerData.instance.nightmareLanternLit)))
                        fsm.SendEvent("DEACTIVATE");
                })
            }
        });
        fsm.GetState("Check").AdjustTransition("DEACTIVATE", "Clear Repeat");
        fsm.GetState("Clear Repeat").AddTransition("DEACTIVATE", "Deactivate");
    }
}
