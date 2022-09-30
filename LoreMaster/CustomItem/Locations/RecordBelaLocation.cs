using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Util;
using LoreMaster.Helper;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LoreMaster.CustomItem.Locations;

internal class RecordBelaLocation : AutoLocation
{
    protected override void OnLoad() => Events.AddSceneChangeEdit(sceneName, SpawnBela);
    
    protected override void OnUnload() => Events.RemoveSceneChangeEdit(sceneName, SpawnBela);
    
    private void SpawnBela(Scene scene)
    {
        GameObject tablet = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Glow Response Mage Computer"]);
        tablet.name = "Mage_Computer_2";
        tablet.transform.localPosition = new(70f, 6.21f, .02f);
        tablet.SetActive(true);

        GameObject inspectRegion = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Inspect Region"]);
        inspectRegion.name = "Computer_2_Inspect";
        inspectRegion.transform.localPosition = new(70f, 4.31f, .006f);
        inspectRegion.SetActive(true);
        PlayMakerFSM fsm = inspectRegion.LocateMyFSM("inspect_region");

        if (Placement.Items.All(x => x.IsObtained()))
            fsm.FsmVariables.FindFsmString("Game Text Convo").Value = "MAGE_COMP_03";
        else
        {
            fsm.AddState(new FsmState(fsm.Fsm)
            {
                Name = "Give Items",
                Actions = new FsmStateAction[]
                {
                    new AsyncLambda(callback => ItemUtility.GiveSequentially(Placement.Items, Placement, new GiveInfo
                    {
                        FlingType = flingType,
                        Container = Container.Tablet,
                        MessageType = MessageType.Any
                    }, callback), "CONVO_FINISH")
                }
            });
            fsm.GetState("Give Items").AddTransition("CONVO_FINISH", "Look Up End?");
            fsm.GetState("Hero Look Up?").ClearTransitions();
            fsm.GetState("Hero Look Up?").AddTransition("FINISHED", "Give Items");
        }
    }
}
