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
using LoreMaster.LorePowers.Deepnest;
using LoreMaster.LorePowers.WhitePalace;
using GlobalEnums;

namespace LoreMaster
{
    public class LoreMaster : Mod
    {
        #region Members

        private readonly Dictionary<string, Power> _powerActivators = new()
        {
            // Ancient Basin
            {"ABYSS_TUT_TAB_01", new WeDontTalkAboutShadePower() },
            // City of Tears
            {"RUINS_TAB_01", new HotStreakPower() },
            {"FOUNTAIN_PLAQUE_MAIN", new TouristPower() },
            {"RUINS_MARISSA_POSTER", new MarissasAudiencePower() },
            {"MAGE_COMP_03", new OverwhelmingPower() },
            {"MAGE_COMP_01", new SoulExtractEfficiencyPower() },
            {"LURIAN_JOURNAL", new EyeOfTheWatcherPower() },
            // Dirtmouth/King's Pass
            {"TUT_TAB_01", new WellFocusedPower() },
            {"TUT_TAB_03", new ScrewTheRulesPower() },
            {"TUT_TAB_02", new TrueFormPower() },
            {"BRETTA", new HazardPower() },
            // Fog Canyon
            {"ARCHIVE_01", new FriendOfTheJellyPower() },
            {"ARCHIVE_02", new JellyBellyPower() },
            {"ARCHIVE_03", new JellyFlowPower() },
            // Crossroads
            {"PILGRIM_TAB_01", new PilgerPathPower() },
            {"COMPLETION_RATE_UNLOCKED", new GreaterMindPower() },
            // Fungal Wastes
            {"FUNG_TAB_04", new OneOfUsPower() },
            {"FUNG_TAB_01", new PaleLuckPower() },
            {"FUNG_TAB_02", new ImposterPower() },
            {"FUNG_TAB_03", new UnitedWeStandPower() },
            {"MANTIS_PLAQUE_01", new MantisStylePower() },
            {"MANTIS_PLAQUE_02", new EternalValorPower() },
            {"PILGRIM_TAB_02", new GloryPower() },
            // Greenpath
            {"GREEN_TABLET_01", new TouchGrassPower() },
            {"GREEN_TABLET_02", new GiftOnUnnPower() },
            {"GREEN_TABLET_03", new UnnMindblastPower() },
            {"GREEN_TABLET_05", new CamouflagePower() },
            {"GREEN_TABLET_06", new ReturnToUnnPower() },
            {"GREEN_TABLET_07", new RootedPower() },
            // Howling Cliffs
            {"CLIFFS_TAB_02", new LifebloodWingsPower() },
            {"JONI", new JonisProtectionPower() },
            // Queen's Garden
            {"XUN_GRAVE_INSPECT", new FlowerRingPower() },
            {"QUEEN", new QueenThornsPower() },
            // Resting Grounds
            {"DREAMERS", new DreamBlessingPower() },
            // Waterways
            {"DUNG_DEF_SIGN", new EternalSentinelPower() },
            {"FLUKE_HERMIT", new RelentlessSwarmPower() },
            // Kingdom's Edge
            {"MR_MUSH_RIDDLE_TAB_NORMAL", new WisdomOfTheSagePower() },
            {"BADOON", new ConcussiveStrikePower() },
            {"HIVEQUEEN", new YouLikeJazzPower() },
            // Deepnest
            {"MASKMAKER", new MaskOverchargePower() },
            {"MIDWIFE", new InfestedPower() },
            // White Palace
            {"WP_WORKSHOP_01", new ShadowForgedPower() },
            {"WP_THRONE_01", new ShiningBoundPower() },
            {"PLAQUE_WARN", new DiminishingCursePower() },
            {"EndOfPathOfPain", new SacredShellPower() }
        };

        private Area _lastMapArea;

