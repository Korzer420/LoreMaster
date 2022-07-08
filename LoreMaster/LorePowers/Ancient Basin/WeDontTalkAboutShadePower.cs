using LoreMaster.Enums;
using Modding;
using UnityEngine;

namespace LoreMaster.LorePowers.Ancient_Basin;

public class WeDontTalkAboutShadePower : Power
{
    #region Constructors

    public WeDontTalkAboutShadePower() : base("We don't talk about the Shade", Area.AncientBasin)
    {
        Hint = "Shade? What should that be? Can you prove, that the \"Shade\" exist? Your soul vessel? What are you talking about?";
        Description = "You don't get the soul limit punishment, when dying. Your geo will still be on the shade.";
    }

    #endregion

    #region Event Handler

    /// <summary>
    /// Removes the soul limited punishment from the player.
    /// </summary>
    private void AfterPlayerDied() => PlayerData.instance.SetBool("soulLimited", false);

    #endregion

    #region Public Methods

    protected override void Enable()
    {
        ModHooks.AfterPlayerDeadHook += AfterPlayerDied;
        // The game uses the soulLimited value to determine, if the shade is active, because we negate that, we manually need to spawn the shade.
        LoreMaster.Instance.SceneActions.Add(PowerName, () =>
        {
            if (string.Equals(PlayerData.instance.shadeScene, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
                GameObject.Instantiate(GameManager.instance.sm.hollowShadeObject, new Vector3(PlayerData.instance.shadePositionX, PlayerData.instance.shadePositionY), Quaternion.identity);
        });
        PlayerData.instance.SetBool("soulLimited", false);
    }

    protected override void Disable()
    {
        ModHooks.AfterPlayerDeadHook -= AfterPlayerDied;
        LoreMaster.Instance.SceneActions.Remove(PowerName);
        if (!string.IsNullOrEmpty(PlayerData.instance.shadeScene))
            PlayerData.instance.StartSoulLimiter();
    }

    #endregion
}
