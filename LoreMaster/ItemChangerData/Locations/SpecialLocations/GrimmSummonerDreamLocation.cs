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
    private GameObject _grimmSummoner;
    protected override void OnLoad()
    {
        base.OnLoad();
        Events.AddFsmEdit(sceneName, new(GameObjectName, "FSM"), StoreSummoner);
        Events.AddFsmEdit(sceneName, new("Grimm Arrival Audio", "Control"), ActivateSummoner);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        Events.RemoveFsmEdit(sceneName, new(GameObjectName, "FSM"), StoreSummoner);
        Events.RemoveFsmEdit(sceneName, new("Grimm Arrival Audio", "Control"), ActivateSummoner);
    }

    /// <summary>
    /// Stores the reference of the summoner.
    /// </summary>
    private void StoreSummoner(PlayMakerFSM fsm)
    {
        // Make sure, that this fsm is handled only once.
        if (_grimmSummoner == null)
        {
            fsm.AddState(new FsmState(fsm.Fsm)
            {
                Name = "Clear Repeat",
                Actions = new FsmStateAction[]
                {
                new Lambda(() => fsm.GetState("Check").ClearTransitions())
                }
            });
            fsm.GetState("Check").AdjustTransition("DEACTIVATE", "Clear Repeat");
            fsm.GetState("Clear Repeat").AddTransition("FINISHED", "Deactivate");
        }
        _grimmSummoner = fsm.gameObject;
    }

    /// <summary>
    /// Activates the dream dialogue after the nightmare lantern has been activated.
    /// </summary>
    private void ActivateSummoner(PlayMakerFSM fsm)
    {
        fsm.GetState("Arrive").AddLastAction(new Lambda(() =>
        {
            if (_grimmSummoner != null)
                _grimmSummoner.SetActive(true);
        }));
    }
}
