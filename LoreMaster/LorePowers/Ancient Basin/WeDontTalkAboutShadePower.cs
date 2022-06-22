using HutongGames.PlayMaker.Actions;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vasi;

namespace LoreMaster.LorePowers.Ancient_Basin
{
    public class WeDontTalkAboutShadePower : Power
    {
        #region Constructors

        public WeDontTalkAboutShadePower() : base("", Area.AncientBasin)
        {
            Description = "[We don't talk about the Shade]<br>Shade? What should that be? Can you prove, that the \"Shade\" exist? Your soul vessel? What are you talking about?";
        }
        #endregion

        #region Methods

        public override void Enable()
        {
            ModHooks.AfterPlayerDeadHook += ModHooks_AfterPlayerDeadHook;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene oldScene, UnityEngine.SceneManagement.Scene newScene)
        {
            if (string.Equals(PlayerData.instance.shadeScene, newScene.name))
                GameObject.Instantiate(GameManager.instance.sm.hollowShadeObject, new Vector3(PlayerData.instance.shadePositionX, PlayerData.instance.shadePositionY), Quaternion.identity);
        }

        private void ModHooks_AfterPlayerDeadHook()
        {
            PlayerData.instance.SetBool("soulLimited", false);
        }

        public override void Disable()
        {
            ModHooks.AfterPlayerDeadHook -= ModHooks_AfterPlayerDeadHook;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        }

        #endregion
    }
}
