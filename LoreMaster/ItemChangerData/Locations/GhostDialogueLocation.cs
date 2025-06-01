using ItemChanger;
using ItemChanger.FsmStateActions;
using KorzUtils.Helper;
using System;
using System.Linq;

namespace LoreMaster.ItemChangerData.Locations;

internal class GhostDialogueLocation : DialogueLocation
{
    public string GhostName { get; set; }

    protected override void OnLoad()
    {
        base.OnLoad();
        On.PlayMakerFSM.OnEnable += PreventGhostDeath;
        Events.AddFsmEdit(new FsmID(null, "ghost_npc_death"), (a) => { });
    }

    private void PreventGhostDeath(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (string.Equals(self.FsmName, "ghost_npc_death"))
        {
            try
            {
                string ghostName = self.gameObject?.LocateMyFSM("Conversation Control")?.FsmVariables?.FindFsmString("Ghost Name")?.Value?.ToUpper();
                if (ghostName == GhostName)
                {
                    self.AddState(new HutongGames.PlayMaker.FsmState(self.Fsm)
                    {
                        Name = "Check for items",
                        Actions = new HutongGames.PlayMaker.FsmStateAction[]
                        {
                            new Lambda(() =>
                            {
                                try
                                {
                                    if (!Placement.Items.Any() || Placement.Items.All(x => x.IsObtained()))
                                    { 
                                        self.SendEvent("KILL");
                                        return;
                                    }
                                        
                                }
                                catch (Exception exception)
                                {
                                    LoreMaster.Instance.LogError("An error occured while checking for items on ghost: " + exception.Message);
                                    LoreMaster.Instance.LogError(exception.StackTrace);
                                }
                                self.SendEvent("IMMUNE");
                            })
                        }
                    });
                    self.GetState("Idle").AdjustTransition("DREAMNAIL", "Check for items");
                    self.GetState("Check for items").AddTransition("KILL", "Revek?");
                    self.GetState("Check for items").AddTransition("IMMUNE", "Idle");
                }
            }
            catch (Exception exception)
            {
                LoreMaster.Instance.LogError("An error occured while modifiying the ghost_npc_death fsm: " + exception.Message);
                LoreMaster.Instance.LogError(exception.StackTrace);
            }
        }
        orig(self);
    }
}
