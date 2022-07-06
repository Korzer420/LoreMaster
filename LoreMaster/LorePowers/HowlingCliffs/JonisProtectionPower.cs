using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.HowlingCliffs;

public class JonisProtectionPower : Power
{
    #region Members

    private Coroutine _currentlyRunning;

    private int _currentLifebloodBonus = 0;

    #endregion

    #region Constructors

    public JonisProtectionPower() : base("", Area.Cliffs)
    {

    }

    #endregion

    #region Event handler

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (!GameManager._instance.IsGameplayScene())
            return;
        
        if (_currentlyRunning == null)
            _currentlyRunning = HeroController.instance.StartCoroutine(RemoveLifeblood());
        else
        {
            HeroController.instance.StopCoroutine(_currentlyRunning);
            _currentlyRunning = HeroController.instance.StartCoroutine(RemoveLifeblood());
        }
    }

    private void ModHooks_CharmUpdateHook(PlayerData data, HeroController controller)
    {
        if (_currentlyRunning != null)
        {
            HeroController.instance.StopCoroutine(_currentlyRunning);
            _currentLifebloodBonus = 0;
        }
    }

    private int ModHooks_TakeHealthHook(int damage)
    {
        if (_currentLifebloodBonus > 0)
        {
            if (_currentLifebloodBonus - damage < 0)
                _currentLifebloodBonus = 0;
            else
                _currentLifebloodBonus -= damage;
        }
        return damage;
    }

    #endregion

    #region Public Methods

    protected override void Enable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        ModHooks.CharmUpdateHook += ModHooks_CharmUpdateHook;
        ModHooks.TakeHealthHook += ModHooks_TakeHealthHook;
    }

    protected override void Disable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        ModHooks.CharmUpdateHook -= ModHooks_CharmUpdateHook;
        ModHooks.TakeHealthHook -= ModHooks_TakeHealthHook;
    }

    #endregion

    #region Private Methods

    private IEnumerator RemoveLifeblood()
    {
        int currentBonus = _currentLifebloodBonus;
        for (int i = 0; i < (PlayerData.instance.GetBool("equippedCharm_27") ? 10 : 5) - currentBonus; i++)
        {
            EventRegister.SendEvent("ADD BLUE HEALTH");
            _currentLifebloodBonus++;
        }

        while (_currentLifebloodBonus > 0)
        {
            yield return new WaitForSeconds(3f);
            // This is an extra check for the case, that the last lifeblood gets taken to prevent removing real masks.
            if (_currentLifebloodBonus > 0)
                HeroController.instance.TakeHealth(1);
        }
    }

    #endregion
}