        private readonly Dictionary<Area, List<MapZone>> _mapZones = new()
        {
            {Area.Dirtmouth, new(){MapZone.KINGS_PASS} },
            {Area.Crossroads, new(){MapZone.CROSSROADS, MapZone.TOWN, MapZone.TRAM_UPPER, MapZone.FINAL_BOSS} },
            {Area.Greenpath, new(){MapZone.GREEN_PATH, MapZone.NOEYES_TEMPLE}},
            {Area.FungalWastes, new(){MapZone.WASTES, MapZone.MANTIS_VILLAGE, MapZone.QUEENS_STATION} },
            {Area.FogCanyon, new(){MapZone.FOG_CANYON, MapZone.OVERGROWN_MOUND, MapZone.MONOMON_ARCHIVE} },
            {Area.KingdomsEdge, new(){MapZone.COLOSSEUM, MapZone.OUTSKIRTS, MapZone.HIVE} },
            {Area.Deepnest, new(){MapZone.DEEPNEST, MapZone.TRAM_LOWER, MapZone.BEASTS_DEN, MapZone.RUINED_TRAMWAY} },
            {Area.WaterWays, new(){MapZone.WATERWAYS, MapZone.GODS_GLORY, MapZone.GODSEEKER_WASTE, MapZone.ISMAS_GROVE} },
            {Area.Cliffs, new(){MapZone.CLIFFS, MapZone.JONI_GRAVE } },
            {Area.AncientBasin, new(){MapZone.BONE_FOREST, MapZone.PALACE_GROUNDS} },
            {Area.CityOfTears, new(){MapZone.CITY, MapZone.SOUL_SOCIETY, MapZone.LURIENS_TOWER, MapZone.LOVE_TOWER, MapZone.MAGE_TOWER } },
            {Area.RestingGrounds, new(){MapZone.RESTING_GROUNDS, MapZone.BLUE_LAKE} },
            {Area.WhitePalace, new(){MapZone.WHITE_PALACE} },
            {Area.Peaks, new(){MapZone.PEAK, MapZone.MINES, MapZone.CRYSTAL_MOUND} }
        };

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the running instance of the mod.
        /// </summary>
        public static LoreMaster Instance { get; set; }

        public Dictionary<string, Power> ActivePowers { get; set; } = new();

        public Dictionary<string, GameObject> PreloadedObjects { get; set; } = new Dictionary<string, GameObject>();

        public System.Random Generator { get; set; } = new System.Random();

        public bool UseHints { get; set; }

        /// <summary>
        /// Gets or sets all actions of powers that should be executed once the scene loaded. 
        /// This is used to prevent messing around with powers that may use activeSceneChanged.
        /// <para/>No power shall use activeSceneChanged!!!
        /// </summary>
        public Dictionary<string, Action> SceneActions { get; set; } = new();

        #endregion

        #region Configuration

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private void UIManager_StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            ItemChangerMod.CreateSettingsProfile();
            List<MutablePlacement> teleportItems = new();
            MutablePlacement teleportPlacement = new CoordinateLocation() { x = 35.0f, y = 5.4f, elevation = 0, sceneName = "Ruins1_27", name = "City_Teleporter" }.Wrap() as MutablePlacement;
            teleportPlacement.Cost = new Paypal { ToTemple = true };
            teleportPlacement.Add(new TouristMagnetItem(true, "City_Teleporter"));
            teleportItems.Add(teleportPlacement);

            MutablePlacement secondPlacement = new CoordinateLocation() { x = 57f, y = 5f, elevation = 0, sceneName = "Room_temple", name = "Temple_Teleporter" }.Wrap() as MutablePlacement;
            secondPlacement.Cost = new Paypal { ToTemple = false };
            secondPlacement.Add(new TouristMagnetItem(false, "Temple_Teleporter"));
            teleportItems.Add(secondPlacement);
            ItemChangerMod.AddPlacements(teleportItems);
            orig(self, permaDeath, bossRush);
        }

        public override List<(string, string)> GetPreloadNames() => new List<(string, string)>()
        {
            ("RestingGrounds_08", "Ghost Battle Revek"),
            ("sharedassets156", "Lil Jellyfish"),
            ("sharedassets34", "Shot Mantis"),
            ("GG_Hollow_Knight", "Battle Scene/HK Prime/Focus Blast/focus_ring"),
            ("GG_Hollow_Knight", "Battle Scene/HK Prime/Focus Blast/focus_rune")
        };

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            if (Instance != null)
                return;
            Instance = this;
            ModHooks.LanguageGetHook += GetText;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            On.UIManager.StartNewGame += UIManager_StartNewGame;
            On.UIManager.ContinueGame += UIManager_ContinueGame;

