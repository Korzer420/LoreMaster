using LoreMaster.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoreMaster.LorePowers.Dirtmouth;

public class RequiemPower : Power
{
    #region Members

    private string _warpScene;
    private List<GameObject> _mockSpawns = new();

    #endregion

    #region Constructors

    public RequiemPower() : base("Requiem", Area.Dirtmouth)
    {
        Hint = "When your shell breaks, holding a pure focus may arise you at the home of death. Holding the light's artifact will instead catch you in the protecting wings of the last one, if she opened her secret. " +
            "Holding the shiny stone vessel, will drop your soul onto the now abandoned place, where the light started its revenge.";
        Description = "Holding focus, while you dying will spawn you in dirtmouth instead of your bench. Holding dream nail will spawn you at spirit's glade instead (when the glade is open). " +
            "Holding Crystal Dash will spawn you at Hallownests Crown (?), requires Crystal Heart to work.";
    }

    #endregion

    #region Event Handler

    private void CheckForDeathInput(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
    {
        if (GameManager.instance.RespawningHero)
        {
            if (InputHandler.Instance.inputActions.cast.IsPressed)
            {
                PlayerData.instance.SetInt(nameof(PlayerData.instance.respawnType), 2);
                info.SceneName = "Town";
                info.EntryGateName = "left1";
                _warpScene = "Town";
            }
            else if (InputHandler.Instance.inputActions.dreamNail.IsPressed && PlayerData.instance.GetBool(nameof(PlayerData.instance.gladeDoorOpened)))
            {
                PlayerData.instance.SetInt(nameof(PlayerData.instance.respawnType), 2);
                info.SceneName = "RestingGrounds_08";
                info.EntryGateName = "left1";
                _warpScene = "Grounds";
            }
            else if (InputHandler.Instance.inputActions.superDash.IsPressed && PlayerData.instance.GetBool(nameof(PlayerData.instance.hasSuperDash)))
            {
                PlayerData.instance.SetInt(nameof(PlayerData.instance.respawnType), 2);
                info.SceneName = "Mines_34";
                info.EntryGateName = "bottom1";
                _warpScene = "Peaks";
            }
        }
        orig(self, info);
    }

    /// <summary>
    /// Event Handler to mock a spawn point, which the hero controller can use to do the custom respawn.
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    /// <returns></returns>
    private Transform MockSpawnPoint(On.HeroController.orig_LocateSpawnPoint orig, HeroController self)
    {
        Transform spawnPoint = orig(self);
        if (string.IsNullOrEmpty(_warpScene))
            return spawnPoint;
        GameObject gameObject = new("SpawnPoint");
        GameObject.DontDestroyOnLoad(gameObject);
        gameObject.transform.localPosition = _warpScene.Equals("Town") ? new(230f, 8.41f) : (_warpScene.Equals("Grounds") ? new(33.1f, 5.41f) : new(74f, 51.4f, 0f));
        gameObject.AddComponent<RespawnMarker>().respawnFacingRight = false;
        _mockSpawns.Add(gameObject);
        if (_mockSpawns.Count >= 3)
            LoreMaster.Instance.Handler.StartCoroutine(WaitForControl());
        return gameObject.transform;
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    /// <inheritdoc/>
    protected override void Enable()
    {
        On.GameManager.BeginSceneTransition += CheckForDeathInput;
        On.HeroController.LocateSpawnPoint += MockSpawnPoint;
    }

    /// <inheritdoc/>
    /// <inheritdoc/>
    protected override void Disable()
    {
        On.GameManager.BeginSceneTransition -= CheckForDeathInput;
        On.HeroController.LocateSpawnPoint -= MockSpawnPoint;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Wait for the player to get control before removing the spawn points.
    /// </summary>
    private IEnumerator WaitForControl()
    {
        yield return new WaitUntil(() => HeroController.instance.acceptingInput);
        _warpScene = null;
        foreach (GameObject respawn in _mockSpawns)
            GameObject.Destroy(respawn);
        _mockSpawns.Clear();
    }

    #endregion
}
