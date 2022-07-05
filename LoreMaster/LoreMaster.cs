using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LoreMaster.LorePowers;
using LoreMaster.LorePowers.Crossroads;
using LoreMaster.LorePowers.FungalWastes;
using LoreMaster.LorePowers.Ancient_Basin;
using LoreMaster.LorePowers.CityOfTears;
using ItemChanger;
using ItemChanger.Placements;
using ItemChanger.Locations;
using LoreMaster.LorePowers.FogCanyon;
using LoreMaster.LorePowers.Greenpath;
using LoreMaster.LorePowers.HowlingCliffs;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.LorePowers.QueensGarden;
using LoreMaster.LorePowers.RestingGrounds;
using LoreMaster.LorePowers.Waterways;
using LoreMaster.LorePowers.KingdomsEdge;

namespace LoreMaster
{
    public class LoreMaster : Mod, ITogglableMod
    {
        #region Members

        //private int _damageStacks = 0;
        //private bool _currentlyRunning;
        //private bool _hasHitEnemy;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the running instance of the mod.
        /// </summary>
        public static LoreMaster Instance { get; set; }

        public List<Power> ActivePowers { get; set; } = new();

        public Dictionary<string, GameObject> PreloadedObjects { get; set; } = new Dictionary<string, GameObject>();

        public System.Random Generator { get; set; } = new System.Random();

        #endregion

        #region Configuration

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private void UIManager_StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            ItemChangerMod.CreateSettingsProfile();
            List<MutablePlacement> teleportItems = new();
            MutablePlacement teleportPlacement = new CoordinateLocation() { x = 35.0f, y = 5.4f, elevation = 0, sceneName = "Ruins1_27", name = "City_Teleporter" }.Wrap() as MutablePlacement;
            teleportPlacement.Cost = new Paypal{ ToTemple = true };
            teleportPlacement.Add(new TouristMagnetItem(true, "City_Teleporter"));
            teleportItems.Add(teleportPlacement);

            MutablePlacement secondPlacement = new CoordinateLocation() { x = 57f, y = 5f, elevation = 0, sceneName = "Room_temple", name = "Temple_Teleporter" }.Wrap() as MutablePlacement;
            secondPlacement.Cost = new Paypal{ ToTemple = false };
            secondPlacement.Add(new TouristMagnetItem(false, "Temple_Teleporter"));
            teleportItems.Add(secondPlacement);
            ItemChangerMod.AddPlacements(teleportItems);
            orig(self, permaDeath, bossRush);
        }

        public override List<(string, string)> GetPreloadNames() => new List<(string, string)>()
        {
            ("RestingGrounds_08", "Ghost Battle Revek"),
            ("sharedassets156", "Lil Jellyfish")
        };

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            if (Instance != null)
                return;

            Instance = this;
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
            On.UIManager.StartNewGame += UIManager_StartNewGame;
            
            foreach (string key in preloadedObjects.Keys)
                foreach (string subKey in preloadedObjects[key].Keys)
                    if (!PreloadedObjects.ContainsKey(subKey))
                    { 
                        PreloadedObjects.Add(subKey, preloadedObjects[key][subKey]);
                        GameObject.DontDestroyOnLoad(preloadedObjects[key][subKey]);
                    }
        }

        private string ModHooks_LanguageGetHook(string key, string sheetTitle, string text)
        {
            if (key.Contains("TUT_TAB_02"))
            {
                Power power = new WisdomOfTheSagePower();
                power.Enable();
                ActivePowers.Add(power);
            }
            return text;
        }

        public void Unload()
        {
            Instance = null;
            ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
            On.UIManager.StartNewGame -= UIManager_StartNewGame;
        }

        #endregion
    }
}
