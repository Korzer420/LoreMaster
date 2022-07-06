using HutongGames.PlayMaker.Actions;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vasi;

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

    #region Methods

    protected override void Enable()
    {
        ModHooks.AfterPlayerDeadHook += ModHooks_AfterPlayerDeadHook;
        LoreMaster.Instance.SceneActions.Add(PowerName, () =>
        {
            if (string.Equals(PlayerData.instance.shadeScene, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
                GameObject.Instantiate(GameManager.instance.sm.hollowShadeObject, new Vector3(PlayerData.instance.shadePositionX, PlayerData.instance.shadePositionY), Quaternion.identity);
        });
    }

    private void ModHooks_AfterPlayerDeadHook() => PlayerData.instance.SetBool("soulLimited", false);
    
    protected override void Disable()
    {
        ModHooks.AfterPlayerDeadHook -= ModHooks_AfterPlayerDeadHook;
        LoreMaster.Instance.SceneActions.Remove(PowerName);
        if (!string.IsNullOrEmpty(PlayerData.instance.shadeScene))
            PlayerData.instance.StartSoulLimiter();
    }

    #endregion
}
