using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;

using ItemChanger.FsmStateActions;
using ItemChanger.Placements;
using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.Helper;
using LoreMaster.ItemChangerData.Other;
using LoreMaster.Manager;
using Modding;
using MonoMod.Cil;
using MoreDoors.Data;
using MoreDoors.IC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

internal class TreasureHunterPower : Power
{
    #region Members

    private static Sprite _emptySprite;

    private static Sprite _chartSprite;

    private static Sprite[] _chartImages = new Sprite[14];

    private static Sprite[] _treasureSprites = new Sprite[11];

    private static GameObject[] _inventoryItems = new GameObject[21];

    private readonly string[] _specialRelics = new string[] { "silksongJournal", "silverSeal", "bronzeKingIdol", "goldenArcaneEgg" };

    private readonly int[] _treasureBonus = new int[] { 100, 225, 400, 600 };

    private static string[] _locationNames = new string[]
    {
        LocationList.Treasure_Howling_Cliffs,
        LocationList.Treasure_Crossroads,
        LocationList.Treasure_Greenpath,
        LocationList.Treasure_Fog_Canyon,
        LocationList.Treasure_Fungal_Wastes,
        LocationList.Treasure_City_Of_Tears,
        LocationList.Treasure_Waterways,
        LocationList.Treasure_Deepnest,
        LocationList.Treasure_Ancient_Basin,
        LocationList.Treasure_Kingdoms_Edge,
        LocationList.Treasure_Crystal_Peaks,
        LocationList.Treasure_Resting_Grounds,
        LocationList.Treasure_Queens_Garden,
        LocationList.Treasure_White_Palace,
    };

    #endregion

    #region Constructors

