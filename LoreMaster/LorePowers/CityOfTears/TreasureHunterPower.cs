using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Items;
using ItemChanger.UIDefs;
using LoreMaster.CustomItem;
using LoreMaster.Enums;
using LoreMaster.Helper;
using Modding;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

internal class TreasureHunterPower : Power
{
    #region Members

    private static Sprite[] _chartImages = new Sprite[14];

    private static Sprite[] _treasureSprites = new Sprite[7];

    private static GameObject[] _mapItems = new GameObject[14];

    // Key scenes: Ruins1_31, Ruins2_11b, Crossroads_46
    //Lemm: Ruins1_05b
    #endregion

    #region Constructors

    public TreasureHunterPower() : base("Treasure Hunter", Area.CityOfTears)
    {
        _treasureSprites[0] = SpriteHelper.CreateSprite("Silksong_Journal",false);
        _treasureSprites[1] = SpriteHelper.CreateSprite("Silver_Seal", false);
        _treasureSprites[2] = SpriteHelper.CreateSprite("Bronze_King_Idol", false);
        _treasureSprites[3] = SpriteHelper.CreateSprite("Golden_Egg", false);
        _treasureSprites[4] = SpriteHelper.CreateSprite("MagicKey", false);
        _treasureSprites[5] = SpriteHelper.CreateSprite("Dream_Medallion", false);
        _treasureSprites[6] = SpriteHelper.CreateSprite("Lemms_Order", false);
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
    public static Dictionary<string, TreasureState> Artifacts { get; set; } = new()
    {
        {"silksongJournal", TreasureState.NoMap},
        {"silverSeal", TreasureState.NoMap},
        {"bronzeKingIdol", TreasureState.NoMap},
        {"goldenArcaneEgg", TreasureState.NoMap},
        {"magicKey", TreasureState.NoMap },
        {"dreamMedallion", TreasureState.NoMap }
    };

    #endregion

    #region Event handler

    private void ActivateGameObject_OnEnter(On.HutongGames.PlayMaker.Actions.ActivateGameObject.orig_OnEnter orig, ActivateGameObject self)
    {
        orig(self);
        if (string.Equals(self.Fsm.GameObjectName, "Shop Menu") && string.Equals(self.Fsm.Name, "shop_control")
            && string.Equals(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, "Ruins1_05b"))
        {
            TreasureState[] states = Artifacts.Values.Take(4).ToArray();
            int[] prices = new int[] { 300, 675, 1200, 1800 };
            for (int i = 1; i < 5; i++)
            {
                if (states[i - 1] == TreasureState.NoMap)
                    continue;
                else if (states[i - 1] == TreasureState.ObtainedMap)
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

                    relic.GetComponent<ShopItemStats>().cost = prices[i - 1];
                    relic.transform.Find("Item Sprite").GetComponent<SpriteRenderer>().sprite = _treasureSprites[i - 1];
                    relic.transform.Find("Item cost").GetComponent<TextMeshPro>().text = $"<color=#7FFF7B>{prices[i - 1]}</color>";
                }
            }
        }
    }

    private void EnemyDeathEffects_EmitEssence(ILContext il)
    {
        foreach (string key in Artifacts.Keys)
        {
            LoreMaster.Instance.Log("The key is: " + key);
        }

        if (Artifacts["dreamMedallion"] == TreasureState.NoMap)
            return;
        ILCursor cursor = new(il);

        cursor.Goto(0);
        if (cursor.TryGotoNext(MoveType.After,
            x => x.MatchCall(typeof(UnityEngine.Random), "Range")))
            cursor.EmitDelegate<Func<int, int>>(orig => LoreMaster.Instance.Generator.Next(0, 5) == 0 ? 0 : orig);
    }

    private void GetPlayerDataInt_OnEnter(On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.orig_OnEnter orig, GetPlayerDataInt self)
    {
        orig(self);
        if (Artifacts["magicKey"] != TreasureState.NoMap && string.Equals(self.intName, nameof(PlayerData.instance.simpleKeys)))
            self.storeValue.Value = 99;
    }

    private void SetPlayerDataBool_OnEnter(On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.orig_OnEnter orig, SetPlayerDataBool self)
    {
        orig(self);
        if(self.boolName.Value.Contains("noTrinket") && self.State.Name.Contains("Check Relic") && self.value.Value)
        {
            int trinketNumber = Convert.ToInt16(self.boolName.Value.Substring(9));
            PlayerData.instance.SetBool("noTrinket" + trinketNumber, Artifacts.Values.ToArray()[trinketNumber -1] == TreasureState.ObtainedMap);
        }
    }

    #endregion

    #region Control

    protected override void Enable()
    {
        On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.OnEnter += GetPlayerDataInt_OnEnter;
        On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter += ActivateGameObject_OnEnter;
        On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter += SetPlayerDataBool_OnEnter;
        IL.EnemyDeathEffects.EmitEssence += EnemyDeathEffects_EmitEssence;
    }

