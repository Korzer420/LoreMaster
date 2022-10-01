using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Helper;

namespace LoreMaster.LorePowers.CityOfTears;

public class TouristPower : Power
{
    #region Constructors

    public TouristPower() : base("Tourist", Area.CityOfTears) { }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the flag that indicates, if the fast travel can be taken.
    /// <para/> This is used to increase performance (Could also be done by searching through the active powers).
    /// </summary>
    public static bool Inspected { get; set; }

    #endregion

    #region Event handler

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (string.Equals(self.FsmName, "Door Control") && string.Equals(self.gameObject.name, "door1")
            && string.Equals(self.transform.parent?.name, "Final Boss Door"))
        {
            PlayMakerFSM nailSmithCost = LoreMaster.Instance.PreloadedObjects["Nailsmith"].LocateMyFSM("Conversation Control");
            self.AddState(new FsmState(self.Fsm)
            {
                Name = "Gate",
                Actions = new FsmStateAction[]
                {
                    new Lambda(() =>
                    {
                        if(State != PowerState.Twisted)
                            self.SendEvent("YES");
                        HeroController.instance.RelinquishControl();
                    }),
                    nailSmithCost.GetState("Box Up YN").Actions[0],
                    nailSmithCost.GetState("Box Up YN").Actions[1],
                    new Lambda(() =>
                    {
                        nailSmithCost.FsmVariables.FindFsmInt("Upgrade Cost").Value = 3000;
                    }),
                    nailSmithCost.GetState("Send Text").Actions[0],
                    nailSmithCost.GetState("Send Text").Actions[1],
                    nailSmithCost.GetState("Send Text").Actions[2],
                }
            });
            self.GetState("Gate").GetFirstActionOfType<SetFsmGameObject>().setValue.Value = self.gameObject;
            self.GetState("Gate").GetFirstActionOfType<CallMethodProper>().parameters[0].SetValue("Temple_Door");

            self.GetState("Enter Anim?").AdjustTransition("FINISHED", "Gate");
            self.GetState("Gate").AddTransition("NO", "In Range");
            self.GetState("Gate").AddTransition("YES", "Enter");
        }
        orig(self);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable() => Inspected = true;

    /// <inheritdoc/>
    protected override void Disable() => Inspected = false;

    /// <inheritdoc/>
    protected override void TwistEnable() 
    { 
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        On.HutongGames.PlayMaker.Actions.SendEventByName.OnEnter += SendEventByName_OnEnter;
    }

    private void SendEventByName_OnEnter(On.HutongGames.PlayMaker.Actions.SendEventByName.orig_OnEnter orig, SendEventByName self)
    {
        orig(self);
        if(self.IsCorrectContext("Door Control", "door1", null) && string.Equals(self.Fsm.FsmComponent.transform.parent?.name, "Final Boss Door")
            && (string.Equals(self.State.Name, "In Range") || string.Equals(self.State.Name, "Enter")))
        {
            if (self.State.Name == "In Range")
                HeroController.instance.RegainControl();
            PlayMakerFSM.BroadcastEvent("BOX DOWN YN");
        }
    }

    /// <inheritdoc/>
    protected override void TwistDisable() => On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;

    #endregion
}