    public TreasureHunterPower() : base("Treasure Hunter", Area.CityOfTears)
    {
        _treasureSprites[0] = SpriteHelper.CreateSprite<LoreMaster>("Base.Silksong_Journal");
        _treasureSprites[1] = SpriteHelper.CreateSprite<LoreMaster>("Base.Silver_Seal");
        _treasureSprites[2] = SpriteHelper.CreateSprite<LoreMaster>("Base.Bronze_King_Idol");
        _treasureSprites[3] = SpriteHelper.CreateSprite<LoreMaster>("Base.Golden_Egg");
        _treasureSprites[4] = SpriteHelper.CreateSprite<LoreMaster>("Base.MagicKey");
        _treasureSprites[5] = SpriteHelper.CreateSprite<LoreMaster>("Base.Dream_Medallion");
        _treasureSprites[6] = SpriteHelper.CreateSprite<LoreMaster>("Base.Lemms_Order");
        //_treasureSprites[7] = Finder.GetItem(ItemNames.Wanderers_Journal).UIDef.GetSprite();
        //_treasureSprites[8] = Finder.GetItem(ItemNames.Hallownest_Seal).UIDef.GetSprite();
        //_treasureSprites[9] = Finder.GetItem(ItemNames.Kings_Idol).UIDef.GetSprite();
        //_treasureSprites[10] = Finder.GetItem(ItemNames.Arcane_Egg).UIDef.GetSprite();
        _emptySprite = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Backboards/BB 3").GetComponent<SpriteRenderer>().sprite;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Contains the flags to indicate, if the charts have been obtained.
    /// </summary>
    public static bool[] HasCharts { get; set; } = new bool[14];

    /// <summary>
    /// Gets or sets the flag that indicates, if the player can purchase treasure charts from iselda.
    /// </summary>
    public static bool CanPurchaseTreasureCharts { get; set; }

    /// <summary>
    /// Gets or sets the state of the special treasure items.
    /// </summary>
    public static Dictionary<string, TreasureState> Treasures { get; set; } = new()
    {
        {"silksongJournal", TreasureState.NotObtained},
        {"silverSeal", TreasureState.NotObtained},
        {"bronzeKingIdol", TreasureState.NotObtained},
        {"goldenArcaneEgg", TreasureState.NotObtained},
        {"magicKey", TreasureState.NotObtained },
        {"dreamMedallion", TreasureState.NotObtained }
    };

    #endregion

    #region Event handler

    private void ActivateGameObject_OnEnter(On.HutongGames.PlayMaker.Actions.ActivateGameObject.orig_OnEnter orig, ActivateGameObject self)
    {
        orig(self);
        if (string.Equals(self.Fsm.GameObjectName, "Shop Menu") && string.Equals(self.Fsm.Name, "shop_control")
            && string.Equals(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, "Ruins1_05b")
            && string.Equals(self.State.Name, "Activate Item List"))
            LoreMaster.Instance.Handler.StartCoroutine(WaitForItemList(self));
    }

    private void EnemyDeathEffects_EmitEssence(ILContext il)
    {
        if (Treasures["dreamMedallion"] == TreasureState.NotObtained)
            return;
        ILCursor cursor = new(il);

        cursor.Goto(0);
        if (cursor.TryGotoNext(MoveType.After,
            x => x.MatchCall(typeof(UnityEngine.Random), "Range")))
            cursor.EmitDelegate<Func<int, int>>(orig => LoreMaster.Instance.Generator.Next(0, PlayerData.instance.GetBool("equippedCharm_30") ? 5 : 10) == 0 ? 0 : orig);
    }

    private void GetPlayerDataInt_OnEnter(On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.orig_OnEnter orig, GetPlayerDataInt self)
    {
        orig(self);
        if (Treasures["magicKey"] != TreasureState.NotObtained && string.Equals(self.intName.Value, nameof(PlayerData.instance.simpleKeys))
            && self.State.Name.Contains("Key?"))
            self.Fsm.Variables.FindFsmInt("Simple Keys").Value++;
    }

    private void SetPlayerDataBool_OnEnter(On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.orig_OnEnter orig, SetPlayerDataBool self)
    {
        if (self.IsCorrectContext("npc_control", "Relic Dealer", "Convo End"))
            if (ModHooks.GetMod("QoL", true) is Mod)
                QoLQuickSell();
        orig(self);
    }

    private void CallMethodProper_OnEnter(On.HutongGames.PlayMaker.Actions.CallMethodProper.orig_OnEnter orig, CallMethodProper self)
    {
        try
        {
            if (string.Equals(self.State.Name, "Sell Item") && string.Equals(self.Fsm.Name, "Confirm Control") && string.Equals(self.methodName.Value, "AddGeo"))
            {
                if (State == PowerState.Twisted)
                    self.parameters[0].SetValue(_treasureBonus[self.Fsm.Variables.FindFsmInt("Relic Number").Value - 1]);
                else if (Treasures[_specialRelics[self.Fsm.Variables.FindFsmInt("Relic Number").Value - 1]] == TreasureState.GivenLemm)
                    self.parameters[0].SetValue(_treasureBonus[self.Fsm.Variables.FindFsmInt("Relic Number").Value - 1] * 3);
                else if (Treasures[_specialRelics[self.Fsm.Variables.FindFsmInt("Relic Number").Value - 1]] == TreasureState.Obtained)
                    self.parameters[0].SetValue(500);
            }
            else if (string.Equals(self.State.Name, "Convo") && string.Equals(self.Fsm.Name, "Relic Discussions"))
            {
                string key = new(self.Fsm.Variables.FindFsmString("Convo Prefix").Value.Substring("RELICDEALER_".Length).TakeWhile(x => !x.Equals('_')).ToArray());
                string treasure = null;
                switch (key)
                {
                    case "JOURNAL":
                        treasure = "silksongJournal";
                        break;
                    case "SEAL":
                        treasure = "silverSeal";
                        break;
                    case "IDOL":
                        treasure = "bronzeKingIdol";
                        break;
                    case "EGG":
                        treasure = "goldenArcaneEgg";
                        break;
                }
                key = key[0] + key.Substring(1).ToLower();
                if (Treasures[treasure] == TreasureState.Obtained)
                {
                    Treasures[treasure] = TreasureState.GivenLemm;
                    self.parameters[0].SetValue($"Sold{key}");
                }
            }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error while modifying relic price: " + exception.Message);
            LoreMaster.Instance.LogError(exception.StackTrace);
        }
        orig(self);
    }

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        try
        {
            if (string.Equals(self.FsmName, "Conversation Control") && (string.Equals(self.gameObject.name, "Love Door") || string.Equals(self.gameObject.name, "Mage Door")))
            {
                FsmState extraState = new(self.Fsm)
                {
                    Name = "Check Magic Key",
                    Actions = new FsmStateAction[]
                    {
                    new Lambda(() => self.SendEvent(Treasures["magicKey"] != TreasureState.NotObtained ? "KEY" : "NO KEY"))
                    }
                };
                self.AddState(extraState);
                extraState.AddTransition("KEY", "Box Up YN");
                extraState.AddTransition("NO KEY", "Box Up");
                self.GetState("Check Key").RemoveTransitionsTo("Box Up");
                self.GetState("Check Key").AddTransition("NO KEY", "Check Magic Key");
            }
            else if (string.Equals(self.FsmName, "Control") && string.Equals(self.gameObject.name, "abyss_door"))
                self.GetState("Check").AddActions(new Lambda(() =>
                {
                    if (Treasures["magicKey"] != TreasureState.NotObtained)
                        self.SendEvent("READY");
                }));
            else if ((string.Equals(self.FsmName, "Conversation Control") && string.Equals(self.gameObject.name, "Tram Call Box")))
            {
                FsmState extraState = new(self.Fsm)
                {
                    Name = "Check Magic Key",
                    Actions = new FsmStateAction[]
                    {
                        new Lambda(() => self.SendEvent(Treasures["magicKey"] != TreasureState.NotObtained ? "YES" : "NO"))
                    }
                };
                self.AddState(extraState);
                extraState.AddTransition("YES", "Box Up YN");
                extraState.AddTransition("NO", "Box Up");
                self.GetState("Got Pass?").AdjustTransition("NO", extraState.Name);
            }
            else if (string.Equals(self.FsmName, "Tram Door") && string.Equals(self.gameObject.name, "Door Inspect"))
            {
                FsmState extraState = new(self.Fsm)
                {
                    Name = "Check Magic Key",
                    Actions = new FsmStateAction[]
                    {
                        new Lambda(() => self.SendEvent(Treasures["magicKey"] != TreasureState.NotObtained ? "PASS" : "NO PASS"))
                    }
                };
                self.AddState(extraState);
                extraState.AddTransition("PASS", "Box Up YN");
                extraState.AddTransition("NO PASS", "Box Up");
                self.GetState("Check Pass").AdjustTransition("NO PASS", extraState.Name);
            }
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error occured while modifying Master key doors: " + exception.Message);
            LoreMaster.Instance.LogError(exception.StackTrace);
        }
        orig(self);
    }