    protected override void Disable()
    {
        On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.OnEnter -= GetPlayerDataInt_OnEnter;
        On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter -= ActivateGameObject_OnEnter;
        On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter -= SetPlayerDataBool_OnEnter;
        IL.EnemyDeathEffects.EmitEssence -= EnemyDeathEffects_EmitEssence;
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
        GameObject charts = new("Charts");
        charts.transform.SetParent(treasureChartPage.transform);
        charts.transform.localScale = new(1f, 1f, 1f);
        charts.transform.localPosition = new(0f, 0f, 0f);

        GameObject chartImage = new("Active Chart");
        chartImage.transform.SetParent(treasureChartPage.transform);
        chartImage.transform.localScale = new(1f, 1f, 1f);
        chartImage.transform.position = new(-.1f, -2f, 0f);
        chartImage.layer = treasureChartPage.layer;
        chartImage.AddComponent<SpriteRenderer>();
        chartImage.GetComponent<SpriteRenderer>().sortingLayerID = 629535577;
        chartImage.GetComponent<SpriteRenderer>().sortingLayerName = "HUD";

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
            map.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite("Treasure_Chart");
            map.GetComponent<SpriteRenderer>().sortingLayerID = 629535577;
            map.GetComponent<SpriteRenderer>().sortingLayerName = "HUD";
            xPosition += 3f;
            if (xPosition > 9.1f)
            {
                xPosition = -9.1f;
                yPosition -= 1.6f;
            }
            _chartImages[i] = SpriteHelper.CreateSprite("Treasure_Chart_" + (i + 1), ".jpg");
            _mapItems[i] = map;
        }
        chartImage.GetComponent<SpriteRenderer>().sprite = _chartImages[0];

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
        currentWorkingState.AddLastAction(new Lambda(() =>
        {
            foreach (Transform child in charts.transform)
                child.gameObject.SetActive(true);
        }));

        // Create main state
        inventoryFsm.AddState(new FsmState(inventoryFsm.Fsm)
        {
            Name = "Charts",
            Actions = new FsmStateAction[]
            {
                    new Lambda(() => inventoryFsm.gameObject.LocateMyFSM("Update Cursor").FsmVariables.FindFsmGameObject("Item").Value = _mapItems[indexVariable.Value]),
                    new SetSpriteRendererOrder()
                    {
                        gameObject = new() { GameObject = inventoryFsm.FsmVariables.FindFsmGameObject("Cursor Glow") },
                        order = 0,
                        delay = 0f
                    },
                    new Lambda(() => inventoryFsm.gameObject.LocateMyFSM("Update Cursor").SendEvent("UPDATE CURSOR")),
                    new Lambda(() =>
                    {
                        SpriteRenderer spriteRenderer = chartImage.GetComponent<SpriteRenderer>();
                        spriteRenderer.sprite = HasCharts[indexVariable.Value] ? _chartImages[indexVariable.Value] : null;
                        spriteRenderer.color = Finder.GetLocation("Treasure_"+(indexVariable.Value + 1)).Placement.Items.All(x => x.IsObtained()) ? Color.grey : Color.white;
                    })
            }
        });

