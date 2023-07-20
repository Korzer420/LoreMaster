using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.LorePowers;
using LoreMaster.LorePowers.CityOfTears;
using LoreMaster.LorePowers.FungalWastes;
using LoreMaster.LorePowers.HowlingCliffs;
using LoreMaster.Manager;
using SFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LoreMaster;

internal class LorePage
{
    #region Members

    private static List<Power> _powers = new();

    private static Sprite _emptySprite;

    private static Sprite _notActive;

    private static GameObject[] _loreObjects;

    private static SpriteRenderer _stagEgg;

    private static Dictionary<Area, Sprite> _sprites = new();

    private static Dictionary<string, GameObject> _controlElements = new();

    private static string _lastState;

    private static int _selectedEffect = 0;

    private static GameObject _totalLore;

    #endregion

    #region Constructors

    static LorePage()
    {
        _emptySprite = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Backboards/BB 3").GetComponent<SpriteRenderer>().sprite;
        _notActive = SpriteHelper.CreateSprite<LoreMaster>("Base.DisabledPower");
        for (int i = 1; i < 16; i++)
            _sprites.Add((Area)i, SpriteHelper.CreateSprite<LoreMaster>($"Base.Tablets.{(Area)i}"));
    }

    #endregion

    #region Methods

    /// <summary>
    /// Passes the power to the inventory page.
    /// </summary>
    internal static void PassPowers(List<Power> powers)
        => _powers = powers;

    /// <summary>
    /// Updates the inventory page according to the acquired and currently active powers.
    /// </summary>
    /// <returns></returns>
    internal static bool UpdateLorePage()
    {
        try
        {
            for (int i = 0; i < _loreObjects.Length; i++)
            {
                //if (_loreObjects[i] == null)
                //{
                //    LoreMaster.Instance.LogDebug("Lore Object " + i + " doesn't exist");
                //    continue;
                //}

                //if (_powers[i].State == PowerState.Active)
                //{
                //    _loreObjects[i].GetComponentInChildren<SpriteRenderer>().sprite = _sprites[_powers[i].Location];
                //    _loreObjects[i].GetComponentInChildren<SpriteRenderer>().color = Color.white;
                //    _loreObjects[i].transform.eulerAngles = new Vector3(0f, 0f, 0f);
                //}
                //else if (_powers[i].State == PowerState.Twisted)
                //{
                //    _loreObjects[i].GetComponentInChildren<SpriteRenderer>().sprite = _sprites[_powers[i].Location];
                //    _loreObjects[i].GetComponentInChildren<SpriteRenderer>().color = new(1f, 0f, 1f);
                //    _loreObjects[i].transform.eulerAngles = new Vector3(0f, 0f, 180f);
                //}
                //else if (PowerManager.HasObtainedPower)
                //{
                //    _loreObjects[i].GetComponentInChildren<SpriteRenderer>().sprite = _notActive;
                //    _loreObjects[i].GetComponentInChildren<SpriteRenderer>().color = Color.red;
                //    _loreObjects[i].transform.eulerAngles = new Vector3(0f, 0f, 0f);
                //}
                //else
                //{
                //    _loreObjects[i].GetComponentInChildren<SpriteRenderer>().sprite = _emptySprite;
                //    _loreObjects[i].GetComponentInChildren<SpriteRenderer>().color = Color.white;
                //    _loreObjects[i].transform.eulerAngles = new Vector3(0f, 0f, 0f);
                //}
            }
            _stagEgg.sprite = StagAdoptionPower.Instance.CanSpawnStag
                ? StagAdoptionPower.Instance.InventorySprites[0]
                : StagAdoptionPower.Instance.InventorySprites[1];
            //_controlElements["Joker"].SetActive(LoreManager.JokerScrolls >= 0);
            //_controlElements["Joker"].GetComponentInChildren<TextMeshPro>().text = LoreManager.JokerScrolls.ToString();
            //_controlElements["Cleanse"].SetActive(LoreManager.CleansingScrolls >= 0);
            //_controlElements["Cleanse"].GetComponentInChildren<TextMeshPro>().text = LoreManager.CleansingScrolls.ToString();

            //_totalLore.SetActive(true);
            //_totalLore.GetComponent<TextMeshPro>().text = "Total Lore amount: " + PowerManager.ObtainedPowers.Count;

        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error when updating inventory: " + exception.Message);
        }
        return true;
    }

