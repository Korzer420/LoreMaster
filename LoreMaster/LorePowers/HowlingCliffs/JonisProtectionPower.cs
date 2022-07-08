using LoreMaster.Enums;
using Modding;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.HowlingCliffs;

public class JonisProtectionPower : Power
{
    #region Members

    private Coroutine _currentlyRunning;

    private int _currentLifebloodBonus = 0;

    #endregion

    #region Constructors

    public JonisProtectionPower() : base("Joni's Protection", Area.Cliffs)
    {
        CustomText = "Did you just took my blessing? How rude of you. First you banish me here and now that? Not cool, dude. Well, now that you have already took it, my prayers are with you....................... please don't dream nail me, dude.";
        Hint = "When going to a new area, you will receive the gift of joni, which will quickly fade away.";
        Description = "When going to another area, you will be granted 5 life blood (10 if you have Joni's equipped). Each 3 seconds a lifeblood will fade away.";
    }

    #endregion

    #region Event handler

    /// <summary>
    /// Event handler when the charms are updated.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="controller"></param>
    private void CharmUpdate(PlayerData data, HeroController controller)
    {
        if (_currentlyRunning != null)
        {
            LoreMaster.Instance.Handler.StopCoroutine(_currentlyRunning);
            _currentLifebloodBonus = 0;
        }
    }

    /// <summary>
    /// Event handler which takes away the health.
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    private int TakeHealth(int damage)
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

    #region Protected Methods

    protected override void Enable()
    {
        LoreMaster.Instance.SceneActions.Add(PowerName, () =>
        {
            LoreMaster.Instance.Handler.StopCoroutine(_currentlyRunning);
            _currentlyRunning = LoreMaster.Instance.Handler.StartCoroutine(FadingLifeblood());
            
        });
        ModHooks.CharmUpdateHook += CharmUpdate;
        ModHooks.TakeHealthHook += TakeHealth;
    }

    protected override void Disable()
    {
        LoreMaster.Instance.SceneActions.Remove(PowerName);
        ModHooks.CharmUpdateHook -= CharmUpdate;
        ModHooks.TakeHealthHook -= TakeHealth;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Quickly fades away lifeblood.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadingLifeblood()
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
