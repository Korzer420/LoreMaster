using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
using LoreMaster.LorePowers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        {Area.Cliffs, new(.2f,1f,0f) },
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
        On.HeroController.CanOpenInventory += UpdateLorePage;
    }

    private static bool UpdateLorePage(On.HeroController.orig_CanOpenInventory orig, HeroController self)
    {
        if (orig(self))
        {
            try
            {

                for (int i = 0; i < _loreObjects.Length; i++)
                {
                    if (_loreObjects[i] == null)
                        continue;
                    if (_powers[i].Tag == PowerTag.Disable || _powers[i].Tag == PowerTag.Remove)
                    {
                        _loreObjects[i].GetComponent<SpriteRenderer>().sprite = _notActive;
                        _loreObjects[i].GetComponent<SpriteRenderer>().color = LoreMaster.Instance.ActivePowers.ContainsValue(_powers[i])
                            ? Color.red
                            : Color.grey;
                    }
                    else
                    {
                        _loreObjects[i].GetComponent<SpriteRenderer>().sprite = LoreMaster.Instance.ActivePowers.ContainsValue(_powers[i])
                                ? _loreSprite
                                : _emptySprite;
                        _loreObjects[i].GetComponent<SpriteRenderer>().color = LoreMaster.Instance.ActivePowers.ContainsValue(_powers[i])
                            ? _colors[_powers[i].Location]
                            : Color.white;
                    }
                }
            }
            catch (Exception exception)
            {
                LoreMaster.Instance.Log("Error when updating inventory: " + exception.Message);
            }
            return true;
        }
        return false;
    }

    #endregion

    #region Methods

    internal static void PassPowers(List<Power> powers)
        => _powers = powers;

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
            confirmButton.transform.localPosition = new(3.72f,-3.36f,-30.13f);
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
            fsm.AddState(new(fsm.Fsm)
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
                        if (LoreMaster.Instance.ActivePowers.ContainsValue(_powers[indexVariable.Value]))
                        {
                            powerTitle.GetComponent<TextMeshPro>().text = _powers[indexVariable.Value].PowerName;
                            powerDescription.GetComponent<TextMeshPro>().text = LoreMaster.Instance.UseHints
                            ? _powers[indexVariable.Value].Hint.Replace("<br>","\r\n")
                            :_powers[indexVariable.Value].Description.Replace("<br>","\r\n");
                            confirmButton.SetActive(true);
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

            fsm.AddState(new(fsm.Fsm) { Name = "Up Press" });
            fsm.AddState(new(fsm.Fsm) { Name = "Right Press" });
            fsm.AddState(new(fsm.Fsm) { Name = "Down Press" });
            fsm.AddState(new(fsm.Fsm) { Name = "Left Press" });
            fsm.AddState(new(fsm.Fsm) { Name = "Toggle Power" });

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
                if (indexVariable.Value == 9 || indexVariable.Value % 10 == 9 || indexVariable.Value == 52)
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
                if (indexVariable.Value <= 2)
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
                else if (indexVariable.Value >= 43)
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
                if (LoreMaster.Instance.ActivePowers.ContainsValue(_powers[indexVariable.Value]))
                {
                    Power power = _powers[indexVariable.Value];

                    if (power.Active)
                    {
                        power.DisablePower(false);
                        _loreObjects[indexVariable.Value].GetComponent<SpriteRenderer>().sprite = _notActive;
                        _loreObjects[indexVariable.Value].GetComponent<SpriteRenderer>().color = Color.red;
                        power.Tag = power.DefaultTag != PowerTag.Remove && power.DefaultTag != PowerTag.Disable
                        ? PowerTag.Disable
                        : power.DefaultTag;
                    }
                    else
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

            // Logic for disabling the page when the inventory was closed there.
            //PlayMakerFSM inventoryFSM = lorePage.transform.parent.gameObject.LocateMyFSM("Inventory Control");
            //inventoryFSM.GetState("Regain Control").ReplaceAction(new Lambda(() =>
            //{
            //    lorePage.SetActive(false);
            //    PlayerData.instance.SetBool("disablePause", false);
            //}), 6);

            //inventoryFSM.GetState("Regain Control 2").ReplaceAction(new Lambda(() =>
            //{
            //    lorePage.SetActive(false);
            //    PlayerData.instance.SetBool("disablePause", false);
            //}), 5);
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("An error occured in the inventory: " + exception.Message);
        }
    }

    #endregion
}
