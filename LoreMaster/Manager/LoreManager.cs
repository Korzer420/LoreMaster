using LoreMaster.ItemChangerData;
using LoreMaster.LorePowers.QueensGarden;
using LoreMaster.LorePowers;

namespace LoreMaster.Manager;

/// <summary>
/// Manager for handling the lore related logic.
/// </summary>
internal static class LoreManager
{
    #region Properties

    public static LorePowerModule Module { get; set; }

    public static bool UseCustomText { get; set; }

    public static bool UseHints { get; set; }

    #endregion

    #region Eventhandler

    private static void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (string.Equals(self.gameObject.name, "Elderbug") && string.Equals(self.FsmName, "npc_control"))
        {
            self.transform.localScale = new(2f, 2f, 2f);
            self.transform.localPosition = new(126.36f, 12.35f, 0f);
        }
        else if (string.Equals(self.FsmName, "Thorn Counter"))
            PowerManager.GetPower<QueenThornsPower>().ModifyThorns(self);
        orig(self);
    }

    /// <summary>
    /// Event handler to disable all powers after a final boss has been killed.
    /// </summary>
    private static void EndAllPowers(On.HutongGames.PlayMaker.Actions.SendEventByName.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SendEventByName self)
    {
        orig(self);
        if (string.Equals(self.sendEvent.Value, "ALL CHARMS END") && (string.Equals(self.Fsm.GameObjectName, "Hollow Knight Boss")
            || string.Equals(self.Fsm.GameObjectName, "Radiance") || string.Equals(self.Fsm.GameObjectName, "Absolute Radiance")))
            PowerManager.DisableAllPowers();
    }

    #endregion

    #region Methods

    public static void Initialize()
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        On.HutongGames.PlayMaker.Actions.SendEventByName.OnEnter += EndAllPowers;
    }

    #endregion
}