    /// <summary>
    /// Generates the inventory page.
    /// </summary>
    internal static void GeneratePage(GameObject lorePage)
    {
        try
        {
            // Prevent multiple calls (The only element that should already been here is the cursor)
            if (lorePage.transform.childCount > 1)
                return;
            _controlElements.Clear();
            PlayMakerFSM fsm = lorePage.LocateMyFSM("Empty UI");

            // Add index variable
            List<FsmInt> intVariables = fsm.FsmVariables.IntVariables.ToList();
            FsmInt indexVariable = new() { Name = "ItemIndex", Value = 0 };
            intVariables.Add(indexVariable);
            fsm.FsmVariables.IntVariables = intVariables.ToArray();
            
            // Generates the power holder
            GameObject powerList = new("Power List");
            powerList.transform.SetParent(lorePage.transform);
            powerList.transform.localPosition = new(0f, 0f, 0f);
            powerList.transform.localScale = new(1f, 1f, 1f);

            GameObject powerTitle = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Name").gameObject);
            powerTitle.transform.SetParent(lorePage.transform);
            powerTitle.transform.localPosition = new(13f, -7.5f, -2f);
            powerTitle.GetComponent<TextMeshPro>().text = "";
            _controlElements.Add("powerTitle", powerTitle);

            GameObject powerDescription = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Desc").gameObject);
            powerDescription.transform.SetParent(lorePage.transform);
            powerDescription.transform.localPosition = new(13f, -9f, 1f);
            powerDescription.GetComponent<TextMeshPro>().text = "";
            _controlElements.Add("powerDescription", powerDescription);

            GameObject confirmButton = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Confirm Action").gameObject);
            confirmButton.transform.SetParent(lorePage.transform);
            UnityEngine.Object.Destroy(confirmButton.GetComponent<PlayMakerFSM>());
            confirmButton.transform.localPosition = new(3.72f, -3.36f, -30.13f);
            confirmButton.transform.Find("Text").GetComponent<TextMeshPro>().text = "Toggle Power";
            confirmButton.SetActive(false);
            _controlElements.Add("confirmButton", confirmButton);

            // Generates all power objects
            float xPosition = -11f; // in 1,5f steps
            float yPosition = 4.6f; // in -2f steps
            _loreObjects = new GameObject[_powers.Count];
            for (int i = 1; i <= _powers.Count; i++)
            {
                GameObject tablet = new("Power " + i);
                tablet.transform.SetParent(powerList.transform);
                tablet.transform.localScale = new Vector3(.9f, .9f, 1f);
                tablet.transform.position = new Vector3(xPosition, yPosition, -3f);
                xPosition += 1.5f;
                if (xPosition > 2.5f)
                {
                    xPosition = -11;
                    yPosition -= 2f;
                }
                tablet.layer = lorePage.layer;
                // The cursor need a collider to jump to
                tablet.AddComponent<BoxCollider2D>().offset = new(0f, 0f);
                GameObject child = new("Image");
                child.transform.SetParent(tablet.transform);
                child.transform.localPosition = new(0f, 0f, 0f);
                child.transform.localScale = new(1f, 1f, 1f);
                child.layer = lorePage.layer;
                child.AddComponent<SpriteRenderer>().sprite = _emptySprite;
                child.GetComponent<SpriteRenderer>().sortingLayerID = 629535577;
                child.GetComponent<SpriteRenderer>().sortingLayerName = "HUD";
                _loreObjects[i - 1] = tablet;
            }

            // Removing the jump from arrow button to arrow button.
            fsm.GetState("L Arrow").RemoveTransitionsTo("R Arrow");
            fsm.GetState("R Arrow").RemoveTransitionsTo("L Arrow");

            FsmState currentWorkingState = fsm.GetState("Init Heart Piece");
            currentWorkingState.Name = "Init Lore";
            currentWorkingState.RemoveTransitionsTo("L Arrow");
            currentWorkingState.AddLastAction(new Lambda(() =>
            {
                int runs = 0;
                foreach (Transform child in lorePage.transform)
                {
                    child.gameObject.SetActive(true);
                    runs++;
                    if (runs == 5)
                        break;
                }
            }));

            // Create main state
            fsm.AddState(new FsmState(fsm.Fsm)
            {
                Name = "Powers",
                Actions = new FsmStateAction[]
                {
                    new Lambda(() => fsm.gameObject.LocateMyFSM("Update Cursor").FsmVariables.FindFsmGameObject("Item").Value = _loreObjects[indexVariable.Value]),
                    new SetSpriteRendererOrder()
                    {
                        gameObject = new() { GameObject = fsm.FsmVariables.FindFsmGameObject("Cursor Glow") },
                        order = 0,
                        delay = 0f
                    },
                    new Lambda(() => fsm.gameObject.LocateMyFSM("Update Cursor").SendEvent("UPDATE CURSOR")),
                    new Lambda(() =>
                    {

                    })
                }
            });

            // Add transition from init to main
            currentWorkingState.AddTransition("FINISHED", "Powers");

            currentWorkingState = fsm.GetState("Powers");

            fsm.AddState(new FsmState(fsm.Fsm) { Name = "Up Press" });
            fsm.AddState(new FsmState(fsm.Fsm) { Name = "Right Press" });
            fsm.AddState(new FsmState(fsm.Fsm) { Name = "Down Press" });
            fsm.AddState(new FsmState(fsm.Fsm) { Name = "Left Press" });
            fsm.AddState(new FsmState(fsm.Fsm) { Name = "Toggle Power" });

            currentWorkingState.AddTransition("UI UP", "Up Press");
            currentWorkingState.AddTransition("UI RIGHT", "Right Press");
            currentWorkingState.AddTransition("UI DOWN", "Down Press");
            currentWorkingState.AddTransition("UI LEFT", "Left Press");
            currentWorkingState.AddTransition("UI CONFIRM", "Toggle Power");

            // Left
            currentWorkingState = fsm.GetState("Left Press");
            currentWorkingState.AddTransition("OUT", "L Arrow");
            currentWorkingState.AddTransition("FINISHED", "Powers");
            currentWorkingState.AddLastAction(new Lambda(() =>
            {
                if (indexVariable.Value == 0 || indexVariable.Value % 10 == 0)
                {
                    indexVariable.Value = -2;
                    fsm.SendEvent("OUT");
                    return;
                }
                indexVariable.Value = indexVariable.Value == -1 ? 9 : indexVariable.Value - 1;
                fsm.SendEvent("FINISHED");
            }));
            fsm.GetState("R Arrow").AddTransition("UI LEFT", "Left Press");

            // Right
            currentWorkingState = fsm.GetState("Right Press");
            currentWorkingState.AddTransition("OUT", "R Arrow");
            currentWorkingState.AddTransition("FINISHED", "Powers");
            currentWorkingState.AddLastAction(new Lambda(() =>
            {
                if (indexVariable.Value == 9 || indexVariable.Value % 10 == 9)
                {
                    fsm.SendEvent(indexVariable.Value == 59 ? "STAG" : "OUT");
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
            currentWorkingState.AddTransition("FINISHED", "Powers");
            currentWorkingState.AddLastAction(new Lambda(() =>
            {
                if (indexVariable.Value < 10)
                    indexVariable.Value += 50;
                else
                    indexVariable.Value -= 10;
                fsm.SendEvent("FINISHED");
            }));

            // Down
            currentWorkingState = fsm.GetState("Down Press");
            currentWorkingState.AddTransition("FINISHED", "Powers");
            currentWorkingState.AddLastAction(new Lambda(() =>
            {
                if (indexVariable.Value >= 50)
                    indexVariable.Value -= 50;
                else
                    indexVariable.Value += 10;
                fsm.SendEvent("FINISHED");
            }));

            // Toggle
            currentWorkingState = fsm.GetState("Toggle Power");
            currentWorkingState.AddTransition("FINISHED", "Powers");
            currentWorkingState.AddLastAction(new Lambda(() =>
            {

            }));

            _totalLore = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Name").gameObject);
            _totalLore.transform.SetParent(lorePage.transform);
            _totalLore.transform.position = new(-9.5106f, 0.9982f, 0f);
            _totalLore.GetComponent<TextMeshPro>().text = "Total Lore Amount: 0";
            _totalLore.GetComponent<TextMeshPro>().fontSize = 2;

            BuildExtraItems(lorePage);

            lorePage.SetActive(false);
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("An error occured in the inventory: " + exception.Message);
            LoreMaster.Instance.LogError("An error occured in the inventory: " + exception.StackTrace);
        }
    }

    private static void BuildExtraItems(GameObject lorePage)
    {
        PlayMakerFSM fsm = lorePage.LocateMyFSM("Empty UI");
        // Stag egg
        GameObject stagEgg = new("Stag Egg");
        stagEgg.transform.SetParent(lorePage.transform);
        stagEgg.transform.localScale = new(1f, 1f, 1f);
        stagEgg.transform.position = new(4.0254f, -5.3418f, -3f);
        stagEgg.layer = lorePage.layer;
        _stagEgg = stagEgg.AddComponent<SpriteRenderer>();
        stagEgg.AddComponent<BoxCollider2D>().offset = new(0f, 0f);
        _stagEgg.sortingLayerID = 629535577;
        _stagEgg.sortingLayerName = "HUD";
        FsmState currentWorkingState = new(fsm.Fsm)
        {
            Name = "Stag Egg",
            Actions = new FsmStateAction[]
            {
                    new Lambda(() => fsm.gameObject.LocateMyFSM("Update Cursor").FsmVariables.FindFsmGameObject("Item").Value = stagEgg),
                    new SetSpriteRendererOrder()
                    {
                        gameObject = new() { GameObject = fsm.FsmVariables.FindFsmGameObject("Cursor Glow") },
                        order = 0,
                        delay = 0f
                    },
                    new Lambda(() => fsm.gameObject.LocateMyFSM("Update Cursor").SendEvent("UPDATE CURSOR")),
                    new Lambda(() =>
                    {

                        if(!stagEgg.activeSelf)
                        {
                            _controlElements["confirmButton"].SetActive(false);
                            fsm.SendEvent(_lastState == "Select Joker Scroll" ? "UI LEFT" : "UI RIGHT");
                            _lastState = "Stag Egg";
                            return;
                        }
                        _lastState = "Stag Egg";
                        _controlElements["powerTitle"].GetComponent<TextMeshPro>().text = StagAdoptionPower.Instance.CanSpawnStag ? "Stag Egg" : "Broken Stag Egg";
                        _controlElements["powerDescription"].GetComponent<TextMeshPro>().text = StagAdoptionPower.Instance.CanSpawnStag
                        ? "You can feel something moving in there... maybe tapping it does the trick?"
                        : "The empty shell of a stag. They probably now live a happier life... somewhere.";
                        _controlElements["confirmButton"].SetActive(StagAdoptionPower.Instance.CanSpawnStag);
                    })
            }
        };
        currentWorkingState.AddTransition("UI LEFT", "Left Press");
        fsm.AddState(currentWorkingState);
        currentWorkingState = new FsmState(fsm.Fsm)
        {
            Name = "Spawn",
            Actions = new FsmStateAction[]
            {
                new Lambda(() =>
                {
                    if(!StagAdoptionPower.Instance.CanSpawnStag)
                        fsm.SendEvent("FINISHED");
                    StagAdoptionPower.Instance.SpawnStag();
                    _stagEgg.sprite = StagAdoptionPower.Instance.InventorySprites[1];
                    _controlElements["powerTitle"].GetComponent<TextMeshPro>().text = "Broken Stag Egg";
                    _controlElements["powerDescription"].GetComponent<TextMeshPro>().text = "The empty shell of a stag. They probably now live a happier life... somewhere.";
                })
            }
        };
        fsm.AddState(currentWorkingState);
        fsm.GetState("Stag Egg").AddTransition("UI CONFIRM", currentWorkingState);
        currentWorkingState.AddTransition("FINISHED", "Stag Egg");
        fsm.GetState("Right Press").AddTransition("STAG", "Stag Egg");
        stagEgg.SetActive(false);

        GameObject jokerScroll = new("Joker Scrolls");
        jokerScroll.transform.SetParent(lorePage.transform);
        jokerScroll.transform.localScale = new(1.2f, 1.2f, 1f);
        jokerScroll.transform.position = new(6.0254f, -5.4418f, -3f);
        jokerScroll.layer = lorePage.layer;
        jokerScroll.AddComponent<BoxCollider2D>().offset = new(0f, 0f);
        SpriteRenderer spriteRenderer = jokerScroll.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerID = 629535577;
        spriteRenderer.sortingLayerName = "HUD";
        spriteRenderer.sprite = SpriteHelper.CreateSprite<LoreMaster>("Base.SummoningScroll");
        GameObject jokerAmount = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Desc").gameObject);
        jokerAmount.transform.SetParent(jokerScroll.transform);
        jokerAmount.transform.localScale = new(1.2f, 1.2f, 1);
        jokerAmount.transform.localPosition = new(5.7527f, -5.8764f, -6f);
        jokerAmount.GetComponent<TextMeshPro>().text = "0";
        jokerAmount.SetActive(true);
        jokerScroll.SetActive(false);
        _controlElements.Add("Joker", jokerScroll);

        GameObject cleanseScroll = new("Cleanse Scrolls");
        cleanseScroll.transform.SetParent(lorePage.transform);
        cleanseScroll.transform.localScale = new(1.2f, 1.2f, 1f);
        cleanseScroll.transform.position = new(8.0254f, -5.4418f, -3f);
        cleanseScroll.layer = lorePage.layer;
        cleanseScroll.AddComponent<BoxCollider2D>().offset = new(0f, 0f);
        spriteRenderer = cleanseScroll.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerID = 629535577;
        spriteRenderer.sortingLayerName = "HUD";
        spriteRenderer.sprite = SpriteHelper.CreateSprite<LoreMaster>("Base.CurseDispell");
        GameObject cleanseAmount = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Desc").gameObject);
        cleanseAmount.transform.SetParent(cleanseScroll.transform);
        cleanseAmount.transform.localScale = new(1.2f, 1.2f, 1);
        cleanseAmount.transform.localPosition = new(5.7527f, -5.8764f, -6f);
        cleanseAmount.GetComponent<TextMeshPro>().text = "0";
        cleanseAmount.SetActive(true);
        cleanseScroll.SetActive(false);
        _controlElements.Add("Cleanse", cleanseScroll);

        GameObject cursor = lorePage.FindChild("Cursor");
        _controlElements.Add("Cursor", cursor);
        GameObject interactSprite = new("Interact Option");
        interactSprite.transform.SetParent(cursor.transform);
        interactSprite.transform.localPosition = new(0f, 0f, -6f);
        interactSprite.layer = cursor.layer;
        interactSprite.AddComponent<SpriteRenderer>().sortingLayerID = 62935577;
        interactSprite.GetComponent<SpriteRenderer>().sortingLayerName = "HUD";
        interactSprite.SetActive(true);

        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Select Joker Scroll",
            Actions = new FsmStateAction[]
            {
                new Lambda(() => fsm.gameObject.LocateMyFSM("Update Cursor").FsmVariables.FindFsmGameObject("Item").Value = jokerScroll),
                new SetSpriteRendererOrder()
                {
                    gameObject = new() { GameObject = fsm.FsmVariables.FindFsmGameObject("Cursor Glow") },
                    order = 0,
                    delay = 0f
                },
                new Lambda(() => fsm.gameObject.LocateMyFSM("Update Cursor").SendEvent("UPDATE CURSOR")),
                new Lambda(() =>
                {
                    if (!jokerScroll.activeSelf)
                    {
                        fsm.SendEvent(_lastState == "Stag Egg" ? "UI RIGHT" : "UI LEFT");
                        _lastState = "Select Joker Scroll";
                        return;
                    }
                    _lastState = "Select Joker Scroll";
                    _controlElements["powerTitle"].GetComponent<TextMeshPro>().text = "Knowledge Scroll";
                    _controlElements["powerDescription"].GetComponent<TextMeshPro>().text = "A cryptic scroll written by Elderbug. Apparently this can be used once to obtain a power of your choice.";
                    //_controlElements["confirmButton"].SetActive(LoreManager.JokerScrolls > 0);
                })
            }
        });
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Toggle Joker Scroll",
            Actions = new FsmStateAction[]
            {
                new Lambda(() =>
                {
                    //if (LoreManager.JokerScrolls >= 1)
                    //{
                    //    if (_selectedEffect == 1)
                    //    {
                    //        _selectedEffect = 0;
                    //        interactSprite.GetComponent<SpriteRenderer>().sprite = null;
                    //    }
                    //    else
                    //    {
                    //        _selectedEffect = 1;
                    //        interactSprite.GetComponent<SpriteRenderer>().sprite = jokerScroll.GetComponent<SpriteRenderer>().sprite;
                    //    }
                    //}
                    fsm.SendEvent("FINISHED");
                })
            }
        });
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Select Cleanse Scroll",
            Actions = new FsmStateAction[]
            {
                new Lambda(() => fsm.gameObject.LocateMyFSM("Update Cursor").FsmVariables.FindFsmGameObject("Item").Value = cleanseScroll),
                new SetSpriteRendererOrder()
                {
                    gameObject = new() { GameObject = fsm.FsmVariables.FindFsmGameObject("Cursor Glow") },
                    order = 0,
                    delay = 0f
                },
                new Lambda(() => fsm.gameObject.LocateMyFSM("Update Cursor").SendEvent("UPDATE CURSOR")),
                new Lambda(() =>
                {
                    _lastState = "Select Cleanse Scroll";
                    if (!cleanseScroll.activeSelf)
                    {
                        fsm.SendEvent("UI RIGHT");
                        return;
                    }
                    _controlElements["powerTitle"].GetComponent<TextMeshPro>().text = "Cleansing Scroll";
                    _controlElements["powerDescription"].GetComponent<TextMeshPro>().text = "A mysterious scroll written by Elderbug. If he's right, you can undo a curse spoken by acquired knowledge from you once.";
                    //_controlElements["confirmButton"].SetActive(LoreManager.CleansingScrolls > 0);
                })
            }
        });
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Toggle Cleanse Scroll",
            Actions = new FsmStateAction[]
            {
                new Lambda(() =>
                {
                    //if (LoreManager.CleansingScrolls >= 1)
                    //{
                    //    if (_selectedEffect == 2)
                    //    {
                    //        _selectedEffect = 0;
                    //        interactSprite.GetComponent<SpriteRenderer>().sprite = null;
                    //    }
                    //    else
                    //    {
                    //        _selectedEffect = 2;
                    //        interactSprite.GetComponent<SpriteRenderer>().sprite = cleanseScroll.GetComponent<SpriteRenderer>().sprite;
                    //    }
                    //}
                    fsm.SendEvent("FINISHED");
                })
            }
        });
        fsm.GetState("Stag Egg").AddTransition("UI RIGHT", "Select Joker Scroll");
        fsm.GetState("Select Joker Scroll").AddTransition("UI LEFT", "Stag Egg");
        fsm.GetState("Select Joker Scroll").AddTransition("UI CONFIRM", "Toggle Joker Scroll");
        fsm.GetState("Select Joker Scroll").AddTransition("UI RIGHT", "Select Cleanse Scroll");
        fsm.GetState("Toggle Joker Scroll").AddTransition("FINISHED", "Select Joker Scroll");
        fsm.GetState("Select Cleanse Scroll").AddTransition("UI LEFT", "Select Joker Scroll");
        fsm.GetState("Select Cleanse Scroll").AddTransition("UI CONFIRM", "Toggle Cleanse Scroll");
        fsm.GetState("Select Cleanse Scroll").AddTransition("UI RIGHT", "R Arrow");
        fsm.GetState("Toggle Cleanse Scroll").AddTransition("FINISHED", "Select Cleanse Scroll");

        fsm.GetState("Move Pane R").AddFirstAction(new Lambda(() =>
        {
            _selectedEffect = 0;
            interactSprite.GetComponent<SpriteRenderer>().sprite = null;
        }));

        fsm.GetState("Move Pane L").AddFirstAction(new Lambda(() =>
        {
            _selectedEffect = 0;
            interactSprite.GetComponent<SpriteRenderer>().sprite = null;
        }));
    }

    internal static void ActivateStagEgg() => _stagEgg.gameObject.SetActive(true);

    #endregion
}
