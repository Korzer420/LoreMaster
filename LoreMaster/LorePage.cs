using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.FsmStateActions;
using KorzUtils.Data;
using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.LorePowers;
using LoreMaster.LorePowers.HowlingCliffs;
using LoreMaster.Manager;
using LoreMaster.Properties;
using RandomizerMod.RandomizerData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LoreMaster;

internal static class LorePage
{
    #region Members

    private const int SortingLayerId = 629535577;

    private static GameObject[] _glyphObjects;

    private static SpriteRenderer _stagEgg;

    private static Dictionary<Area, Sprite> _tabletSprites = [];

    private static Dictionary<string, Sprite> _emptySprites = [];

    private static Dictionary<string, GameObject> _controlElements = [];

    private static string _lastState;

    private static List<Power> _availablePowers = [];

    private static (Vector3, PowerRank)[] _glyphPositions =
    [
        // --First "line"--
        (new(-11.25f, 3.8f, -3), PowerRank.Lower),
        (new(-8.45f, 5, -3), PowerRank.Lower),
        (new(-5, 4.55f, -3f), PowerRank.Greater),
        (new(-1.55f, 5, -3), PowerRank.Lower),
        (new(1.25f, 3.8f, -3), PowerRank.Lower),
        // --Second "line"--
        (new(-8.275f, 2.5f, -3), PowerRank.Medium),
        (new(-1.725f, 2.5f, -3), PowerRank.Medium),
        // --Third "line"--
        (new(-10.45f, 1, -3), PowerRank.Lower),
        (new(-8.275f, -0.4f, -3), PowerRank.Medium),
        (new(-1.725f, -0.4f, -3), PowerRank.Medium),
        (new(0.45f, 1, -3), PowerRank.Lower),
        // --Fourth "Line"--
        (new(-11.55f, -2.45f, -3), PowerRank.Greater),
        (new(-8.3f, -3.2f, -3), PowerRank.Lower),
        (new(-5, -3.1f, -3), PowerRank.Medium),
        (new(-1.7f, -3.2f, -3), PowerRank.Lower),
        (new(1.55f, -2.45f, -3), PowerRank.Greater),
        // --Fifth "Line"--
        (new(-10, -5.3f, -3), PowerRank.Permanent),
        (new(-7.5f, -5.3f, -3), PowerRank.Permanent),
        (new(-5, -5.3f, -3), PowerRank.Permanent),
        (new(-2.5f, -5.3f, -3), PowerRank.Permanent),
        (new(0, -5.3f, -3), PowerRank.Permanent),
    ];

    #endregion

    #region Constructors

