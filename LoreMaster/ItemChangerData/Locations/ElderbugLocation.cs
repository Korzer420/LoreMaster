using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Util;
using LoreMaster.ItemChangerData.Other;
using LoreMaster.Randomizer;
using System;
using UnityEngine;

namespace LoreMaster.ItemChangerData.Locations;

internal class ElderbugLocation : AutoLocation
{
    #region Properties

    /// <summary>
    /// Gets or sets the flag that indicates if Elderbug has already thrown an item.
    /// </summary>
    public static bool ItemThrown { get; set; }

    #endregion

    #region Methods

    protected override void OnLoad()
    {
        Events.AddFsmEdit("Town", new("Elderbug", "Conversation Control"), ModifyElderbug);
        LoreMaster.Instance.Log("Elderbug location name is: " + name);
        if (name == LocationList.Elderbug_Reward_Prefix + "1")
            Events.AddSceneChangeEdit("Town", a =>
            {
                if (!RandomizerManager.PlayingRandomizer)
                    return;

                GameObject tablet = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Glow Response Mage Computer"]);
                tablet.name = "Elderbug_Tablet";
                tablet.transform.localPosition = new(105.74f, 14.21f, 0.5f);
                tablet.SetActive(true);

                GameObject inspectRegion = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Inspect Region"]);
                inspectRegion.name = name;
                inspectRegion.transform.localPosition = new(105.74f, 12.11f);
                inspectRegion.SetActive(true);
                inspectRegion.LocateMyFSM("inspect_region").FsmVariables.FindFsmString("Game Text Convo").Value = "Elderbug_Preview";
            });
    }

    protected override void OnUnload()
    {
        Events.RemoveFsmEdit("Town", new("Elderbug", "Conversation Control"), ModifyElderbug);
        if (name == LocationList.Elderbug_Reward_Prefix + "1")
            Events.RemoveSceneChangeEdit("Town", a =>
            {
                if (!RandomizerManager.PlayingRandomizer)
                    return;
                if (name == LocationList.Lore_Tablet_Record_Bela)
                {
                    GameObject tablet = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Glow Response Mage Computer"]);
                    tablet.name = "Mage_Computer_2";
                    tablet.transform.localPosition = new(105.74f, 11.41f);
                    tablet.SetActive(true);
                }

                GameObject inspectRegion = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Inspect Region"]);
                inspectRegion.name = name;
                inspectRegion.transform.localPosition = new(105.74f, 11.41f);
                inspectRegion.SetActive(true);
                inspectRegion.LocateMyFSM("inspect_region").FsmVariables.FindFsmString("Elderbug_Preview");
            });
    }

    private void ModifyElderbug(PlayMakerFSM fsm)
    {
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = Placement.Name,
            Actions = new FsmStateAction[]
            {
                new Lambda(() =>
                {
                    string conversationName = Placement.Name;
                    if (conversationName.EndsWith("1"))
                        conversationName = "Elderbug_Done_Task_1";
                    else if (conversationName.EndsWith("2"))
                        conversationName = "Elderbug_Gain_Listening";
                    else
                        conversationName = $"Elderbug_Task_{Convert.ToInt32(conversationName.Substring(16)) + 1}";
                    fsm.GetState("Sly Rescued").GetFirstActionOfType<CallMethodProper>().gameObject.GameObject.Value
                    .GetComponent<DialogueBox>()
                    .StartConversation(conversationName, "Elderbug");
                })
            }
        });

        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = $"{Placement.Name} Throw",
            Actions = new FsmStateAction[]
            {
                new Lambda(() =>
                {
                    Container container = Container.GetContainer(Container.Shiny);
                    GameObject treasure = container.GetNewContainer(new ContainerInfo(container.Name, Placement, FlingType.StraightUp));
                    ShinyUtility.FlingShinyRight(treasure.LocateMyFSM("Shiny Control"));
                    container.ApplyTargetContext(treasure, fsm.gameObject, 0f);
                    ItemThrown = true;
                    fsm.gameObject.GetComponent<BoxCollider2D>().size = new(1.8361f, 0.2408f);
                })
            }
        });

        fsm.GetState("Convo Choice").AddTransition(Placement.Name, Placement.Name);
        fsm.GetState(Placement.Name).AddTransition("CONVO_FINISH", $"{Placement.Name} Throw");
        fsm.GetState($"{Placement.Name} Throw").AddTransition("FINISHED", "Talk Finish");
    }

    #endregion
}