            foreach (string key in preloadedObjects.Keys)
                foreach (string subKey in preloadedObjects[key].Keys)
                    if (!PreloadedObjects.ContainsKey(subKey))
                    {
                        PreloadedObjects.Add(subKey, preloadedObjects[key][subKey]);
                        GameObject.DontDestroyOnLoad(preloadedObjects[key][subKey]);
                    }
        }

        private void UIManager_ContinueGame(On.UIManager.orig_ContinueGame orig, UIManager self)
        {
            orig(self);
        }

        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            if (arg1.name.Equals("Menu_Title"))
                foreach (Power power in ActivePowers.Values)
                    power.DisablePower();
            else if (arg0.name.Contains("Menu") && GameManager.instance.IsGameplayScene())
                GameManager.instance.StartCoroutine(ManageSceneActions());
            else
                GameManager.instance.StartCoroutine(ManageSceneActions());
        }

        public void Unload()
        {
            Instance = null;
            ModHooks.LanguageGetHook -= GetText;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
            On.UIManager.StartNewGame -= UIManager_StartNewGame;
            On.UIManager.ContinueGame -= UIManager_ContinueGame;
        }

        #endregion

        #region Management

        /// <summary>
        /// This is the main control, which determines which power is on.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sheetTitle"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private string GetText(string key, string sheetTitle, string text)
        {
            key = ModifyKey(key);
            if (_powerActivators.ContainsKey(key))
            {
                try
                {
                    Power power;
                    if (!ActivePowers.ContainsKey(key))
                    {
                        power = _powerActivators[key];
                        power.EnablePower();
                        ActivePowers.Add(key, power);
                    }
                    else
                        power = ActivePowers[key];
                    if (!string.IsNullOrEmpty(power.CustomText))
                        text = power.CustomText;
                    text += "<br>[" + power.PowerName + "]";
                    text += "<br>" + (UseHints ? power.Hint : power.Description);
                }
                catch (Exception exception)
                {
                    LogError(exception.Message);
                }
            }
            return text;
        }

        #endregion

        #region Private Methods

        private string ModifyKey(string key)
        {
            if (key.Contains("Bretta_Diary"))
                key = "BRETTA";
            else if (key.Contains("MIDWIFE"))
                key = "MIDWIFE";
            else if (key.Contains("BIG_CATERPILLAR"))
                key = "BADOON";
            else if (key.Contains("HIVEQUEEN"))
                key = "HIVEQUEEN";
            else if (key.Contains("JONI"))
                key = "JONI";
            else if (key.Contains("FLUKE_HERMIT"))
                key = "FLUKE_HERMIT";
            else if (key.Take(5).Equals("QUEEN"))
                key = "QUEEN";
            else if (key.Contains("MASK_MAKER"))
                key = "MASK_MAKER";
            else if (key.Contains("DREAMERS_INSPECT"))
                key = "DREAMERS";

            return key;
        }

        private IEnumerator ManageSceneActions()
        {
            yield return new WaitForFinishedEnteringScene();
            Area newArea = _lastMapArea;
            if (!Enum.TryParse(GameManager.instance.GetCurrentMapZone(), out MapZone newMapZone))
                LogError("Couldn't convert map zone to enum. Value: " + GameManager.instance.GetCurrentMapZone());
            else if (newMapZone != MapZone.DREAM_WORLD)
            {
                bool foundResult = false;
                foreach (Area area in _mapZones.Keys)
                    if (_mapZones[area].Contains(newMapZone))
                    {
                        newArea = area;
                        foundResult = true;
                        break;
                    }
                if (!foundResult)
                    LogError("Couldn't find area: " + newMapZone);
            }

            if (_lastMapArea != newArea)
            {
                // Activate all local abilities
                if (!IsAreaGlobal(newArea))
                    foreach (Power power in ActivePowers.Values.Where(x => x.Location == newArea))
                        if (!power.Active && (power.Tag == PowerTag.Exclude || power.Tag == PowerTag.Local))
                            power.EnablePower();

                if (!IsAreaGlobal(_lastMapArea))
                    foreach (Power power in ActivePowers.Values.Where(x => x.Location == _lastMapArea))
                        if (power.Tag == PowerTag.Local || power.Tag == PowerTag.Exclude)
                            power.DisablePower();
            }

            foreach (Action action in SceneActions.Values)
                action();
            _lastMapArea = newArea;
        }

        /// <summary>
        /// Checks through all needed powers to determine if the powers should be granted globally.
        /// </summary>
        /// <param name="toCheck"></param>
        /// <returns></returns>
        private bool IsAreaGlobal(Area toCheck)
        {
            List<Power> neededAreaPowers = _powerActivators.Values.Where(x => x.Location == toCheck && (x.Tag == PowerTag.Local || x.Tag == PowerTag.Disabled)).ToList();
            foreach (Power neededPower in neededAreaPowers)
                if (!ActivePowers.ContainsValue(neededPower))
                    return false;
            return true;
        }

        #endregion
    }
}
