using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using LoreMaster.Enums;
using Modding;
using System;
using UnityEngine;

namespace LoreMaster.LorePowers.Ancient_Basin;

public class WeDontTalkAboutShadePower : Power
{
    #region Constructors

    public WeDontTalkAboutShadePower() : base("We don't talk about Shade", Area.AncientBasin) { }

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

    #region Normal handler

    /// <summary>
    /// Removes the soul limited punishment from the player.
    /// </summary>
    private void AfterPlayerDied() => PlayerData.instance.SetBool("soulLimited", false);

    private void OnSetPlayerDataIntAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.orig_OnEnter orig, SetPlayerDataInt self)
    {
        if (string.Equals(self.Fsm.GameObjectName, "Hero Death") && string.Equals(self.Fsm.Name, "Hero Death Anim")
            && string.Equals(self.State.Name, "Remove Geo") && string.Equals(self.intName.Value, "geoPool"))
            self.value.Value += PlayerData.instance.GetInt(nameof(PlayerData.instance.geoPool)) > 1 ? PlayerData.instance.GetInt(nameof(PlayerData.instance.geoPool)) / 2 : 0;
        orig(self);
    }

    private void OnPlayerDataBoolTestAction(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, PlayerDataBoolTest self)
    {
        if (string.Equals(self.Fsm.Name, "Deactivate if !SoulLimited"))
            self.isFalse = string.Equals(PlayerData.instance.GetString(nameof(PlayerData.instance.shadeScene)), "None") ? FsmEvent.GetFsmEvent("DEACTIVATE") : null;
        orig(self);
    }

    #endregion

    #region Twisted handler

    private void SpawnShade(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        if (!string.Equals(self.gameObject.name, "Hollow Knight Boss"))
        {
            GameObject shade = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Shade Sibling (25)"]);
            shade.transform.position = self.transform.position;
            shade.SetActive(true);
        }
    }
    private void ForceShadeEnemy(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
    {
        orig(self);
        if (string.Equals(self.State.Name, "Friendly?") && self.Fsm.GameObjectName.Contains("Shade Sibling")
            && string.Equals(self.Fsm.Name, "Control") && self.integer1.Value == self.integer2.Value)
            self.Fsm.FsmComponent.SendEvent("FINISHED");
    }


    #endregion

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += OnPlayerDataBoolTestAction;
        On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.OnEnter += OnSetPlayerDataIntAction;
        ModHooks.AfterPlayerDeadHook += AfterPlayerDied;
        PlayerData.instance.SetBool("soulLimited", false);
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter -= OnPlayerDataBoolTestAction;
        On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.OnEnter -= OnSetPlayerDataIntAction;
        ModHooks.AfterPlayerDeadHook -= AfterPlayerDied;
        // Reapply the soul limiter if it should be active right now (when the shade is active)
        if (!PlayerData.instance.GetString(nameof(PlayerData.instance.shadeScene)).Equals("None"))
            PlayerData.instance.StartSoulLimiter();
    }

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        On.HealthManager.Die += SpawnShade;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += ForceShadeEnemy;
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        On.HealthManager.Die -= SpawnShade;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter -= ForceShadeEnemy;
    }

    #endregion
}
