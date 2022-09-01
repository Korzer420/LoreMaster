using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using Modding;
using System;
using UnityEngine;

namespace LoreMaster.LorePowers.Ancient_Basin;

public class WeDontTalkAboutShadePower : Power
{
    #region Constructors

    public WeDontTalkAboutShadePower() : base("We don't talk about the Shade", Area.AncientBasin) { }

    #endregion

    #region Properties

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override Action SceneAction => () =>
    {
        // The game uses the soulLimited value to determine, if the shade is active, because we negate that, we manually need to spawn the shade.
        if (GameObject.Find("Hollow Shade(Clone)") == null && string.Equals(PlayerData.instance.GetString(nameof(PlayerData.instance.shadeScene)), UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
            GameObject.Instantiate(GameManager.instance.sm.hollowShadeObject, new Vector3(PlayerData.instance.GetFloat(nameof(PlayerData.instance.shadePositionX)), PlayerData.instance.GetFloat(nameof(PlayerData.instance.shadePositionY))), Quaternion.identity);
    };

    #endregion

    #region Event Handler

    /// <summary>
    /// Removes the soul limited punishment from the player.
    /// </summary>
    private void AfterPlayerDied() => PlayerData.instance.SetBool("soulLimited", false);

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    private void OnSetPlayerDataIntAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.orig_OnEnter orig, SetPlayerDataInt self)
    {
        if (string.Equals(self.Fsm.FsmComponent.gameObject.name, "Hero Death") && string.Equals(self.Fsm.FsmComponent.FsmName, "Hero Death Anim") && string.Equals(self.Fsm.FsmComponent.ActiveStateName, "Remove Geo") && string.Equals(self.intName.Value, "geoPool") && Active)
        {
            self.value.Value += PlayerData.instance.GetInt(nameof(PlayerData.instance.geoPool)) > 1 ? PlayerData.instance.GetInt(nameof(PlayerData.instance.geoPool)) / 2 : 0;
        }
        orig(self);
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += OnPlayerDataBoolTestAction;
        On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.OnEnter += OnSetPlayerDataIntAction;
        ModHooks.AfterPlayerDeadHook += AfterPlayerDied;
        PlayerData.instance.SetBool("soulLimited", false);
    }

    private void OnPlayerDataBoolTestAction(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, PlayerDataBoolTest self)
    {
        if (string.Equals(self.Fsm.FsmComponent.FsmName, "Deactivate if !SoulLimited") && string.Equals(PlayerData.instance.GetString(nameof(PlayerData.instance.shadeScene)), "None") && Active)
        {
            self.Fsm.FsmComponent.SendEvent("DEACTIVATE");
        }
        orig(self);
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter -= OnPlayerDataBoolTestAction;
        On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.OnEnter -= OnSetPlayerDataIntAction;
        ModHooks.AfterPlayerDeadHook -= AfterPlayerDied;
        if (!PlayerData.instance.GetString(nameof(PlayerData.instance.shadeScene)).Equals("None"))
            PlayerData.instance.StartSoulLimiter();
    }

    #endregion
}
