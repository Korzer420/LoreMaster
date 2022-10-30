using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Util;
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
    }

    protected override void OnUnload()
    {
        Events.RemoveFsmEdit("Town", new("Elderbug", "Conversation Control"), ModifyElderbug);
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
                    container.ApplyTargetContext(treasure, fsm.gameObject, -2f);
                    ItemThrown = true;
                })
            }
        });

        fsm.GetState("Convo Choice").AddTransition(Placement.Name, Placement.Name);
        fsm.GetState(Placement.Name).AddTransition("CONVO_FINISH", $"{Placement.Name} Throw");
        fsm.GetState($"{Placement.Name} Throw").AddTransition("FINISHED", "Talk Finish");
    }

    #endregion
}
