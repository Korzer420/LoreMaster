using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.FogCanyon
{
    public class FriendOfTheJellyPower : Power
    {
        #region Constructors

        public FriendOfTheJellyPower() : base("", Area.FogCanyon)
        {

        }

        #endregion

        #region Methods

        public override void Enable()
        {
            On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
            LoreMaster.Instance.PreloadedObjects["Lil Jellyfish"].GetComponent<DamageHero>().damageDealt = 0;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            HealthManager[] enemies = GameObject.FindObjectsOfType<HealthManager>();
            if (enemies.Length == 0)
                return;
            foreach (HealthManager item in enemies)
            {
                // Uumuu is called Mega Jellyfish in the files, that's why we check for jelly fish without mega in their name
                if (item.gameObject.name.Contains("Jellyfish") && !item.gameObject.name.Contains("Mega"))
                {
                    HeroController.Destroy(item.gameObject.GetComponent<DamageHero>());
                    // The tentacles of the jelly have their own component.
                    if(!item.gameObject.name.Contains("Baby"))
                        HeroController.Destroy(item.transform.Find("Tentacle Box").GetComponent<DamageHero>());
                }
            }
        }

        private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            if (self.FsmName.Equals("Explosion Control"))
            {
                if (IsCurrentlyActive())
                { 
                    // This is for godhome oomas
                    if(self.gameObject.name.Contains("Gas Explosion Uumuu"))
                        HeroController.Destroy(self.transform.Find("Hero Damage").GetComponent<DamageHero>()); 
                    else
                        HeroController.Destroy(self.GetComponent<DamageHero>());
                }
                else if (self.GetComponent<DamageHero>() == null)
                {
                    DamageHero damageHero = self.gameObject.AddComponent<DamageHero>();
                    damageHero.damageDealt = 2;
                    damageHero.hazardType = 1;
                }
            }
            else if (self.FsmName.Equals("Jellyfish") && self.gameObject.name.Contains("Jellyfish GG"))
            {
                HeroController.Destroy(self.GetComponent<DamageHero>());
                HeroController.Destroy(self.transform.Find("Tentacle Box").GetComponent<DamageHero>());
            }

            orig(self);
        }

        public override void Disable()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
            LoreMaster.Instance.PreloadedObjects["Lil Jellyfish"].GetComponent<DamageHero>().damageDealt = 2;
        }

        #endregion
    }
}
