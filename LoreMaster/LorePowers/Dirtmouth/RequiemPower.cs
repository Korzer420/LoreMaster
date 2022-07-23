using LoreMaster.Enums;
using UnityEngine;

namespace LoreMaster.LorePowers.Dirtmouth;

public class RequiemPower : Power
{
    #region Members

    private string _warpScene;
    private int _counter = 3;

    #endregion

    #region Constructors

    public RequiemPower() : base("Requiem", Area.Dirtmouth)
    {
        Hint = "When your shell breaks, holding a pure focus may arise you at the home of death. Holding the light's artifact will instead catch you in the protecting wings of the last one.";
        Description = "Holding focus, while you dying will spawn you in dirtmouth instead of your bench. Holding dream nail will spawn you at spirit's glade instead";
    }

    #endregion

    #region Event Handler

    private void CheckForDeathInput(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
    {
        if (GameManager.instance.RespawningHero)
        {
            if (InputHandler.Instance.inputActions.cast.IsPressed)
            {
                //GameManager.instance.RespawningHero = false;
                PlayerData.instance.SetInt(nameof(PlayerData.instance.respawnType), 2);
                info.SceneName = "Town";
                info.EntryGateName = "left1";
                _warpScene = "Town";
            }
            else if (InputHandler.Instance.inputActions.dreamNail.IsPressed)
            {
                //GameManager.instance.RespawningHero = false;
                PlayerData.instance.SetInt(nameof(PlayerData.instance.respawnType), 2);
                info.SceneName = "RestingGrounds_08";
                info.EntryGateName = "left1";
                _warpScene = "Grounds";
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
        GameObject gameObject = new("Something");
        GameObject.DontDestroyOnLoad(gameObject);
        gameObject.transform.localPosition = _warpScene.Equals("Town") ? new(230f, 8.41f) : new(33.1f, 5.41f);
        gameObject.AddComponent<RespawnMarker>().respawnFacingRight = false;
        _counter--;
        if (_counter <= 0)
        {
            _warpScene = null;
            _counter = 3;
        }
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
}
