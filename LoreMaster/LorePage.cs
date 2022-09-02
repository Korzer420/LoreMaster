using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Helper;
using LoreMaster.LorePowers;
using LoreMaster.LorePowers.CityOfTears;
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

    private static Sprite _loreSprite;

    private static Sprite _notActive;

    private static GameObject[] _loreObjects;

    private static readonly Dictionary<Area, Color> _colors = new()
    {
        {Area.WhitePalace, Color.white },
        {Area.Greenpath, Color.green },
        {Area.WaterWays, Color.blue },
        {Area.CityOfTears, Color.cyan },
        {Area.AncientBasin, Color.gray },
        {Area.RestingGrounds, Color.red },
        {Area.Peaks, Color.magenta },
        {Area.Dirtmouth, Color.yellow},
        {Area.FungalWastes, new(1f, 0.4f, 0f) },
        {Area.FogCanyon, new(.75f, 0f, 1f) },
        {Area.QueensGarden, new(.1f, 1f, .3f) },
        {Area.Crossroads, new(.4f,.25f,.25f) },
        {Area.Cliffs, new(.2f,.5f,0f) },
        {Area.Deepnest, new(.3f,0f,.6f)},
        {Area.KingdomsEdge, new(.2f,.4f,.6f) }
    };

    #endregion

    #region Constructors

    static LorePage()
    {
        _emptySprite = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Backboards/BB 3").GetComponent<SpriteRenderer>().sprite;
        _loreSprite = SpriteHelper.CreateSprite("Lore");
        _notActive = SpriteHelper.CreateSprite("DisabledPower");
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
                    _loreObjects[i].GetComponent<SpriteRenderer>().sprite = _notActive;
                    _loreObjects[i].GetComponent<SpriteRenderer>().color = PowerManager.ActivePowers.Contains(_powers[i])
                        ? Color.red
                        : Color.grey;
                }
                else
                {
                    if (_powers[i].Active)
                    {
                        _loreObjects[i].GetComponent<SpriteRenderer>().sprite = _loreSprite;
                        _loreObjects[i].GetComponent<SpriteRenderer>().color = _colors[_powers[i].Location];
                    }
                    else if (PowerManager.ActivePowers.Contains(_powers[i]))
                    {
                        _loreObjects[i].GetComponent<SpriteRenderer>().sprite = _notActive;
                        _loreObjects[i].GetComponent<SpriteRenderer>().color = Color.red;
                    }
                    else
                    {
                        _loreObjects[i].GetComponent<SpriteRenderer>().sprite = _emptySprite;
                        _loreObjects[i].GetComponent<SpriteRenderer>().color = Color.white;
                    }

                }
            }
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
                tablet.AddComponent<SpriteRenderer>().sprite = _emptySprite;
                tablet.GetComponent<SpriteRenderer>().sortingLayerID = 629535577;
                tablet.GetComponent<SpriteRenderer>().sortingLayerName = "HUD";
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
                        if (PowerManager.ActivePowers.Contains(_powers[indexVariable.Value]))
                        {
                            Power power = _powers[indexVariable.Value];
                            powerTitle.GetComponent<TextMeshPro>().text = power.Tag == PowerTag.Global || PowerManager.IsAreaGlobal(power.Location)
                            ? "<color=#7FFF7B>"+ power.PowerName + "</color>"
                            : power.PowerName;
                            powerDescription.GetComponent<TextMeshPro>().text = LoreManager.Instance.UseHints
                            ? power.Hint.Replace("<br>","\r\n")
                            :power.Description.Replace("<br>","\r\n");
                            if(power.Active)
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
                if (indexVariable.Value == 9 || indexVariable.Value % 10 == 9 || indexVariable.Value == 55)
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
            currentWorkingState.AddTransition("FINISHED", "Powers");
            currentWorkingState.AddLastAction(new Lambda(() =>
            {
                if (indexVariable.Value <= 5)
                    indexVariable.Value += 50;
                else if (indexVariable.Value < 10)
                    indexVariable.Value += 40;
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
                else if (indexVariable.Value >= 46)
                    indexVariable.Value -= 40;
                else
                    indexVariable.Value += 10;
                fsm.SendEvent("FINISHED");
            }));

            // Toggle
            currentWorkingState = fsm.GetState("Toggle Power");
            currentWorkingState.AddTransition("FINISHED", "Powers");
            currentWorkingState.AddLastAction(new Lambda(() =>
            {
                if (PlayerData.instance.GetBool(nameof(PlayerData.instance.atBench)) && PowerManager.ActivePowers.Contains(_powers[indexVariable.Value]))
                {
                    Power power = _powers[indexVariable.Value];

                    if (power.Active)
                    {
                        if (power is MarissasAudiencePower audience && audience.IsMarissaDead)
                            return;

                        power.DisablePower();
                        _loreObjects[indexVariable.Value].GetComponent<SpriteRenderer>().sprite = _notActive;
                        _loreObjects[indexVariable.Value].GetComponent<SpriteRenderer>().color = Color.red;
                        power.Tag = power.DefaultTag != PowerTag.Remove && power.DefaultTag != PowerTag.Disable
                        ? PowerTag.Disable
                        : power.DefaultTag;
                    }
                    else if(power.Tag == PowerTag.Global || PowerManager.IsAreaGlobal(power.Location) || SettingManager.Instance.CurrentArea == power.Location)
                    {
                        
                        _loreObjects[indexVariable.Value].GetComponent<SpriteRenderer>().sprite = _loreSprite;
                        _loreObjects[indexVariable.Value].GetComponent<SpriteRenderer>().color = _colors[power.Location];
                        power.Tag = power.DefaultTag == PowerTag.Remove || power.DefaultTag == PowerTag.Disable
                        ? PowerTag.Local
                        : power.DefaultTag;
                        power.EnablePower();
                    }
                }
            }));

            lorePage.SetActive(false);
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("An error occured in the inventory: " + exception.Message);
        }
    }

    #endregion
}
