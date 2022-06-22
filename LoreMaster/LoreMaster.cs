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
using Vasi;

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

        public override void Initialize()
        {
            if (Instance != null)
                return;

            Instance = this;
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;

            On.UIManager.StartNewGame += UIManager_StartNewGame;

        }

        private void UIManager_StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            ItemChangerMod.CreateSettingsProfile();
            MutablePlacement teleportPlacement = new CoordinateLocation() { x = 35.0f, y = 5.4f, elevation = 0, sceneName = "Ruins1_27", name = "City_Teleporter" }.Wrap() as MutablePlacement;
            teleportPlacement.Cost = new Paypal() { ToTemple = true };
            teleportPlacement.Add(new TouristMagnetItem(true, "City_Teleporter"));
            ItemChangerMod.AddPlacements(new List<MutablePlacement>() { teleportPlacement });

            MutablePlacement secondPlacement = new CoordinateLocation() { x = 56.62f, y = 3.41f, elevation = 0, sceneName = "ROOM_TEMPLE", name = "Temple_Teleporter" }.Wrap() as MutablePlacement;
            secondPlacement.Cost = new Paypal() { ToTemple = false };
            secondPlacement.Add(new TouristMagnetItem(false, "Temple_Teleporter"));
            ItemChangerMod.AddPlacements(new List<MutablePlacement>() { secondPlacement });
            orig(self, permaDeath, bossRush);
        }

        //public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        //{
        //    foreach (string key in preloadedObjects.Keys)
        //        foreach (string subKey in preloadedObjects[key].Keys)
        //        {
        //            if (!PreloadedObjects.ContainsKey(subKey))
        //                PreloadedObjects.Add(subKey, preloadedObjects[key][subKey]);
        //        }
        //}

        //public override List<(string, string)> GetPreloadNames()
        //{
        //    return new List<(string, string)>() { ("resources", "Hollow Shade") };
        //}

        private string ModHooks_LanguageGetHook(string key, string sheetTitle, string text)
        {
            if (key.Equals("TUT_TAB_01"))
            {
                ActivePowers.Add(new FokusPower() { Acquired = true });
                ActivePowers.Last().Enable();
                text += ActivePowers[0].Description;
            }
            else if (key.Equals("TUT_TAB_02"))
            {
                ActivePowers.Add(new ScrewTheRulesPower() { Acquired = true });
                ActivePowers.Last().Enable();
                text += ActivePowers.Last().Description;
            }
            else if (key.Equals("TUT_TAB_03"))
            {
                Power power = new TrueFormPower();
                power.Enable();
                text += power.Description;
                ActivePowers.Add(power);

            }
            else if (key.Contains("BRETTA_DIARY"))
            {
                Power power = new TouristPower();
                power.Enable();
                //text = power.Description;
                //ActivePowers.Add(power);
                //ActivePowers.Add(new TouristPower());
                //ActivePowers[0].Enable();
                
            }
            else if (key.Equals("PILGRIM_TAB_01"))
            {
                Power power = new PilgerPathPower();
                power.Enable();
                text += power.Description;
                ActivePowers.Add(power);
            }

            return text;
        }

        public void Unload()
        {
            // Reset the damage increase
            //_damageStacks = 0;
            Instance = null;
        }

        #endregion

        //private void ModHooks_SlashHitHook(Collider2D otherCollider, GameObject slash)
        //{
        //    // This event is fired multiple times, therefore we check every instance if an enemy was hit
        //    if (otherCollider.gameObject.GetComponent<HealthManager>())
        //        _hasHitEnemy = true;

        //    // To prevent running multiple coroutines
        //    if (_currentlyRunning)
        //        return;

        //    _currentlyRunning = true;
        //    GameManager.instance.StartCoroutine(HitCooldown());
        //}

        //private int EmpowerNail(string name, int damage)
        //{
        //    if (string.Equals(name,"nailDamage"))
        //        damage += _damageStacks;
        //    return damage;
        //}

        //IEnumerator HitCooldown()
        //{
        //    // Give the event handler time to acknowledge a hit.
        //    yield return new WaitForSeconds(0.25f);

        //    if (_hasHitEnemy)
        //    {
        //        if (_damageStacks < 10)
        //            _damageStacks++;
        //    }
        //    else
        //        _damageStacks = 0;

        //    UpdateNail();
        //}

        //private void UpdateNail()
        //{
        //    IEnumerator WaitThenUpdate()
        //    {
        //        yield return null;
        //        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
        //    }
        //    GameManager.instance.StartCoroutine(WaitThenUpdate());

        //    _hasHitEnemy = false;
        //    _currentlyRunning = false;
        //}
    }
}
