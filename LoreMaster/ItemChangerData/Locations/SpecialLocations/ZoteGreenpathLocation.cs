using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Manager;
using System.Linq;
using UnityEngine;

namespace LoreMaster.ItemChangerData.Locations.SpecialLocations;

internal class ZoteGreenpathLocation : DialogueLocation
{
    protected override void OnLoad()
    {
        base.OnLoad();
        Events.AddFsmEdit(sceneName, new("Giant Buzzer", "Big Buzzer"), ModifyVengefly);
        Events.AddFsmEdit(sceneName, new("Giant Buzzer", "FSM"), ManualZoteSpawn);
        Events.AddFsmEdit(sceneName, new("Giant Buzzer", "Encounter Control"), ManualZoteSpawn);
        Events.AddFsmEdit(sceneName, new("Corpse Giant Buzzer(Clone)", "corpse"), SetZoteFlag);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        Events.RemoveFsmEdit(sceneName, new("Giant Buzzer", "Big Buzzer"), ModifyVengefly);
        Events.RemoveFsmEdit(sceneName, new("Giant Buzzer", "FSM"), ManualZoteSpawn);
        Events.RemoveFsmEdit(sceneName, new("Giant Buzzer", "Encounter Control"), ManualZoteSpawn);
        Events.RemoveFsmEdit(sceneName, new("Corpse Giant Buzzer", "corpse"), SetZoteFlag);
    }

    private void ModifyVengefly(PlayMakerFSM fsm)
    {
        fsm.AddState(new HutongGames.PlayMaker.FsmState(fsm.Fsm)
        {
            Name = "Check Spawn",
            Actions = new HutongGames.PlayMaker.FsmStateAction[]
            {
                new Lambda(() =>
                {
                    if (LoreManager.Instance.Traveller[Enums.Traveller.Zote].CurrentStage < LoreManager.Instance.Traveller[Enums.Traveller.Zote].Locations.IndexOf("Zote Buzzer Convo(Clone)"))
                        GameObject.Destroy(fsm.FsmVariables.FindFsmGameObject("Zote").Value);
                })
            }
        });
        KorzUtils.Helper.FsmHelper.AdjustTransition(fsm.GetState("Unfurl"), "FINISHED", "Check Spawn");
        fsm.GetState("Check Spawn").AddTransition("FINISHED", "Intro Fly");
    }

    private void ManualZoteSpawn(PlayMakerFSM fsm)
    {
        fsm.AddState(new HutongGames.PlayMaker.FsmState(fsm.Fsm)
        {
            Name = "Check Zote Spawn",
            Actions = new HutongGames.PlayMaker.FsmStateAction[]
            {
                new Lambda(() =>
                {
                    if (LoreManager.Instance.Traveller[Enums.Traveller.Zote].CurrentStage >= LoreManager.Instance.Traveller[Enums.Traveller.Zote].Locations.IndexOf("Zote Buzzer Convo(Clone)")
                    && !Placement.Items.All(x => x.IsObtained()))
                    {
                        GameObject zote = GameObject.Instantiate(fsm.gameObject.LocateMyFSM("Big Buzzer").GetState("Unfurl").GetFirstActionOfType<CreateObject>().gameObject.Value);
                        zote.SetActive(true);
                        zote.transform.position = new Vector3(47.5214f, 13.4081f, 0.004f);
                        zote.LocateMyFSM("Zote Buzzer Control").GetState("Land").AddLastAction(new Lambda(() => PlayMakerFSM.BroadcastEvent("SAVE ZOTE")));
                    }
                })
            }
        });
        if (fsm.FsmName == "FSM")
        {
            KorzUtils.Helper.FsmHelper.AdjustTransition(fsm.GetState("Check"), "DESTROY", "Check Zote Spawn");
            fsm.GetState("Check Zote Spawn").AddTransition("FINISHED", "Destroy");
        }
        else
        {
            KorzUtils.Helper.FsmHelper.AdjustTransition(fsm.GetState("State 1"), "KILLED", "Check Zote Spawn");
            fsm.GetState("Check Zote Spawn").AddTransition("FINISHED", "State 2");
        }
    }

    private void SetZoteFlag(PlayMakerFSM fsm)
    {
        fsm.GetState("Blow").AddLastAction(new Lambda(() => PlayerData.instance.SetBool(nameof(PlayerData.instance.zoteRescuedBuzzer), true)));
    }
}
