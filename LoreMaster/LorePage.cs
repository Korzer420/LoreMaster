using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Helper;
using LoreMaster.LorePowers;
using LoreMaster.LorePowers.CityOfTears;
using LoreMaster.LorePowers.HowlingCliffs;
using LoreMaster.Manager;
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

    #endregion

    #region Constructors

    static LorePage()
    {
        _emptySprite = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Backboards/BB 3").GetComponent<SpriteRenderer>().sprite;
        _notActive = SpriteHelper.CreateSprite("DisabledPower");
        for (int i = 1; i < 16; i++)
            _sprites.Add((Area)i, SpriteHelper.CreateSprite($"/Tablets/{((Area)i)}"));
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
                if (_loreObjects[i] == null)
                {
                    LoreMaster.Instance.LogDebug("Lore Object " + i + " doesn't exist");
                    continue;
                }

                if (_powers[i].Tag == PowerTag.Remove)
                {
                    _loreObjects[i].GetComponentInChildren<SpriteRenderer>().sprite = _notActive;
                    _loreObjects[i].GetComponentInChildren<SpriteRenderer>().color = PowerManager.ObtainedPowers.Contains(_powers[i])
                        ? Color.red
                        : Color.grey;
                }
                else
                {
                    if (_powers[i].State == PowerState.Active)
                    {
                        _loreObjects[i].GetComponentInChildren<SpriteRenderer>().sprite = _sprites[_powers[i].Location];
                        _loreObjects[i].GetComponentInChildren<SpriteRenderer>().color = Color.white;
                    }
                    else if (PowerManager.ObtainedPowers.Contains(_powers[i]))
                    {
                        _loreObjects[i].GetComponentInChildren<SpriteRenderer>().sprite = _notActive;
                        _loreObjects[i].GetComponentInChildren<SpriteRenderer>().color = Color.red;
                    }
                    else
                    {
                        _loreObjects[i].GetComponentInChildren<SpriteRenderer>().sprite = _emptySprite;
                        _loreObjects[i].GetComponentInChildren<SpriteRenderer>().color = Color.white;
                    }
                }
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

            GameObject powerDescription = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Desc").gameObject);
            powerDescription.transform.SetParent(lorePage.transform);
            powerDescription.transform.localPosition = new(13f, -9f, 1f);
            powerDescription.GetComponent<TextMeshPro>().text = "";

            GameObject confirmButton = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Confirm Action").gameObject);
            confirmButton.transform.SetParent(lorePage.transform);
            UnityEngine.Object.Destroy(confirmButton.GetComponent<PlayMakerFSM>());
            confirmButton.transform.localPosition = new(3.72f, -3.36f, -30.13f);
            confirmButton.transform.Find("Text").GetComponent<TextMeshPro>().text = "Toggle Power";
            confirmButton.SetActive(false);

            // Generates all power objects
            float xPosition = -11f; // in 1,5f steps
            float yPosition = 4.6f; // in -2f steps
            _loreObjects = new GameObject[_powers.Count];
            for (int i = 1; i <= _powers.Count; i++)
            {
                GameObject tablet = new("Power " + i);
                tablet.transform.SetParent(powerList.transform);
                tablet.transform.localScale = new Vector3(1f, 1f, 1f);
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
                child.transform.localScale = new(0.4f, 0.4f, 1f);
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
                foreach (Transform child in lorePage.transform)
                    if (!child.gameObject.name.Contains("Stag"))
                        child.gameObject.SetActive(true);
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
                        if (PowerManager.ObtainedPowers.Contains(_powers[indexVariable.Value]))
                        {
                            Power power = _powers[indexVariable.Value];
                            powerTitle.GetComponent<TextMeshPro>().text = power.Tag == PowerTag.Global || PowerManager.IsAreaGlobal(power.Location)
                            ? "<color=#7FFF7B>"+ power.PowerName + "</color>"
                            : power.PowerName;
                            powerDescription.GetComponent<TextMeshPro>().text = LoreManager.Instance.UseHints
                            ? power.Hint.Replace("<br>","\r\n")
                            :power.Description.Replace("<br>","\r\n");
                            if(power.State == PowerState.Active)
                                confirmButton.SetActive(PlayerData.instance.GetBool(nameof(PlayerData.instance.atBench)));
                            else
                               confirmButton.SetActive(PlayerData.instance.GetBool(nameof(PlayerData.instance.atBench))
                                   && (PowerManager.IsAreaGlobal(power.Location) || SettingManager.Instance.CurrentArea == power.Location));
                        }
                        else
                        {
                            powerTitle.GetComponent<TextMeshPro>().text = "???";
                            powerDescription.GetComponent<TextMeshPro>().text = "You don't have obtained the power yet. Maybe someone can help you finding it?";
                            confirmButton.SetActive(false);
                        }
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
                if (PlayerData.instance.GetBool(nameof(PlayerData.instance.atBench)) && PowerManager.ObtainedPowers.Contains(_powers[indexVariable.Value]))
                {
                    Power power = _powers[indexVariable.Value];

                    if (power.State == PowerState.Active && SettingManager.Instance.GameMode != GameMode.Heroic)
                    {
                        if (power is MarissasAudiencePower audience && audience.IsMarissaDead)
                            return;

                        power.DisablePower();
                        _loreObjects[indexVariable.Value].GetComponentInChildren<SpriteRenderer>().sprite = _notActive;
                        _loreObjects[indexVariable.Value].GetComponentInChildren<SpriteRenderer>().color = Color.red;
                        power.Tag = power.DefaultTag != PowerTag.Remove && power.DefaultTag != PowerTag.Disable
                        ? PowerTag.Disable
                        : power.DefaultTag;
                    }
                    else if ((power.Tag == PowerTag.Global || PowerManager.IsAreaGlobal(power.Location) || SettingManager.Instance.CurrentArea == power.Location)
                     && !power.StayTwisted)
                    {
                        _loreObjects[indexVariable.Value].GetComponentInChildren<SpriteRenderer>().sprite = _sprites[power.Location];
                        _loreObjects[indexVariable.Value].GetComponentInChildren<SpriteRenderer>().color = Color.white;
                        power.Tag = power.DefaultTag == PowerTag.Remove || power.DefaultTag == PowerTag.Disable
                        ? PowerTag.Local
                        : power.DefaultTag;
                        power.EnablePower();
                    }
                }
            }));

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
            currentWorkingState = new FsmState(fsm.Fsm)
            {
                Name = "Stag Egg",
                Actions = new FsmStateAction[]
                {
                    new Lambda(() => fsm.gameObject.LocateMyFSM("Update Cursor").FsmVariables.FindFsmGameObject("Item").Value = _stagEgg.gameObject),
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
                            confirmButton.SetActive(false);
                            fsm.SendEvent("UI RIGHT");
                            return;
                        }
                        powerTitle.GetComponent<TextMeshPro>().text = StagAdoptionPower.Instance.CanSpawnStag ? "Stag Egg" : "Broken Stag Egg";
                        powerDescription.GetComponent<TextMeshPro>().text = StagAdoptionPower.Instance.CanSpawnStag
                        ? "You can feel something moving in there... maybe tapping it does the trick?"
                        : "The empty shell of a stag. They probably now live a happier life... somewhere.";
                        confirmButton.SetActive(StagAdoptionPower.Instance.CanSpawnStag);
                    })
                }
            };
            currentWorkingState.AddTransition("UI RIGHT", "R Arrow");
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
                        powerTitle.GetComponent<TextMeshPro>().text = "Broken Stag Egg";
                        powerDescription.GetComponent<TextMeshPro>().text = "The empty shell of a stag. They probably now live a happier life... somewhere.";
                    })
                }
            };
            fsm.AddState(currentWorkingState);
            fsm.GetState("Stag Egg").AddTransition("UI CONFIRM", currentWorkingState);
            currentWorkingState.AddTransition("FINISHED", "Stag Egg");
            fsm.GetState("Right Press").AddTransition("STAG", "Stag Egg");
            stagEgg.SetActive(false);

            lorePage.SetActive(false);
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("An error occured in the inventory: " + exception.Message);
            LoreMaster.Instance.LogError("An error occured in the inventory: " + exception.StackTrace);
        }
    }

    internal static void ActivateStagEgg()
    {
        _stagEgg.gameObject.SetActive(true);
    }

    #endregion
}
