using LoreMaster.Enums;
using Modding;
using System;
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
    protected override void Enable()
    {
        ModHooks.AfterPlayerDeadHook += AfterPlayerDied;
        PlayerData.instance.SetBool("soulLimited", false);
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ModHooks.AfterPlayerDeadHook -= AfterPlayerDied;
        if (!PlayerData.instance.GetString(nameof(PlayerData.instance.shadeScene)).Equals("None"))
            PlayerData.instance.StartSoulLimiter();
    }

    #endregion
}