    static LorePage()
    {
        _emptySprites.Add("Empty_Permanent", GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Backboards/BB 3").GetComponent<SpriteRenderer>().sprite);
        _emptySprites.Add("MajorGlyph_Locked", SpriteHelper.CreateSprite<LoreMaster>("Sprites.MajorGlyph_Locked"));
        _emptySprites.Add("MajorGlyph_Unlocked", SpriteHelper.CreateSprite<LoreMaster>("Sprites.MajorGlyph_Unlocked"));
        _emptySprites.Add("MinorGlyph_Locked", SpriteHelper.CreateSprite<LoreMaster>("Sprites.MinorGlyph_Locked"));
        _emptySprites.Add("MinorGlyph_Unlocked", SpriteHelper.CreateSprite<LoreMaster>("Sprites.MinorGlyph_Unlocked"));
        _emptySprites.Add("SmallGlyph_Locked", SpriteHelper.CreateSprite<LoreMaster>("Sprites.SmallGlyph_Locked"));
        _emptySprites.Add("SmallGlyph_Unlocked", SpriteHelper.CreateSprite<LoreMaster>("Sprites.SmallGlyph_Unlocked"));
        for (int i = 1; i < 16; i++)
            _tabletSprites.Add((Area)i, SpriteHelper.CreateSprite<LoreCore.LoreCore>($"Sprites.Tablets.{(Area)i}"));
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
            for (int i = 0; i < 21; i++)
            {
                (int, PowerRank) data = GetMatchingIndex(i);
                SpriteRenderer spriteRenderer = _glyphObjects[i].GetComponentInChildren<SpriteRenderer>();
                if (LoreManager.Module.IsIndexAvailable(data))
                {
                    Power power = PowerManager.GetPowerInSlot(data);
                    if (power != null)
                        spriteRenderer.sprite = _tabletSprites[power.Location];
                    else
                        spriteRenderer.sprite = GetSlotSprite(data.Item2, false);
                }
                else
                    spriteRenderer.sprite = GetSlotSprite(data.Item2);
            }
            //_stagEgg.sprite = StagAdoptionPower.Instance.CanSpawnStag
            //    ? StagAdoptionPower.Instance.InventorySprites[0]
            //    : StagAdoptionPower.Instance.InventorySprites[1];
        }
        catch (Exception exception)
        {
            LogHelper.Write<LoreMaster>("Error while updating inventory", exception);
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
            _controlElements.Add("Cursor", lorePage.transform.Find("Cursor").gameObject);
            PlayMakerFSM fsm = lorePage.LocateMyFSM("Empty UI");
            CreateObjects(lorePage);
            SetupFsm(fsm, lorePage);

            //BuildExtraItems(lorePage);
            lorePage.SetActive(false);
        }
        catch (Exception exception)
        {
            LogHelper.Write<LoreMaster>("Error while generating inventory", exception);
        }
    }

