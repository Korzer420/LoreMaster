using ItemChanger;
using KorzUtils.Helper;
using LoreMaster.ItemChangerData;
using LoreMaster.LorePowers;
using LoreMaster.SaveManagement;
using Modding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoreMaster.Manager;

/// <summary>
/// Manager for handling basic operations of this mod.
/// </summary>
internal static class LoreManager
{
    #region Members

    private static bool _active;

    #endregion

    #region Properties

    public static LorePowerModule Module { get; set; }

    public static LoreMasterGlobalSaveData GlobalSaveData { get; set; } = new();

    public static LoreMasterLocalSaveData LocalSaveData { get; set; } = new();

    #endregion

    #region Eventhandler

    private static void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (GameManager.instance?.IsGameplayScene() == true)
            PowerManager.ExecuteSceneActions();
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

    private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name == "LoreArtifact")
            return _active;
        return orig;
    }

    #endregion

    #region Methods

    public static void Initialize()
    {
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        On.HutongGames.PlayMaker.Actions.SendEventByName.OnEnter += EndAllPowers;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        LoreMaster.Instance.Handler.StartCoroutine(WaitForPlayerControl());
        Module = ItemChangerMod.Modules.GetOrAdd<LorePowerModule>();
        _active = true;
    }

    public static void Unload()
    {
        if (!_active)
            return;
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        On.HutongGames.PlayMaker.Actions.SendEventByName.OnEnter -= EndAllPowers;
        IEnumerable<Power> activePowers = PowerManager.GetAllActivePowers();
        LoreMaster.Instance.Handler.StopAllCoroutines();
        foreach (Power power in activePowers)
            power.DisablePower(true);
        Module = null;
        _active = false;
    }

    private static IEnumerator WaitForPlayerControl()
    {
        yield return new WaitForFinishedEnteringScene();
        // Just to make sure the controller exist. A desperate attempt.
        if (HeroController.instance?.acceptingInput != true)
            yield return new WaitUntil(() => HeroController.instance?.acceptingInput == true);
        IEnumerable<Power> activePowers = PowerManager.GetAllActivePowers();
        foreach (Power power in activePowers)
            try
            {
                power.EnablePower();
            }
            catch (System.Exception exception)
            {
                string message = string.Format("Failed to active power: '{0}'. Error: '{1}'", power.PowerName, exception.ToString());
                LogHelper.Write<LoreMaster>(message, KorzUtils.Enums.LogType.Error);
            }
    }

    #endregion
}
