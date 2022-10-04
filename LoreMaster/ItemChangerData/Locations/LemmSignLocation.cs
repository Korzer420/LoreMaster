using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Util;
using LoreMaster.Randomizer;
using System.Linq;

namespace LoreMaster.ItemChangerData.Locations
{
    internal class LemmSignLocation : AutoLocation
    {
        protected override void OnLoad()
        {
            Events.AddFsmEdit(sceneName, new FsmID("Antique Dealer Door", "Door Control"), ModifySignControl);
            Events.AddFsmEdit(sceneName, new FsmID("Antique Dealer Door", "Conversation Control"), ModifySignText);
        }

        protected override void OnUnload()
        {
            Events.RemoveFsmEdit(sceneName, new FsmID("Antique Dealer Door", "Door Control"), ModifySignControl);
            Events.RemoveFsmEdit(sceneName, new FsmID("Antique Dealer Door", "Conversation Control"), ModifySignText);
        }

        private void ModifySignText(PlayMakerFSM fsm)
        {
            fsm.GetState("Idle").ClearTransitions();
            if (RandomizerManager.PlayingRandomizer && Placement.Items.All(x => x.IsObtained()))
                return;
            FsmState controlState = new(fsm.Fsm)
            {
                Name = "Control",
                Actions = new FsmStateAction[]
                {
                    new Lambda(() =>
                    {
                        if(Placement.Items.All(x => x.IsObtained()))
                            fsm.SendEvent("FINISHED");
                        else
                            fsm.SendEvent("ITEMS");
                    })
                }
            };
            fsm.GetState("Idle").AddTransition("CONVO START", controlState);
            controlState.AddTransition("FINISHED", "Box Up");
            controlState.AddTransition("ITEMS", "Talk Finish");
            fsm.GetState("Anim End").AddLastAction(new AsyncLambda(callback => ItemUtility.GiveSequentially(Placement.Items, Placement, new GiveInfo
            {
                FlingType = flingType,
                Container = Container.Tablet,
                MessageType = MessageType.Lore,
            }, callback), "CONVO FINISHED"));
            fsm.GetState("Anim End").ClearTransitions();
            fsm.GetState("Anim End").AddTransition("CONVO FINISHED", "Box Down");
        }

        /// <summary>
        /// Force the sign to stay until all items are obtained (once lemm left the shop)
        /// </summary>
        /// <param name="fsm"></param>
        private void ModifySignControl(PlayMakerFSM fsm)
        {
            fsm.AddState(new FsmState(fsm.Fsm)
            {
                Name = "Force Sign",
                Actions = new FsmStateAction[]
                {
                    new Lambda(() =>
                    {
                        if (!PlayerData.instance.GetBool("marmOutside") || PlayerData.instance.GetBool("marmOutsideConvo") && Placement.Items.All(x => x.IsObtained()))
                            fsm.SendEvent("DESTROY");
                    })
                }
            });

            fsm.GetState("Check").ClearTransitions();
            fsm.GetState("Check").AddTransition("DESTROY", "Force Sign");
            fsm.GetState("Force Sign").AddTransition("DESTROY", "Destroy");
        }
    }
}
