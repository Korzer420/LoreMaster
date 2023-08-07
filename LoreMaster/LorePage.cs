using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.FsmStateActions;
using KorzUtils.Data;
using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.ItemChangerData;
using LoreMaster.LorePowers;
using LoreMaster.LorePowers.HowlingCliffs;
using LoreMaster.Manager;
using LoreMaster.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LoreMaster;

internal static class LorePage
{
    #region Members

    private static Sprite _emptySprite;

    private static GameObject[] _glyphObjects;

    private static SpriteRenderer _stagEgg;

    private static Dictionary<Area, Sprite> _sprites = new();

    private static Dictionary<string, GameObject> _controlElements = new();

    private static string _lastState;

    private static (Vector3, PowerRank)[] _glyphPositions = new (Vector3, PowerRank)[]
    {
        // --First "line"--
        (new(-6.25f, 3.8f, -3), PowerRank.Lower),
        (new(-3.45f, 5, -3), PowerRank.Lower),
        (new(0, 4.55f, -3f), PowerRank.Greater),
        (new(3.45f, 5, -3), PowerRank.Lower),
        (new(6.25f, 3.8f, -3), PowerRank.Lower),
        // --Second "line"--
        (new(-3.275f, 2.5f, -3), PowerRank.Medium),
        (new(3.275f, 2.5f, -3), PowerRank.Medium),
        // --Third "line"--
        (new(-5.45f, 1, -3), PowerRank.Lower),
        (new(-3.275f, -0.4f, -3), PowerRank.Medium),
        (new(3.275f, -0.4f, -3), PowerRank.Medium),
        (new(5.45f, 1, -3), PowerRank.Lower),
        // --Fourth "Line"--
        (new(-6.55f, -2.45f, -3), PowerRank.Greater),
        (new(-3.3f, -3.2f, -3), PowerRank.Lower),
        (new(0, -3.1f, -3), PowerRank.Medium),
        (new(3.3f, -3.2f, -3), PowerRank.Lower),
        (new(6.55f, -2.45f, -3), PowerRank.Greater),
        // --Fifth "Line"--
        (new(-5, -5.3f, -3), PowerRank.Permanent),
        (new(-2.5f, -5.3f, -3), PowerRank.Permanent),
        (new(0, -5.3f, -3), PowerRank.Permanent),
        (new(2.5f, -5.3f, -3), PowerRank.Permanent),
        (new(5, -5.3f, -3), PowerRank.Permanent),
    };

    #endregion

    #region Constructors