    private static void CreateObjects(GameObject lorePage)
    {
        // Generates the power holder
        GameObject powerList = new("Power List");
        powerList.transform.SetParent(lorePage.transform);
        powerList.transform.localPosition = new(0f, 0f, 0f);
        powerList.transform.localScale = new(1f, 1f, 1f);
        powerList.layer = lorePage.layer;

        GameObject powerTitle = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Name").gameObject);
        powerTitle.transform.SetParent(lorePage.transform);
        powerTitle.transform.position = new(8.45f, 1.05f, 0.3f);
        powerTitle.GetComponent<TextMeshPro>().text = "";
        powerTitle.GetComponent<TextMeshPro>().fontSize = 5;
        _controlElements.Add("PowerTitle", powerTitle);

        GameObject powerDescription = GenerateTextObject(lorePage, "PowerDescription", new(8.3973f, 1.1f /*-5.8f*/, 3.3f), 5, new(9f, 8f));
        TextMeshPro description = powerDescription.GetComponent<TextMeshPro>();
        description.OverflowMode = TextOverflowModes.Truncate;
        description.enableAutoSizing = true;
        description.fontSizeMax = 5;
        description.fontSizeMin = 2;

        //GameObject confirmButton = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Confirm Action").gameObject);
        //confirmButton.transform.SetParent(lorePage.transform);
        //UnityEngine.Object.Destroy(confirmButton.GetComponent<PlayMakerFSM>());
        //confirmButton.transform.localPosition = new(3.72f, -3.36f, -30.13f);
        //confirmButton.transform.Find("Text").GetComponent<TextMeshPro>().text = "Toggle Power";
        //confirmButton.SetActive(false);
        //_controlElements.Add("confirmButton", confirmButton);

        GameObject rotateLeftArrow = new("MoveLeft");
        rotateLeftArrow.transform.SetParent(lorePage.transform);
        rotateLeftArrow.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<LoreMaster>("Sprites.ArrowSprite");
        rotateLeftArrow.GetComponent<SpriteRenderer>().flipX = true;
        rotateLeftArrow.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayerId;
        _controlElements.Add("MoveLeft", rotateLeftArrow);
        rotateLeftArrow.SetActive(false);

        GameObject rotateRightArrow = new("MoveRight");
        rotateRightArrow.transform.SetParent(lorePage.transform);
        rotateRightArrow.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<LoreMaster>("Sprites.ArrowSprite");
        rotateRightArrow.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayerId;
        _controlElements.Add("MoveRight", rotateRightArrow);
        rotateRightArrow.SetActive(false);

        rotateLeftArrow.layer = lorePage.layer;
        rotateRightArrow.layer = lorePage.layer;

        GenerateTextObject(lorePage, "FirstSetBoni", new(5.85f, - 7.15f, 3.3f), 6).GetComponent<TextMeshPro>().text = "??????????";
        //GameObject firstSetBoni = GameObject.Instantiate(powerTitle);
        //firstSetBoni.transform.SetParent(lorePage.transform);
        //firstSetBoni.transform.localPosition = new(15.5f, -14.5f, 1f);
        //firstSetBoni.GetComponent<TextMeshPro>().text = "Unstoppable Force";
        //firstSetBoni.GetComponent<TextMeshPro>().fontSize = 6;
        //_controlElements.Add("FirstSetBoni", firstSetBoni);

        GenerateTextObject(lorePage, "SecondSetBoni", new(5.85f, -8.65f, 3.3f), 6).GetComponent<TextMeshPro>().text = "??????????";
        //GameObject secondSetBoni = GameObject.Instantiate(powerTitle);
        //secondSetBoni.transform.SetParent(lorePage.transform);
        //secondSetBoni.transform.localPosition = new(15.5f, -16f, 1f);
        //secondSetBoni.GetComponent<TextMeshPro>().text = "Unstoppable Force";
        //secondSetBoni.GetComponent<TextMeshPro>().fontSize = 6;
        //_controlElements.Add("SecondSetBoni", secondSetBoni);

        GenerateTextObject(lorePage, "ThirdSetBoni", new(5.85f, -10.15f, 3.3f), 6).GetComponent<TextMeshPro>().text = "??????????";
        //GameObject thirdSetBoni = GameObject.Instantiate(powerTitle);
        //thirdSetBoni.transform.SetParent(lorePage.transform);
        //thirdSetBoni.transform.localPosition = new(15.5f, -17.5f, 1f);
        //thirdSetBoni.GetComponent<TextMeshPro>().text = "Unstoppable Force";
        //thirdSetBoni.GetComponent<TextMeshPro>().fontSize = 6;
        //_controlElements.Add("ThirdSetBoni", thirdSetBoni);

        // Generates all power objects
        _glyphObjects = new GameObject[24];
        for (int i = 1; i <= 21; i++)
        {
            Vector3 scale;
            if (_glyphPositions[i - 1].Item2 == PowerRank.Greater)
                scale = new Vector3(1.5f, 1.5f, 1f);
            else if (_glyphPositions[i - 1].Item2 == PowerRank.Lower)
                scale = new Vector3(1f, 1f, 1f);
            else
                scale = new Vector3(1.2f, 1.2f, 1f);
            GameObject glyphObject = GenerateSpriteObject(powerList, $"Glyph Slot {i}", GetSlotSprite(PowerRank.Permanent), _glyphPositions[i - 1].Item1, scale);
            _glyphObjects[i - 1] = glyphObject;
        }

        // Add seperators
        GameObject rightSeparator = UnityEngine.Object.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Divider L").gameObject);
        rightSeparator.transform.SetParent(lorePage.transform);
        rightSeparator.transform.position = new(2.95f, -0.7555f, 3.3f);
        rightSeparator.transform.localScale = new(6.6422f, 0.5253f, 1.3674f);
        GameObject bottomSeparator = UnityEngine.Object.Instantiate(rightSeparator, lorePage.transform);
        bottomSeparator.transform.position = new(-5.55f, -4.1919f, 3.3f);
        bottomSeparator.transform.localScale = new(6.6422f, 0.5253f, 1.3674f);
        bottomSeparator.transform.SetRotationZ(0f);

        // Extra items
        GameObject knowledgeScrolls = GenerateSpriteObject(lorePage, "Knowledge Scrolls", "SummoningScroll", new(4f, -2.3f, 0), new(1.5f, 1.5f));
        knowledgeScrolls.transform.localScale = new(0.8f, 0.8f);
        GenerateTextObject(knowledgeScrolls, "KnowledgeScrollCount", new(4.6f, -7.3f), 8);
        _glyphObjects[21] = knowledgeScrolls;
        GameObject cleanseScrolls = GenerateSpriteObject(lorePage, "Cleansing Scrolls", "CurseDispell", new(4f, -3.8f, 0f), new(1.5f, 1.5f));
        cleanseScrolls.transform.localScale = new(0.8f, 0.8f);
        GenerateTextObject(cleanseScrolls, "CleansingScrollsCount", new(4.6f, -8.8f, 0f), 8);
        _glyphObjects[22] = cleanseScrolls;
        GameObject stagEgg = GenerateSpriteObject(lorePage, "Stag Egg", "Stag_Egg", new(4f, -5.3f, 0f), new(1.2f, 1.2f));
        stagEgg.transform.localScale = new(0.8f, 0.8f);
    }

