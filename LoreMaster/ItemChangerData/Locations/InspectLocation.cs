using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Placements;
using ItemChanger.Util;
using LoreMaster.ItemChangerData.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LoreMaster.ItemChangerData.Locations;

/// <summary>
/// A location which can spawn an inspect collider and opens a text box upon inspecting.
/// </summary>
internal class InspectLocation : ContainerLocation
{
    /// <summary>
    /// The position on which the inspect region should be spawned. Unused, if <see cref="GameObjectName"/> is not null/empty.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// The name of the game object, if an inspect region already exists. Using a game object, will make <see cref="Position"/> redundant.
    /// </summary>
    public string GameObjectName { get; set; }

    protected override void OnLoad()
    {
        if (string.IsNullOrEmpty(GameObjectName))
        { 
            Events.AddSceneChangeEdit(sceneName, SpawnInspectRegion);
            Events.AddFsmEdit(sceneName, new(name, "inspect_region"), ModifyInspectRegion);
        }
        else
            Events.AddFsmEdit(sceneName, new(GameObjectName, "inspect_region"), ModifyInspectRegion);
    }

    protected override void OnUnload()
    {
        if (string.IsNullOrEmpty(GameObjectName))
        {
            Events.RemoveSceneChangeEdit(sceneName, SpawnInspectRegion);
            Events.RemoveFsmEdit(sceneName, new(name, "inspect_region"), ModifyInspectRegion);
        }
        else
            Events.RemoveFsmEdit(sceneName, new(GameObjectName, "inspect_region"), ModifyInspectRegion);
    }

    private void ModifyInspectRegion(PlayMakerFSM fsm)
    {
        if (Placement.Items.All(x => x.IsObtained()))
        {
            fsm.GetState("Idle").ClearTransitions();
            return;
        }
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Give Items",
            Actions = new FsmStateAction[]
            {
                new Lambda(() => fsm.GetState("Idle").ClearTransitions()),
                new AsyncLambda(callback => ItemUtility.GiveSequentially(Placement.Items, Placement, new GiveInfo
                {
                    FlingType = flingType,
                    Container = Container.Tablet,
                    MessageType = MessageType.Lore
                }, callback), "CONVO_FINISH")
            }
        });
        fsm.GetState("Give Items").AddTransition("CONVO_FINISH", "Look Up End?");
        fsm.GetState("Hero Look Up?").ClearTransitions();
        fsm.GetState("Hero Look Up?").AddTransition("FINISHED", "Give Items");
    }

    private void SpawnInspectRegion(Scene scene)
    {
        // Also spawn a computer for record bela.
        if (name == LocationList.Lore_Tablet_Record_Bela)
        {
            GameObject tablet = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Glow Response Mage Computer"]);
            tablet.name = "Mage_Computer_2";
            tablet.transform.localPosition = new(70f, 6.21f, .02f);
            tablet.SetActive(true);
        }

        if (Placement.Items.All(x => x.IsObtained()))
            return;
        GameObject inspectRegion = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Inspect Region"]);
        inspectRegion.name = name;
        inspectRegion.transform.localPosition = Position;
        inspectRegion.SetActive(true);
    }
}