        // Add transition from init to main
        currentWorkingState.AddTransition("FINISHED", "Charts");
        AddInventoryMovement(inventoryFsm, indexVariable);
        treasureChartPage.SetActive(false);
    }

    private static void AddInventoryMovement(PlayMakerFSM fsm, FsmInt indexVariable)
    {
        FsmState currentWorkingState = fsm.GetState("Charts");

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
        currentWorkingState.AddTransition("FINISHED", "Charts");
        currentWorkingState.AddLastAction(new Lambda(() =>
        {
            if (indexVariable.Value == 0 || indexVariable.Value == 7)
            {
                indexVariable.Value = -2;
                fsm.SendEvent("OUT");
                return;
            }
            indexVariable.Value = indexVariable.Value == -1 ? 6 : indexVariable.Value - 1;
            fsm.SendEvent("FINISHED");
        }));
        fsm.GetState("R Arrow").AddTransition("UI LEFT", "Left Press");

        // Right
        currentWorkingState = fsm.GetState("Right Press");
        currentWorkingState.AddTransition("OUT", "R Arrow");
        currentWorkingState.AddTransition("FINISHED", "Charts");
        currentWorkingState.AddLastAction(new Lambda(() =>
        {
            if (indexVariable.Value == 6 || indexVariable.Value == 13)
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
        currentWorkingState.AddTransition("FINISHED", "Charts");
        currentWorkingState.AddLastAction(new Lambda(() =>
        {
            if (indexVariable.Value <= 6)
                indexVariable.Value += 7;
            else
                indexVariable.Value -= 7;
            fsm.SendEvent("FINISHED");
        }));

        // Down
        currentWorkingState = fsm.GetState("Down Press");
        currentWorkingState.AddTransition("FINISHED", "Charts");
        currentWorkingState.AddLastAction(new Lambda(() =>
        {
            if (indexVariable.Value >= 7)
                indexVariable.Value -= 7;
            else
                indexVariable.Value += 7;
            fsm.SendEvent("FINISHED");
        }));
    }

    private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (string.Equals(name, "hasTreasureCharts"))
            return HasCharts.Any(x => x) || CanPurchaseTreasureCharts || Artifacts.Any(x => x.Value != TreasureState.NoMap);
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
        }
        else if (Artifacts.ContainsKey(name))
            Artifacts[name] = TreasureState.ObtainedMap;
        else if (string.Equals(name, "lemm_Allow"))
            CanPurchaseTreasureCharts = orig;
        // Force lemm after 1 dreamer to be outside. (This is needed because if QoL skips cutscenes, the flag normally gets skipped)
        else if(orig && (string.Equals(name, "monomonDefeated") || string.Equals(name, "hegemolDefeated") || string.Equals(name, "lurienDefeated")))
            if (!PlayerData.instance.GetBool("monomonDefeated") && !PlayerData.instance.GetBool("hegemolDefeated") && !PlayerData.instance.GetBool("lurienDefeated"))
                PlayerData.instance.SetBool("marmOutside", true);
        
        return orig;
    }

    #endregion

    public static List<AbstractItem> GetTreasureItems()
    {
        List<AbstractItem> items = new();
        List<AbstractItem> shuffledItems = new()
        {
            Finder.GetItem(ItemNames.Wanderers_Journal),
            Finder.GetItem(ItemNames.Wanderers_Journal),
            Finder.GetItem(ItemNames.Wanderers_Journal),
            Finder.GetItem(ItemNames.Hallownest_Seal),
            Finder.GetItem(ItemNames.Hallownest_Seal),
            Finder.GetItem(ItemNames.Kings_Idol),
            Finder.GetItem(ItemNames.Kings_Idol),
            Finder.GetItem(ItemNames.Arcane_Egg),
            new BoolItem()
            {
                fieldName = "magicKey",
                name = "Magical_Key",
                setValue = true,
                UIDef = new MsgUIDef()
                {
                    name = new BoxedString("Magical Key"),
                    shopDesc = new BoxedString("The master key of this kingdom. Opens ALMOST every locked mechanism."),
                    sprite = new CustomSprite("MagicKey", false)
                }
            },
            new BoolItem()
            {
                fieldName = "dreamMedallion",
                name = "Dream_Medallion",
                setValue = true,
                UIDef = new MsgUIDef()
                {
                    name = new BoxedString("Dream Medallion"),
                    shopDesc = new BoxedString("An old artifact from the moth tribe. They say, the wielder of this medallion attracts the essence of dreams."),
                    sprite = new CustomSprite("Dream_Medallion", false)
                }
            },
            new BoolItem()
            {
                fieldName = "silksongJournal",
                name = "Silksong_Journal",
                setValue = true,
                UIDef = new MsgUIDef()
                {
                    name = new BoxedString("Silksong Journal?"),
                    shopDesc = new BoxedString("A very special journal which was buried in this kingdom. The only thing on this, that Lemm could decifer, was the text \"Silksong Release Date\"."),
                    sprite = new CustomSprite("Silksong_Journal", false)
                }
            },
            new BoolItem()
            {
                fieldName = "silverSeal",
                name = "Silver_Hallownest_Seal",
                setValue = true,
                UIDef = new MsgUIDef()
                {
                    name = new BoxedString("Silver Seal"),
                    shopDesc = new BoxedString("A very special Hallownest seal which was buried in this kingdom. I personally like the colored design more."),
                    sprite = new CustomSprite("Silver_Seal", false)
                }
            },
            new BoolItem()
            {
                fieldName = "bronzeKingIdol",
                name = "Bronze_King_Idol",
                setValue = true,
                UIDef = new MsgUIDef()
                {
                    name = new BoxedString("Bronze King's Idol"),
                    shopDesc = new BoxedString("A very special King's Idol which was buried in this kingdom. If this color is caused by nature or artifical is beyond me."),
                    sprite = new CustomSprite("Bronze_King_Idol",false)
                }
            },
            new BoolItem()
            {
                fieldName = "goldenArcaneEgg",
                name = "Golden_Arcane_Egg",
                setValue = true,
                UIDef = new MsgUIDef()
                {
                    name = new BoxedString("Golden Arcane Egg"),
                    shopDesc = new BoxedString("A very special Arcane egg which was buried in this kingdom. Since it is one of a kind, I'm curious what kind of information it stored."),
                    sprite = new CustomSprite("Golden_Egg", false)
                }
            }
        };
        
        // Randomize the order of items.
        do
        {
            int rolledIndex = LoreMaster.Instance.Generator.Next(0, shuffledItems.Count);
            items.Add(shuffledItems[rolledIndex]);
            shuffledItems.RemoveAt(rolledIndex);
        } 
        while (shuffledItems.Any());

        return items;
    }
}