    private static void SetupFsm(PlayMakerFSM fsm, GameObject lorePage)
    {
        // Add index variable for glyph slot
        fsm.AddVariable("ItemIndex", 0);
        // Add variable for checking which transition entered the check state
        fsm.AddVariable("SourceStateId", 0);
        // Add variable to track selected power
        fsm.AddVariable("ChosenPower", 0);
        fsm.AddVariable("Reminder", new GameObject("Bla"));
        FsmInt indexVariable = fsm.FsmVariables.FindFsmInt("ItemIndex");
        FsmInt enteredIndex = fsm.FsmVariables.FindFsmInt("SourceStateId");
        FsmInt chosenPower = fsm.FsmVariables.FindFsmInt("ChosenPower");
        PlayMakerFSM charmFsm = lorePage.transform.parent.Find("Charms").gameObject.LocateMyFSM("UI Charms");

        // Fsm initialization
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
                if (runs == 4)
                    break;
            }
        });

        // Setup main state
        fsm.AddState("Powers", new List<FsmStateAction>()
            {
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
                            descriptionText = selectedPower.Hint;
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

                            bool available = selectedPowerSlot.Item2 switch
                            {
                                PowerRank.Greater => selectedPowerSlot.Item1 < LoreManager.Module.MajorGlyphSlots,
                                PowerRank.Medium => selectedPowerSlot.Item1 < LoreManager.Module.MinorGlyphSlots,
                                PowerRank.Lower => selectedPowerSlot.Item1 < LoreManager.Module.SmallGlyphSlots,
                                _ => true
                            };
                            descriptionText = selectedPowerSlot.Item2 switch
                            {
                                PowerRank.Greater => available ? AdditionalText.Greater_Glyph_Description : AdditionalText.Greater_Glyph_Description_Locked,
                                PowerRank.Medium => available ? AdditionalText.Medium_Glyph_Description : AdditionalText.Medium_Glyph_Description_Locked,
                                PowerRank.Lower => available ? AdditionalText.Lesser_Glyph_Description : AdditionalText.Lesser_Glyph_Description_Locked,
                                _ => AdditionalText.Permanent_Glyph_Description
                            };
                        }
                        _controlElements["PowerTitle"].GetComponent<TextMeshPro>().text = titleText;
                        _controlElements["PowerDescription"].GetComponent<TextMeshPro>().text = descriptionText;
                    }
                    else if (selectedIndex == 21)
                    {
                        _controlElements["PowerTitle"].GetComponent<TextMeshPro>().text = "Knowledge Scroll";
                        _controlElements["PowerDescription"].GetComponent<TextMeshPro>().text = "(Not implemented) \r\nWritten by Elderbug, this scroll contains energy capable of mimicking (almost) any knowledge, that can be found in this Kingdom. Though it is quite unstable and may break with your shell.";
                    }
                    else if (selectedIndex == 22)
                    {
                        _controlElements["PowerTitle"].GetComponent<TextMeshPro>().text = "Cleansing Scroll";
                        _controlElements["PowerDescription"].GetComponent<TextMeshPro>().text = "Written by Elderbug, this scroll contains the ability to empty your mind regardless of where you are. Might be useful to adapt to the dangers below more quickly.";
                    }
                    else if (selectedIndex == 23)
                    {
                        _controlElements["PowerTitle"].GetComponent<TextMeshPro>().text = "Stag Egg";
                        _controlElements["PowerDescription"].GetComponent<TextMeshPro>().text = "It feels like something is moving rapidly inside...";
                    }
                })
            }, null);
        currentWorkingState.AddTransition("FINISHED", "Powers");

        SetupCursorMovement(fsm);
        SetupPowerSelection(fsm, charmFsm);

        fsm.GetState("Powers").AddTransition("UI CONFIRM", "Init Toggle");
        fsm.GetState("Init Toggle").AddTransition("FINISHED", "Toggle");
        fsm.GetState("Swap Left").AddTransition("FINISHED", "Toggle");
        fsm.GetState("Swap Right").AddTransition("FINISHED", "Toggle");
    }

    private static void SetupCursorMovement(PlayMakerFSM fsm)
    {
        FsmInt indexVariable = fsm.FsmVariables.FindFsmInt("ItemIndex");
        FsmInt enteredIndex = fsm.FsmVariables.FindFsmInt("SourceStateId");

        // Removing the jump from arrow button to arrow button.
        fsm.GetState("L Arrow").RemoveTransitionsTo("R Arrow");
        fsm.GetState("R Arrow").RemoveTransitionsTo("L Arrow");

        // Setup state to skip unreachable items.
        fsm.AddState("Repeat?", () =>
        {
            if (indexVariable.Value > 20)
            {
                bool available;
                if (indexVariable.Value == 21)
                    available = true;
                else if (indexVariable.Value == 22)
                    available = LoreManager.Module.CleansingScrolls > 0;
                else
                    available = LoreManager.Module.HasStagEgg;
                if (available)
                    fsm.SendEvent("FINISHED");
                else
                    fsm.SendEvent(enteredIndex.Value switch
                    {
                        0 => "REPEAT UP",
                        1 => "REPEAT RIGHT",
                        2 => "REPEAT DOWN",
                        _ => "REPEAT LEFT"
                    });
            }
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
            else if (indexVariable.Value == 21 || indexVariable.Value == 22 || indexVariable.Value == 23)
                indexVariable.Value = indexVariable.Value;
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
            else if (indexVariable.Value == 21 || indexVariable.Value == 22 || indexVariable.Value == 23)
                indexVariable.Value = indexVariable.Value;
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
            else if (indexVariable.Value == 23)
            {
                indexVariable.Value = -1; // To right arrow
                fsm.SendEvent("OUT");
                return;
            }
            else if (indexVariable.Value == 4 || (indexVariable.Value > 5 && indexVariable.Value % 5 == 0))
                indexVariable.Value = 21;
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
                indexVariable.Value = 23;
            else if (indexVariable.Value == 0 || indexVariable.Value == 7 || indexVariable.Value == 11 || indexVariable.Value == 16)
            {
                indexVariable.Value = -1; // To left arrow
                fsm.SendEvent("OUT");
                return;
            }
            else if (indexVariable.Value == 5)
                indexVariable.Value = 0;
            else
                indexVariable.Value--;
            fsm.SendEvent("FINISHED");
        }, FsmTransitionData.FromTargetState("L Arrow").WithEventName("OUT"),
            FsmTransitionData.FromTargetState("Repeat?").WithEventName("FINISHED"));

        FsmState currentWorkingState = fsm.GetState("Repeat?");
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
    }

    private static void SetupPowerSelection(PlayMakerFSM fsm, PlayMakerFSM charmFsm)
    {
        FsmInt indexVariable = fsm.FsmVariables.FindFsmInt("ItemIndex");
        FsmInt enteredIndex = fsm.FsmVariables.FindFsmInt("SourceStateId");
        FsmInt chosenPower = fsm.FsmVariables.FindFsmInt("ChosenPower");
        fsm.AddState("Clear Selection", () =>
        {
            _controlElements["MoveLeft"].SetActive(false);
            _controlElements["MoveRight"].SetActive(false);

            _availablePowers.Clear();
            (int, PowerRank) rank = GetMatchingIndex(indexVariable.Value);
            Power power = indexVariable.Value == 21
                ? PowerManager.GetPowerByName(LoreManager.Module.TempPower)
                : PowerManager.GetPowerInSlot(rank);
            chosenPower.Value = power == null ? -1 : 0;
            // Adjust sprite.
            if (power != null)
                _glyphObjects[indexVariable.Value].GetComponentInChildren<SpriteRenderer>().sprite = _tabletSprites[power.Location];
            else
                _glyphObjects[indexVariable.Value].GetComponentInChildren<SpriteRenderer>().sprite = GetSlotSprite(rank.Item2, true);
        }, FsmTransitionData.FromTargetState("Powers").WithEventName("FINISHED"));
        fsm.AddState("Swap Left", () =>
        {
            if (chosenPower.Value == -1)
                chosenPower.Value = _availablePowers.Count - 1;
            else
                chosenPower.Value--;
            // Make small cursor animation.
            LoreMaster.Instance.Handler.StartCoroutine(PlayArrowAnimation(true));
        }, FsmTransitionData.FromTargetState("Clear Selection").WithEventName("Cancel"));
        fsm.GetState("Swap Left").AddActions(new Wait()
        {
            time = new(0.1f),
            finishEvent = fsm.FsmEvents.First(x => x.Name == "FINISHED")
        });
        fsm.AddState("Swap Right", () =>
        {
            if (chosenPower.Value == _availablePowers.Count - 1)
                chosenPower.Value = -1;
            else
                chosenPower.Value++;
            // Make small cursor animation.
            LoreMaster.Instance.Handler.StartCoroutine(PlayArrowAnimation(false));
        }, FsmTransitionData.FromTargetState("Clear Selection").WithEventName("Cancel"));
        fsm.GetState("Swap Right").AddActions(new Wait()
        {
            time = new(0.1f),
            finishEvent = fsm.FsmEvents.First(x => x.Name == "FINISHED")
        });
        fsm.AddState("Adjust Power", () =>
        {
            // 0 is always the already selected power.
            // -1 is no power.
            if (chosenPower.Value != 0)
            {
                (int, PowerRank) powerIndex = GetMatchingIndex(indexVariable.Value);
                Power powerInSlot = PowerManager.GetPowerInSlot(powerIndex);
                // Check if no power was selected before and now not again.
                if (chosenPower.Value == -1 && powerInSlot == null)
                    return;
                PowerManager.SwapPower(powerIndex, _availablePowers[chosenPower.Value].PowerName);
            }
        }, FsmTransitionData.FromTargetState("Clear Selection").WithEventName("FINISHED"));
        fsm.AddState("Remind for bench", charmFsm.GetState("Bench Reminder").GetActions(),
            FsmTransitionData.FromTargetState("Powers").WithEventName("FINISHED"));
        fsm.AddState("Init Toggle", () =>
        {
            if (indexVariable.Value == 21)
            {
                fsm.SendEvent("Cancel");
                return;
            }
            (int, PowerRank) selectedGlyph = GetMatchingIndex(indexVariable.Value);
            if (selectedGlyph.Item2 == PowerRank.Permanent)
            {
                fsm.SendEvent("Cancel");
                return;
            }
            else if (!PDHelper.AtBench)
            {
                // Show "You need to be at a bench prompt"
                fsm.SendEvent("NOT BENCH");
                return;
            }

            string powerInSlot = selectedGlyph.Item2 switch
            {
                PowerRank.Greater => PowerManager.ActiveMajorPowers[selectedGlyph.Item1],
                PowerRank.Medium => PowerManager.ActiveMediumPowers[selectedGlyph.Item1],
                _ => PowerManager.ActiveSmallPowers[selectedGlyph.Item1]
            };

            if (!string.IsNullOrEmpty(powerInSlot) && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Town")
            {
                // Show "can only change in Dirtmouth"
                fsm.SendEvent("Cancel");
                return;
            }
            _availablePowers.Clear();
            if (!string.IsNullOrEmpty(powerInSlot))
                _availablePowers.Add(PowerManager.GetPowerByName(powerInSlot));
            _availablePowers.AddRange(PowerManager.GetPowersByRank(selectedGlyph.Item2).Except(_availablePowers));
            if (!_availablePowers.Any())
            {
                fsm.SendEvent("Cancel");
                return;
            }
            // Move Cursor out of Screen
            _controlElements["Cursor"].transform.position = new(500f, 500f, _controlElements["Cursor"].transform.position.z);
            // Enable Arrows left and right.
            _controlElements["MoveLeft"].transform.position = _glyphPositions[indexVariable.Value].Item1 - new Vector3(1.5f, 0f);
            _controlElements["MoveLeft"].SetActive(true);
            _controlElements["MoveRight"].transform.position = _glyphPositions[indexVariable.Value].Item1 + new Vector3(1.5f, 0f);
            _controlElements["MoveRight"].SetActive(true);
        },
           FsmTransitionData.FromTargetState("Clear Selection").WithEventName("Cancel"),
           FsmTransitionData.FromTargetState("Remind for bench").WithEventName("NOT BENCH"));
        fsm.AddState("Toggle", () =>
        {
            // Adjust name, sprite and description.
            if (chosenPower.Value != -1)
            {
                Power power = _availablePowers[chosenPower.Value];
                _glyphObjects[indexVariable.Value].GetComponentInChildren<SpriteRenderer>().sprite = _tabletSprites[power.Location];
                _controlElements["PowerTitle"].GetComponent<TextMeshPro>().text = power.PowerName;
                _controlElements["PowerDescription"].GetComponent<TextMeshPro>().text = power.Hint;
            }
            else
            {
                (int, PowerRank) selectedPowerSlot = GetMatchingIndex(indexVariable.Value);
                _glyphObjects[indexVariable.Value].GetComponentInChildren<SpriteRenderer>().sprite = GetSlotSprite(selectedPowerSlot.Item2, false);
                _controlElements["PowerTitle"].GetComponent<TextMeshPro>().text = selectedPowerSlot.Item2 switch
                {
                    PowerRank.Greater => AdditionalText.Greater_Glyph_Title,
                    PowerRank.Medium => AdditionalText.Medium_Glyph_Title,
                    PowerRank.Lower => AdditionalText.Lesser_Glyph_Title,
                    _ => AdditionalText.Permanent_Glyph_Title
                };
                _controlElements["PowerDescription"].GetComponent<TextMeshPro>().text = selectedPowerSlot.Item2 switch
                {
                    PowerRank.Greater => AdditionalText.Greater_Glyph_Description,
                    PowerRank.Medium => AdditionalText.Medium_Glyph_Description,
                    PowerRank.Lower => AdditionalText.Lesser_Glyph_Description,
                    _ => AdditionalText.Permanent_Glyph_Description
                }; ;
            }
        }, FsmTransitionData.FromTargetState("Clear Selection").WithEventName("Cancel"),
        FsmTransitionData.FromTargetState("Adjust Power").WithEventName("UI CONFIRM"),
        FsmTransitionData.FromTargetState("Swap Left").WithEventName("UI LEFT"),
        FsmTransitionData.FromTargetState("Swap Right").WithEventName("UI RIGHT"));
    }

    private static IEnumerator PlayArrowAnimation(bool left)
    {
        GameObject toMove = left
            ? _controlElements["MoveLeft"]
            : _controlElements["MoveRight"];
        Vector3 defaultPosition = toMove.transform.position;
        float passedTime = 0f;
        while (passedTime < 0.05f)
        {
            toMove.transform.position += left
                ? new Vector3(Time.deltaTime * 2, 0f)
                : new Vector3(Time.deltaTime * -2, 0f);
            yield return null;
            passedTime += Time.deltaTime;
        }

        passedTime = 0f;
        while (passedTime < 0.05f)
        {
            toMove.transform.position += left
                ? new Vector3(Time.deltaTime * -2, 0f)
                : new Vector3(Time.deltaTime * 2, 0f);
            yield return null;
            passedTime += Time.deltaTime;
        }
        toMove.transform.position = defaultPosition;
    }

    internal static (int, PowerRank) GetMatchingIndex(int index)
    {
        (Vector3, PowerRank) rank = _glyphPositions[index];
        int typeIndex = _glyphPositions.Where(x => x.Item2 == rank.Item2)
            .Select(x => x.Item1)
            .ToList()
            .IndexOf(rank.Item1);
        return new(typeIndex, rank.Item2);
    }

    private static GameObject GenerateSpriteObject(GameObject parent, string objectName, string spriteName, Vector3 position, Vector3 scale = default)
        => GenerateSpriteObject(parent, objectName, SpriteHelper.CreateSprite<LoreMaster>($"Sprites.{spriteName}"), position, scale);

    private static GameObject GenerateSpriteObject(GameObject parent, string objectName, Sprite sprite, Vector3 position, Vector3 scale = default)
    {
        GameObject holderObject = new(objectName);
        holderObject.transform.SetParent(parent.transform);
        holderObject.transform.position = position;
        holderObject.layer = parent.layer;
        // The cursor need a collider to jump to
        holderObject.AddComponent<BoxCollider2D>().offset = new(0f, 0f);

        GameObject spriteObject = new($"{objectName} Sprite");
        spriteObject.transform.SetParent(holderObject.transform);
        spriteObject.transform.localPosition = new(0f, 0f, 0f);
        spriteObject.transform.localScale = scale == default
            ? new(1f, 1f, 1f)
            : scale;
        spriteObject.layer = parent.layer;
        spriteObject.AddComponent<SpriteRenderer>().sprite = sprite;
        spriteObject.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayerId;
        spriteObject.GetComponent<SpriteRenderer>().sortingLayerName = "HUD";

        return holderObject;
    }

    private static GameObject GenerateTextObject(GameObject parent, string objectName, Vector3 position, float fontSize, Vector3 size = default)
    {
        GameObject textObject = GameObject.Instantiate(_controlElements["PowerTitle"]);
        Component.Destroy(textObject.GetComponent<ChangeFontByLanguage>());
        textObject.name = objectName;
        textObject.transform.SetParent(parent.transform);
        textObject.transform.position = position;
        textObject.GetComponent<TextMeshPro>().text = "3";
        textObject.GetComponent<TextMeshPro>().fontSize = fontSize;
        if (size != default)
            textObject.GetComponent<TextContainer>().size = size;
        _controlElements.Add(objectName, textObject);
        return textObject;
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
        _stagEgg.sortingLayerID = SortingLayerId;
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
        spriteRenderer.sortingLayerID = SortingLayerId;
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
        spriteRenderer.sortingLayerID = SortingLayerId;
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

    private static Sprite GetSlotSprite(PowerRank rank, bool locked = true)
        => rank switch
        {
            PowerRank.Greater => locked ? _emptySprites["MajorGlyph_Locked"] : _emptySprites["MajorGlyph_Unlocked"],
            PowerRank.Medium => locked ? _emptySprites["MinorGlyph_Locked"] : _emptySprites["MinorGlyph_Unlocked"],
            PowerRank.Lower => locked ? _emptySprites["SmallGlyph_Locked"] : _emptySprites["SmallGlyph_Unlocked"],
            _ => _emptySprites["Empty_Permanent"]
        };

    internal static void ActivateStagEgg() => _stagEgg.gameObject.SetActive(true);

    #endregion
}
