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

    public WeDontTalkAboutShadePower() : base("We don't talk about the Shade", Area.AncientBasin)
    {
        Hint = "Shade? What should that be? Can you prove, that the \"Shade\" exist? Your soul vessel and geo? What are you talking about?";
        Description = "You don't get the soul limit punishment, when dying. Your geo will still be on the shade. When dying while the shade is active, your shade only loose 50% of your geo.";
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
    protected override void Initialize()
    {
        HeroController.instance.transform.Find("Hero Death").gameObject.LocateMyFSM("Hero Death Anim").GetState("Remove Geo").ReplaceAction(new Lambda(() =>
        {
            if (Active)
            { 
                int shadeGeo = PlayerData.instance.GetInt(nameof(PlayerData.instance.geoPool));
                PlayerData.instance.SetInt(nameof(PlayerData.instance.geoPool), PlayerData.instance.GetInt(nameof(PlayerData.instance.geo)) + (shadeGeo > 1 
                    ? shadeGeo / 2
                    : 0));
            }
            else
                PlayerData.instance.SetInt(nameof(PlayerData.instance.geoPool), PlayerData.instance.GetInt(nameof(PlayerData.instance.geo)));
        })
        {
            Name = "Diminish geo punishment"
        }, 1);
    }

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