    private void PlayerDataIntAdd_OnEnter(On.HutongGames.PlayMaker.Actions.PlayerDataIntAdd.orig_OnEnter orig, PlayerDataIntAdd self)
    {
        if (string.Equals(self.State.Name, "Activate") && string.Equals(self.Fsm.Name, "Conversation Control")
            && string.Equals(self.intName?.Value, "simpleKeys") && self.amount.Value < 0)
            PlayerData.instance.IncrementInt("simpleKeys");
        orig(self);
    }

    private void DoorPreview(On.ShowPromptMarker.orig_OnEnter orig, ShowPromptMarker self)
    {
        orig(self);
        if (Treasures["magicKey"] != TreasureState.NotObtained && self.IsCorrectContext("npc_control", null, "In Range") && self.Fsm.FsmComponent.GetComponent<DoorNameMarker>() is DoorNameMarker door)
            MoreDoorsPreview(door.DoorName);
    }

    #endregion

    #region Control

    protected override void Initialize()
    {
        _treasureSprites[7] = Finder.GetItem(ItemNames.Wanderers_Journal).UIDef.GetSprite();
        _treasureSprites[8] = Finder.GetItem(ItemNames.Hallownest_Seal).UIDef.GetSprite();
        _treasureSprites[9] = Finder.GetItem(ItemNames.Kings_Idol).UIDef.GetSprite();
        _treasureSprites[10] = Finder.GetItem(ItemNames.Arcane_Egg).UIDef.GetSprite();
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.OnEnter += GetPlayerDataInt_OnEnter;
        On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter += ActivateGameObject_OnEnter;
        On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter += SetPlayerDataBool_OnEnter;
        On.HutongGames.PlayMaker.Actions.CallMethodProper.OnEnter += CallMethodProper_OnEnter;
        On.HutongGames.PlayMaker.Actions.PlayerDataIntAdd.OnEnter += PlayerDataIntAdd_OnEnter;
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        IL.EnemyDeathEffects.EmitEssence += EnemyDeathEffects_EmitEssence;
        if (ModHooks.GetMod("MoreDoors", true) is Mod)
            On.ShowPromptMarker.OnEnter += DoorPreview;
#if DEBUG
        Treasures["magicKey"] = TreasureState.Obtained;
#endif
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.OnEnter -= GetPlayerDataInt_OnEnter;
        On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter -= ActivateGameObject_OnEnter;
        On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter -= SetPlayerDataBool_OnEnter;
        On.HutongGames.PlayMaker.Actions.CallMethodProper.OnEnter -= CallMethodProper_OnEnter;
        On.HutongGames.PlayMaker.Actions.PlayerDataIntAdd.OnEnter -= PlayerDataIntAdd_OnEnter;
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
        IL.EnemyDeathEffects.EmitEssence -= EnemyDeathEffects_EmitEssence;
        if (ModHooks.GetMod("MoreDoors", true) is Mod)
            On.ShowPromptMarker.OnEnter -= DoorPreview;
    }

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter += ActivateGameObject_OnEnter;
        On.HutongGames.PlayMaker.Actions.CallMethodProper.OnEnter += CallMethodProper_OnEnter;
        On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter += SetPlayerDataBool_OnEnter;
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter -= ActivateGameObject_OnEnter;
        On.HutongGames.PlayMaker.Actions.CallMethodProper.OnEnter -= CallMethodProper_OnEnter;
        On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter -= SetPlayerDataBool_OnEnter;
    }

    #endregion

    #region Methods

    private IEnumerator WaitForItemList(ActivateGameObject self)
    {
        yield return null;
        TreasureState[] states = Treasures.Values.Take(4).ToArray();
        for (int i = 1; i < 5; i++)
        {
            if (State == PowerState.Twisted)
            {
                Transform relic = self.Fsm.GameObject.transform.Find($"Item List/Shop Item Rlc {i}(Clone)");
                if (relic == null)
                    continue;

                relic.GetComponent<ShopItemStats>().cost = _treasureBonus[i - 1];
                relic.transform.Find("Item Sprite").GetComponent<SpriteRenderer>().sprite = _treasureSprites[i + 6];
                relic.transform.Find("Item cost").GetComponent<TextMeshPro>().text = $"<color=#ff0000>{_treasureBonus[i - 1]}</color>";
            }
            else if (states[i - 1] == TreasureState.NotObtained)
                continue;
            else if (states[i - 1] == TreasureState.Obtained)
            {
                GameObject relic = self.Fsm.GameObject.transform.Find($"Item List/Shop Item Rlc {i}(Clone)").gameObject;
                ShopItemStats stats = relic.GetComponent<ShopItemStats>();
                stats.descConvo = "Shop_Desc_Special_Treasure_" + i;
                stats.cost = 500;
                stats.nameConvo = "Shop_Title_Special_Treasure_" + i;
                // Since it is not possible to have multiple of the relics, the number is unnecessary.
                GameObject.Destroy(relic.transform.Find("Amount"));
                relic.transform.Find("Item Sprite").GetComponent<SpriteRenderer>().sprite = _treasureSprites[i - 1];
                relic.transform.Find("Item cost").GetComponent<TextMeshPro>().text = "<color=#7FFF7B>500</color>";
            }
            else
            {
                Transform relic = self.Fsm.GameObject.transform.Find($"Item List/Shop Item Rlc {i}(Clone)");
                if (relic == null)
                    continue;

                relic.GetComponent<ShopItemStats>().cost = _treasureBonus[i - 1] * 3;
                relic.transform.Find("Item Sprite").GetComponent<SpriteRenderer>().sprite = _treasureSprites[i + 6];
                relic.transform.Find("Item cost").GetComponent<TextMeshPro>().text = $"<color=#7FFF7B>{_treasureBonus[i - 1] * 3}</color>";
            }
        }
    }

    /// <summary>
    /// Makes sure the geo prices of the relics are considered.
    /// </summary>
    private void QoLQuickSell()
    {
        if (!QoL.Modules.NPCSellAll.LemmSellAll || PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_10)))
            return;
        int finalGeo = 0;
        int trinket = 1;
        foreach (string key in _specialRelics)
        {
            if (State == PowerState.Twisted)
                finalGeo -= PlayerData.instance.GetInt($"trinket{trinket}") * _treasureBonus[trinket - 1];
            else
            {
                if (Treasures[key] == TreasureState.Obtained)
                {
                    finalGeo += 500;
                    PlayerData.instance.DecrementInt($"trinket{trinket}");
                    Treasures[key] = TreasureState.GivenLemm;
                }
                if (Treasures[key] == TreasureState.GivenLemm)
                    finalGeo += PlayerData.instance.GetInt($"trinket{trinket}") * _treasureBonus[trinket - 1];
            }
            trinket++;
        }
        if (finalGeo != 0)
            HeroController.instance.AddGeo(finalGeo);
    }

    private void MoreDoorsPreview(string doorName)
    {
        string keyName = DoorData.GetDoor(doorName).Key.ItemName;
        AbstractPlacement placement = ItemChanger.Internal.Ref.Settings.GetPlacements().FirstOrDefault(x => x.Items.Any(x => x.name == keyName));
        if (placement == null || placement.Items.First(x => x.name == keyName).IsObtained())
            return;

        PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value, "Display");
        playMakerFSM.FsmVariables.GetFsmInt("Convo Amount").Value = 1;
        playMakerFSM.FsmVariables.GetFsmString("Convo Title").Value = "Master_Key_Preview-" + placement.Name.Replace('_', ' ').Replace('-', ' ');
        playMakerFSM.SendEvent("DISPLAY ENEMY DREAM");
    }

    #endregion

    #region Inventory Screen

    public static void BuildInventory(GameObject treasureChartPage)
    {
        // If more then the cursor exists
        if (treasureChartPage.transform.childCount > 1)
            return;
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        ModHooks.SetPlayerBoolHook += SetPlayerDataBool;
        ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        GameObject charts = new("Charts");
        charts.transform.SetParent(treasureChartPage.transform);
        charts.transform.localScale = new(1f, 1f, 1f);
        charts.transform.localPosition = new(0f, 0f, 0f);

        GameObject items = new("Treasures");
        items.transform.SetParent(treasureChartPage.transform);
        items.transform.localScale = new(1f, 1f, 1f);
        items.transform.localPosition = new(0f, 0f, 0f);

        GameObject chartImage = new("Active Chart");
        chartImage.transform.SetParent(treasureChartPage.transform);
        chartImage.transform.localScale = new(1f, 1f, 1f);
        chartImage.transform.localPosition = new(10f, -10f, 0f);
        chartImage.layer = treasureChartPage.layer;
        SpriteRenderer image = chartImage.AddComponent<SpriteRenderer>();
        image.sortingLayerID = 629535577;
        image.sortingLayerName = "HUD";

        GameObject titleObject = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Name").gameObject);
        titleObject.transform.SetParent(treasureChartPage.transform);
        titleObject.transform.localPosition = new(10f, -11f, -2f);
        TextMeshPro title = titleObject.GetComponent<TextMeshPro>();
        title.text = "";

        GameObject descriptionObject = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Desc").gameObject);
        descriptionObject.transform.SetParent(treasureChartPage.transform);
        descriptionObject.transform.localPosition = new(10f, -12f, 1f);
        TextMeshPro desc = descriptionObject.GetComponent<TextMeshPro>();
        desc.text = "";

        float xPosition = -9.1f; // in 3f steps
        float yPosition = 4.6f; // in -1.6f steps
        for (int i = 0; i < 14; i++)
        {
            GameObject map = new("Chart " + (i + 1));
            map.transform.SetParent(charts.transform);
            map.transform.localScale = new(1.5f, 1.5f, 1f);
            map.transform.position = new(xPosition, yPosition, 0f);
            map.AddComponent<BoxCollider2D>();
            map.layer = treasureChartPage.layer;
            map.AddComponent<SpriteRenderer>();
            map.GetComponent<SpriteRenderer>().sortingLayerID = 629535577;
            map.GetComponent<SpriteRenderer>().sortingLayerName = "HUD";
            xPosition += 3f;
            if (xPosition > 9.1f)
            {
                xPosition = -9.1f;
                yPosition -= 1.6f;
            }
            _chartImages[i] = SpriteHelper.CreateSprite<LoreMaster>("Base.Treasure_Chart_" + (i + 1), ".jpg");
            _inventoryItems[i] = map;
        }

        GameObject item = new("Lemm Order");
        item.transform.SetParent(items.transform);
        item.transform.localScale = new(2f, 2f, 1f);
        item.transform.localPosition = new(-6, -9.5f, 0f);
        item.AddComponent<BoxCollider2D>();
        item.layer = treasureChartPage.layer;
        item.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<LoreMaster>("Base.Lemms_Order");
        item.GetComponent<SpriteRenderer>().sortingLayerID = 629535577;
        item.GetComponent<SpriteRenderer>().sortingLayerName = "HUD";
        _inventoryItems[14] = item;

        int index = 0;
        xPosition = -3;
        yPosition = -8;
        foreach (string key in Treasures.Keys)
        {
            GameObject treasure = new(key);
            treasure.transform.SetParent(items.transform);
            treasure.transform.localScale = new(1.5f, 1.5f, 1f);
            treasure.transform.localPosition = new(xPosition, yPosition, 0f);
            treasure.AddComponent<BoxCollider2D>();
            treasure.layer = treasureChartPage.layer;
            treasure.AddComponent<SpriteRenderer>().sprite = _treasureSprites[index];
            treasure.GetComponent<SpriteRenderer>().sortingLayerID = 629535577;
            treasure.GetComponent<SpriteRenderer>().sortingLayerName = "HUD";
            index++;
            xPosition += 3;
            if (xPosition > 3)
            {
                xPosition = -3;
                yPosition -= 3;
            }
            _inventoryItems[index + 14] = treasure;
        }
        // Modify inventory movement
        PlayMakerFSM inventoryFsm = treasureChartPage.LocateMyFSM("Empty UI");

        // Add index variable
        List<FsmInt> intVariables = inventoryFsm.FsmVariables.IntVariables.ToList();
        FsmInt indexVariable = new() { Name = "ItemIndex", Value = 0 };
        intVariables.Add(indexVariable);
        inventoryFsm.FsmVariables.IntVariables = intVariables.ToArray();

        // Removing the jump from arrow button to arrow button.
        inventoryFsm.GetState("L Arrow").RemoveTransitionsTo("R Arrow");
        inventoryFsm.GetState("R Arrow").RemoveTransitionsTo("L Arrow");

        FsmState currentWorkingState = inventoryFsm.GetState("Init Heart Piece");
        currentWorkingState.Name = "Init Lore";
        currentWorkingState.RemoveTransitionsTo("L Arrow");
        currentWorkingState.AddActions(new Lambda(() =>
        {
            foreach (Transform child in charts.transform)
                child.gameObject.SetActive(true);
        }));

        // Create main state
        inventoryFsm.AddState(new FsmState(inventoryFsm.Fsm)
        {
            Name = "Handle Item",
            Actions = new FsmStateAction[]
            {
                    new Lambda(() => inventoryFsm.gameObject.LocateMyFSM("Update Cursor").FsmVariables.FindFsmGameObject("Item").Value = _inventoryItems[indexVariable.Value]),
                    new SetSpriteRendererOrder()
                    {
                        gameObject = new() { GameObject = inventoryFsm.FsmVariables.FindFsmGameObject("Cursor Glow") },
                        order = 0,
                        delay = 0f
                    },
                    new Lambda(() => inventoryFsm.gameObject.LocateMyFSM("Update Cursor").SendEvent("UPDATE CURSOR")),
                    new Lambda(() =>
                    {
                        if(indexVariable.Value < 14)
                        {
                            titleObject.SetActive(false);
                            descriptionObject.SetActive(false);
                            chartImage.SetActive(true);
                            image.sprite = HasCharts[indexVariable.Value] ? _chartImages[indexVariable.Value] : null;
                            image.color = Finder.GetLocation(_locationNames[indexVariable.Value]).Placement.Items.All(x => x.IsObtained()) ? Color.grey : Color.white;
                        }
                        else
                        {
                            bool hasTreasure = indexVariable.Value == 14 ? CanPurchaseTreasureCharts : Treasures.Select(x => x.Value).ToArray()[indexVariable.Value  - 15] != TreasureState.NotObtained;
                            titleObject.SetActive(hasTreasure);
                            descriptionObject.SetActive(hasTreasure);
                            chartImage.SetActive(false);
                            title.text = Properties.TreasureHunter.ResourceManager.GetString("Shop_Title_Special_Treasure_"+(indexVariable.Value - 14));
                            desc.text = Properties.TreasureHunter.ResourceManager.GetString("Shop_Desc_Special_Treasure_"+(indexVariable.Value - 14));
                        }
                    })
            }
        });

        // Add transition from init to main
        currentWorkingState.AddTransition("FINISHED", "Handle Item");
        AddInventoryMovement(inventoryFsm, indexVariable);
        _chartSprite = SpriteHelper.CreateSprite<LoreMaster>("Base.Treasure_Chart");
        treasureChartPage.SetActive(false);
    }

    public static void UpdateTreasurePage()
    {
        try
        {
            for (int i = 0; i < 14; i++)
            {
                if (!HasCharts[i])
                {
                    _inventoryItems[i].GetComponent<SpriteRenderer>().sprite = _emptySprite;
                    _inventoryItems[i].GetComponent<SpriteRenderer>().color = Color.white;
                }
                else
                {
                    _inventoryItems[i].GetComponent<SpriteRenderer>().sprite = _chartSprite;
                    if (ItemManager.GetPlacementByName<MutablePlacement>(_locationNames[i]).Items.Any(x => !x.IsObtained()))
                        _inventoryItems[i].GetComponent<SpriteRenderer>().color = Color.white;
                    else
                        _inventoryItems[i].GetComponent<SpriteRenderer>().color = new(0.2f, 0.2f, 0.2f);
                }
            }
            _inventoryItems[14].GetComponent<SpriteRenderer>().sprite = CanPurchaseTreasureCharts ? _treasureSprites[6] : _emptySprite;
            TreasureState[] states = Treasures.Values.ToArray();
            for (int i = 15; i < 21; i++)
                _inventoryItems[i].GetComponent<SpriteRenderer>().sprite = states[i - 15] != TreasureState.NotObtained ? _treasureSprites[i - 15] : _emptySprite;
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("An error occured while updating the treasure page: " + exception.Message);
            LoreMaster.Instance.LogError("An error occured while updating the treasure page: " + exception.StackTrace);
        }
    }

    private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        if (key.Contains("_Special_Treasure_") || key.StartsWith("Sold"))
            orig = Properties.TreasureHunter.ResourceManager.GetString(key);
        else if (key.Contains("Treasure_"))
            orig = Finder.GetLocation(key.Substring(0, key.Length - 2)).Placement.GetUIName();
        return orig;
    }

    private static void AddInventoryMovement(PlayMakerFSM fsm, FsmInt indexVariable)
    {
        FsmState currentWorkingState = fsm.GetState("Handle Item");

        fsm.AddState(new FsmState(fsm.Fsm) { Name = "Up Press" });
        fsm.AddState(new FsmState(fsm.Fsm) { Name = "Right Press" });
        fsm.AddState(new FsmState(fsm.Fsm) { Name = "Down Press" });
        fsm.AddState(new FsmState(fsm.Fsm) { Name = "Left Press" });

        currentWorkingState.AddTransition("UI UP", "Up Press");
        currentWorkingState.AddTransition("UI RIGHT", "Right Press");
        currentWorkingState.AddTransition("UI DOWN", "Down Press");
        currentWorkingState.AddTransition("UI LEFT", "Left Press");

        // Left
        currentWorkingState = fsm.GetState("Left Press");
        currentWorkingState.AddTransition("OUT", "L Arrow");
        currentWorkingState.AddTransition("FINISHED", "Handle Item");
        currentWorkingState.AddActions(new Lambda(() =>
        {
            if (indexVariable.Value == 0 || indexVariable.Value == 7 || indexVariable.Value == 14)
            {
                indexVariable.Value = -2;
                fsm.SendEvent("OUT");
                return;
            }
            else if (indexVariable.Value == 18)
                indexVariable.Value = 14;
            else
                indexVariable.Value = indexVariable.Value == -1 ? 6 : indexVariable.Value - 1;
            fsm.SendEvent("FINISHED");
        }));
        fsm.GetState("R Arrow").AddTransition("UI LEFT", "Left Press");

        // Right
        currentWorkingState = fsm.GetState("Right Press");
        currentWorkingState.AddTransition("OUT", "R Arrow");
        currentWorkingState.AddTransition("FINISHED", "Handle Item");
        currentWorkingState.AddActions(new Lambda(() =>
        {
            if (indexVariable.Value == 6 || indexVariable.Value == 13 || indexVariable.Value == 20 || indexVariable.Value == 17)
            {
                indexVariable.Value = -1;
                fsm.SendEvent("OUT");
                return;
            }
            indexVariable.Value = indexVariable.Value == -2 ? 0 : indexVariable.Value + 1;
            fsm.SendEvent("FINISHED");
        }));
        fsm.GetState("L Arrow").AddTransition("UI RIGHT", "Right Press");

        // Up
        currentWorkingState = fsm.GetState("Up Press");
        currentWorkingState.AddTransition("FINISHED", "Handle Item");
        currentWorkingState.AddActions(new Lambda(() =>
        {
            // For charts
            if (indexVariable.Value <= 6)
                indexVariable.Value += 14;
            else if (indexVariable.Value < 14)
                indexVariable.Value -= 7;
            // For treasures
            else if (indexVariable.Value < 18)
                indexVariable.Value = 7;
            else
                indexVariable.Value -= 3;
            fsm.SendEvent("FINISHED");
        }));

        // Down
        currentWorkingState = fsm.GetState("Down Press");
        currentWorkingState.AddTransition("FINISHED", "Handle Item");
        currentWorkingState.AddActions(new Lambda(() =>
        {
            // For charts
            if (indexVariable.Value >= 7 && indexVariable.Value < 14)
                indexVariable.Value = 14;
            else if (indexVariable.Value < 7)
                indexVariable.Value += 7;
            // For treasure
            else if (indexVariable.Value < 18 && indexVariable.Value > 14)
                indexVariable.Value += 3;
            else
                indexVariable.Value = 0;
            fsm.SendEvent("FINISHED");
        }));
    }

    private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (string.Equals(name, "hasTreasureCharts"))
            return HasCharts.Any(x => x) || CanPurchaseTreasureCharts || Treasures.Any(x => x.Value != TreasureState.NotObtained);
        else if (string.Equals(name, "lemm_allow"))
            return CanPurchaseTreasureCharts;
        return orig;
    }

    private static bool SetPlayerDataBool(string name, bool orig)
    {
        if (name.Contains("Treasure_Chart_"))
        {
            int number = Convert.ToInt32(new string(name.Skip("Treasure_Chart_".Length).ToArray()));
            HasCharts[number - 1] = orig;
            UpdateTreasurePage();
        }
        else if (Treasures.ContainsKey(name) && Treasures[name] == TreasureState.NotObtained)
        {
            int treasureIndex = ItemChanger.Extensions.Extensions.IndexOf(Treasures.Keys, name);
            if (treasureIndex < 4)
            {
                PlayerData.instance.SetBool("foundTrinket" + (treasureIndex + 1), true);
                PlayerData.instance.IncrementInt("trinket" + (treasureIndex + 1));
            }
            Treasures[name] = TreasureState.Obtained;
            UpdateTreasurePage();
        }
        else if (string.Equals(name, "lemm_Allow"))
        {
            CanPurchaseTreasureCharts = orig;
            UpdateTreasurePage();
        }
        // Force lemm after 1 dreamer to be outside. (This is needed because if QoL skips cutscenes, the flag normally gets skipped)
        else if (orig && (string.Equals(name, "monomonDefeated") || string.Equals(name, "hegemolDefeated") || string.Equals(name, "lurienDefeated")))
            if (!PlayerData.instance.GetBool("monomonDefeated") && !PlayerData.instance.GetBool("hegemolDefeated") && !PlayerData.instance.GetBool("lurienDefeated"))
            {
                PlayerData.instance.SetBool("marmOutside", true);
                PlayerData.instance.SetBool(nameof(PlayerData.instance.hornetFountainEncounter), true);
            }

        return orig;
    }

    #endregion
}