    static LorePage()
    {
        _emptySprite = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Backboards/BB 3").GetComponent<SpriteRenderer>().sprite;
        for (int i = 1; i < 16; i++)
            _sprites.Add((Area)i, SpriteHelper.CreateSprite<LoreCore.LoreCore>($"Sprites.Tablets.{(Area)i}"));
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates the inventory page according to the acquired and currently active powers.
    /// </summary>
    /// <returns></returns>
    internal static bool UpdateLorePage()
    {
        try
        {
            for (int i = 0; i < _glyphObjects.Length; i++)
            {
                (int, PowerRank) data = GetMatchingIndex(i);
                SpriteRenderer spriteRenderer = _glyphObjects[i].GetComponentInChildren<SpriteRenderer>();
                if (LoreManager.Module.IsIndexAvailable(data))
                {
                    Power power = PowerManager.GetPowerInSlot(data);
                    spriteRenderer.sprite = power == null ? _emptySprite : _sprites[power.Location];
                }
                else
                    spriteRenderer.sprite = null;
            }
            _stagEgg.sprite = StagAdoptionPower.Instance.CanSpawnStag
                ? StagAdoptionPower.Instance.InventorySprites[0]
                : StagAdoptionPower.Instance.InventorySprites[1];

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

            // Add index variable for glyph slot
            fsm.AddVariable("ItemIndex", 0);
            FsmInt indexVariable = fsm.FsmVariables.FindFsmInt("ItemIndex");
            // Add variable for checking which transition entered the check state
            fsm.AddVariable("sourceStateId", 0);
            FsmInt enteredIndex = fsm.FsmVariables.FindFsmInt("sourceStateId");

            // Generates the power holder
            GameObject powerList = new("Power List");
            powerList.transform.SetParent(lorePage.transform);
            powerList.transform.localPosition = new(0f, 0f, 0f);
            powerList.transform.localScale = new(1f, 1f, 1f);

            GameObject powerTitle = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Name").gameObject);
            powerTitle.transform.SetParent(lorePage.transform);
            powerTitle.transform.position = new(10.95f, 0.05f, 0.3f);
            powerTitle.GetComponent<TextMeshPro>().text = "";
            powerTitle.GetComponent<TextMeshPro>().fontSize = 5;
            _controlElements.Add("powerTitle", powerTitle);

            GameObject powerDescription = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Desc").gameObject);
            powerDescription.transform.SetParent(lorePage.transform);
            powerDescription.transform.position = new(10.8973f, -1.05f, 3.3f);
            powerDescription.GetComponent<TextMeshPro>().text = "";
            powerDescription.GetComponent<TextMeshPro>().fontSize = 3;
            powerDescription.GetComponent<TextContainer>().size = new(5f, 20f);
            _controlElements.Add("powerDescription", powerDescription);

            GameObject confirmButton = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Confirm Action").gameObject);
            confirmButton.transform.SetParent(lorePage.transform);
            UnityEngine.Object.Destroy(confirmButton.GetComponent<PlayMakerFSM>());
            confirmButton.transform.localPosition = new(3.72f, -3.36f, -30.13f);
            confirmButton.transform.Find("Text").GetComponent<TextMeshPro>().text = "Toggle Power";
            confirmButton.SetActive(false);
            _controlElements.Add("confirmButton", confirmButton);

            // Generates all power objects
            _glyphObjects = new GameObject[21];
            for (int i = 1; i <= 21; i++)
            {
                GameObject tablet = new("Glyph Slot " + i);
                tablet.transform.SetParent(powerList.transform);
                if (_glyphPositions[i - 1].Item2 == PowerRank.Greater)
                    tablet.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                else if (_glyphPositions[i - 1].Item2 == PowerRank.Lower)
                    tablet.transform.localScale = new Vector3(1f, 1f, 1f);
                else
                    tablet.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

                tablet.transform.position = _glyphPositions[i - 1].Item1;
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
                _glyphObjects[i - 1] = tablet;
            }

            // Removing the jump from arrow button to arrow button.
            fsm.GetState("L Arrow").RemoveTransitionsTo("R Arrow");
            fsm.GetState("R Arrow").RemoveTransitionsTo("L Arrow");

            FsmState currentWorkingState = fsm.GetState("Init Heart Piece");
            currentWorkingState.Name = "Init Lore";
            currentWorkingState.RemoveTransitionsTo("L Arrow");
            currentWorkingState.AddActions(() =>
            {
                int runs = 0;
                foreach (Transform child in lorePage.transform)
                {
                    child.gameObject.SetActive(true);
                    runs++;
                    if (runs == 5)
                        break;
                }
            });

            // Setup main state
            fsm.AddState("Powers", new List<FsmStateAction>()
            {
                FsmHelper.WrapAction(() => LogHelper.Write("Called main state with: "+indexVariable.Value)),
                FsmHelper.WrapAction(() => fsm.gameObject.LocateMyFSM("Update Cursor").FsmVariables.FindFsmGameObject("Item").Value = _glyphObjects[indexVariable.Value]),
                new SetSpriteRendererOrder()
                {
                    gameObject = new() { GameObject = fsm.FsmVariables.FindFsmGameObject("Cursor Glow") },
                    order = 0,
                    delay = 0f
                },
                FsmHelper.WrapAction(() => fsm.gameObject.LocateMyFSM("Update Cursor").SendEvent("UPDATE CURSOR")),
                FsmHelper.WrapAction(() =>
                {
                    int selectedIndex = indexVariable.Value;
                    if (selectedIndex >= 0 && selectedIndex < 21)
                    {
                        (int, PowerRank) selectedPowerSlot = GetMatchingIndex(selectedIndex);
                        Power selectedPower = PowerManager.GetPowerInSlot(selectedPowerSlot);
                        string titleText;
                        string descriptionText;
                        if (selectedPower != null)
                        {
                            titleText = selectedPower.PowerName;
                            descriptionText = LoreManager.GlobalSaveData.ShowHint ? selectedPower.Hint : selectedPower.Description;
                        }
                        else
                        {
                            titleText = selectedPowerSlot.Item2 switch
                            {
                                PowerRank.Greater => AdditionalText.Greater_Glyph_Title,
                                PowerRank.Medium => AdditionalText.Medium_Glyph_Title,
                                PowerRank.Lower => AdditionalText.Lesser_Glyph_Title,
                                _ => AdditionalText.Permanent_Glyph_Title
                            };
                            descriptionText = selectedPowerSlot.Item2 switch
                            {
                                PowerRank.Greater => AdditionalText.Greater_Glyph_Description,
                                PowerRank.Medium => AdditionalText.Medium_Glyph_Description,
                                PowerRank.Lower => AdditionalText.Lesser_Glyph_Description,
                                _ => AdditionalText.Permanent_Glyph_Description
                            };
                        }
                        powerTitle.GetComponent<TextMeshPro>().text = titleText;
                        powerDescription.GetComponent<TextMeshPro>().text = descriptionText;
                    }
                })
            }, null);
            currentWorkingState.AddTransition("FINISHED", "Powers");

            // Setup state to skip unreachable items.
            fsm.AddState("Repeat?", () =>
            {
                if (indexVariable.Value < 0 || indexVariable.Value >= 16)
                    fsm.SendEvent("FINISHED");
                else if (!LoreManager.Module.IsIndexAvailable(GetMatchingIndex(indexVariable.Value)))
                    fsm.SendEvent(enteredIndex.Value switch
                    {
                        0 => "REPEAT UP",
                        1 => "REPEAT RIGHT",
                        2 => "REPEAT DOWN",
                        _ => "REPEAT LEFT"
                    });
                else
                    fsm.SendEvent("FINISHED");
            }, FsmTransitionData.FromTargetState("Powers").WithEventName("FINISHED"));

            // Up handling
            fsm.AddState("Up Press", () =>
            {
                enteredIndex.Value = 0;
                if (indexVariable.Value <= 4)
                    indexVariable.Value += 16;
                else if (indexVariable.Value == 5)
                    indexVariable.Value = 1;
                else if (indexVariable.Value == 6)
                    indexVariable.Value = 3;
                else if (indexVariable.Value == 7)
                    indexVariable.Value = 0;
                else if (indexVariable.Value <= 9)
                    indexVariable.Value -= 3;
                else if (indexVariable.Value == 10)
                    indexVariable.Value = 4;
                else if (indexVariable.Value <= 12)
                    indexVariable.Value -= 4;
                else if (indexVariable.Value == 13)
                    indexVariable.Value = 2;
                else
                    indexVariable.Value -= 5;
                fsm.SendEvent("FINISHED");
            }, FsmTransitionData.FromTargetState("Repeat?").WithEventName("FINISHED"));

            // Down handling
            fsm.AddState("Down Press", () =>
            {
                enteredIndex.Value = 2;
                if (indexVariable.Value == 0)
                    indexVariable.Value = 7;
                else if (indexVariable.Value == 1)
                    indexVariable.Value = 5;
                else if (indexVariable.Value == 2)
                    indexVariable.Value = 13;
                else if (indexVariable.Value == 3)
                    indexVariable.Value = 6;
                else if (indexVariable.Value == 4)
                    indexVariable.Value = 10;
                else if (indexVariable.Value <= 6)
                    indexVariable.Value += 3;
                else if (indexVariable.Value <= 8)
                    indexVariable.Value += 4;
                else if (indexVariable.Value <= 15)
                    indexVariable.Value += 5;
                else
                    indexVariable.Value -= 16;
                fsm.SendEvent("FINISHED");
            }, FsmTransitionData.FromTargetState("Repeat?").WithEventName("FINISHED"));

            // Right handling
            fsm.AddState("Right Press", () =>
            {
                enteredIndex.Value = 1;
                if (indexVariable.Value == -2)
                    indexVariable.Value = 0;
                else if (indexVariable.Value == 4 || (indexVariable.Value > 5 && indexVariable.Value % 5 == 0))
                {
                    indexVariable.Value = -1; // To right arrow
                    fsm.SendEvent("OUT");
                    return;
                }
                else if (indexVariable.Value == 5)
                    indexVariable.Value = 2;
                else if (indexVariable.Value == 6)
                    indexVariable.Value = 4;
                else
                    indexVariable.Value++;
                fsm.SendEvent("FINISHED");
            }, FsmTransitionData.FromTargetState("R Arrow").WithEventName("OUT"),
                FsmTransitionData.FromTargetState("Repeat?").WithEventName("FINISHED"));

            // Left handling
            fsm.AddState("Left Press", () =>
            {
                enteredIndex.Value = 3;
                if (indexVariable.Value == -1)
                    indexVariable.Value = 4;
                else if (indexVariable.Value == 0 || indexVariable.Value == 7 || indexVariable.Value == 11 || indexVariable.Value == 16)
                {
                    indexVariable.Value = -1; // To left arrow
                    fsm.SendEvent("OUT");
                    return;
                }
                else if (indexVariable.Value == 5)
                    indexVariable.Value = 0;
                else if (indexVariable.Value == 6)
                    indexVariable.Value = 2;
                else
                    indexVariable.Value--;
                fsm.SendEvent("FINISHED");
            }, FsmTransitionData.FromTargetState("L Arrow").WithEventName("OUT"),
                FsmTransitionData.FromTargetState("Repeat?").WithEventName("FINISHED"));
            fsm.GetState("R Arrow").InsertActions(0, () => LogHelper.Write("Entered R Arrow"));

            currentWorkingState = fsm.GetState("Repeat?");
            currentWorkingState.AddTransition("FINISHED", "Powers");
            currentWorkingState.AddTransition("REPEAT UP", "Up Press");
            currentWorkingState.AddTransition("REPEAT RIGHT", "Right Press");
            currentWorkingState.AddTransition("REPEAT DOWN", "Down Press");
            currentWorkingState.AddTransition("REPEAT LEFT", "Left Press");

            // Allow transitions from the main state to the movement states.
            currentWorkingState = fsm.GetState("Powers");
            currentWorkingState.AddTransition("UI UP", "Up Press");
            currentWorkingState.AddTransition("UI RIGHT", "Right Press");
            currentWorkingState.AddTransition("UI DOWN", "Down Press");
            currentWorkingState.AddTransition("UI LEFT", "Left Press");

            fsm.GetState("L Arrow").AddTransition("UI RIGHT", "Right Press");
            fsm.GetState("R Arrow").AddTransition("UI LEFT", "Left Press");

            //fsm.AddState("Toggle", () =>
            //{

            //}, FsmTransitionData.FromTargetState("Powers").WithEventName("UI Confirm"), 
            //   FsmTransitionData.FromTargetState("Powers").WithEventName("Cancel"));
            //// Toggle
            //currentWorkingState = fsm.GetState("Toggle Power");
            //currentWorkingState.AddTransition("FINISHED", "Powers");
            //currentWorkingState.AddLastAction(new Lambda(() =>
            //{

            //}));

            // Add seperators
            GameObject leftSeparator = UnityEngine.Object.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Divider L").gameObject);
            leftSeparator.transform.SetParent(lorePage.transform);
            leftSeparator.transform.position = new(-8.15f, -0.7555f, 3.3f);
            leftSeparator.transform.localScale = new(6.6422f, 0.5253f, 1.3674f);
            GameObject rightSeparator = UnityEngine.Object.Instantiate(leftSeparator, lorePage.transform);
            rightSeparator.transform.position = new(7.95f, -0.7555f, 3.3f);
            rightSeparator.transform.localScale = new(6.6422f, 0.5253f, 1.3674f);
            GameObject bottomSeparator = UnityEngine.Object.Instantiate(leftSeparator, lorePage.transform);
            bottomSeparator.transform.position = new(-0.55f, -4.1919f, 3.3f);
            bottomSeparator.transform.localScale = new(6.6422f, 0.5253f, 1.3674f);
            bottomSeparator.transform.SetRotationZ(0f);

            //BuildExtraItems(lorePage);
            lorePage.SetActive(false);
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("An error occured in the inventory: " + exception.Message);
            LoreMaster.Instance.LogError("An error occured in the inventory: " + exception.StackTrace);
        }
    }

    internal static (int, PowerRank) GetMatchingIndex(int index)
    {
        (Vector3, PowerRank) rank = _glyphPositions[index];
        int typeIndex = _glyphPositions.Where(x => x.Item2 == rank.Item2)
            .Select(x => x.Item1)
            .ToList()
            .IndexOf(rank.Item1);
        LogHelper.Write("Type index: " + typeIndex + " from type: " + rank.Item2);
        return new(typeIndex, rank.Item2);
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
        fsm.GetState("Stag Egg").AddTransition("UI CONFIRM", "Spawn");
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
        spriteRenderer.sprite = SpriteHelper.CreateSprite<LoreMaster>("Sprites.SummoningScroll");
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
        spriteRenderer.sprite = SpriteHelper.CreateSprite<LoreMaster>("Sprites.CurseDispell");
        GameObject cleanseAmount = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Desc").gameObject);
        cleanseAmount.transform.SetParent(cleanseScroll.transform);
        cleanseAmount.transform.localScale = new(1.2f, 1.2f, 1);
        cleanseAmount.transform.localPosition = new(5.7527f, -5.8764f, -6f);
        cleanseAmount.GetComponent<TextMeshPro>().text = "0";
        cleanseAmount.SetActive(true);
        cleanseScroll.SetActive(false);
        _controlElements.Add("Cleanse", cleanseScroll);

        GameObject cursor = lorePage.transform.Find("Cursor").gameObject;
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

        fsm.GetState("Move Pane R").InsertActions(0, () => interactSprite.GetComponent<SpriteRenderer>().sprite = null);
        fsm.GetState("Move Pane L").InsertActions(0, () => interactSprite.GetComponent<SpriteRenderer>().sprite = null);
    }

    internal static void ActivateStagEgg() => _stagEgg.gameObject.SetActive(true);

    #endregion
}
