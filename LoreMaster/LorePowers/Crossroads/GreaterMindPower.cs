using LoreMaster.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LoreMaster.LorePowers.Crossroads;

public class GreaterMindPower : Power
{
    #region Members

    private Sprite _loreSprite;

    private GameObject _localCounter;

    private Dictionary<Area, string> _areas = new()
    {
        {Area.AncientBasin, "Ancient Basin" },
        {Area.CityOfTears, "City of Tears" },
        {Area.Dirtmouth, "Dirtmouth"},
        {Area.Crossroads, "Crossroads"},
        {Area.Greenpath, "Greenpath"},
        {Area.FungalWastes, "Fungal Wastes"},
        {Area.QueensGarden, "Queens Gardens"},
        {Area.Peaks, "Crystal Peaks"},
        {Area.RestingGrounds, "Resting Grounds"},
        {Area.WaterWays, "Waterways"},
        {Area.KingdomsEdge, "Kingdom's Edge"},
        {Area.FogCanyon, "Fog Canyon"},
        {Area.Cliffs, "Howling Cliffs"},
        {Area.WhitePalace, "White Palace"},
    };

    #endregion

    #region Constructors

    public GreaterMindPower() : base("Greater Mind", Area.Crossroads)
    {
        Hint = "You can now sense, which knowledge of the world you're missing.";
        Description = "Activates the tracker, to show you how many powers you are missing. If the counter is green, you have global access to the power in the area.";
        string imageFile = Path.Combine(Path.GetDirectoryName(typeof(LoreMaster).Assembly.Location), "Resources/Lore.png");
        byte[] imageData = File.ReadAllBytes(imageFile);
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        ImageConversion.LoadImage(tex, imageData, true);
        tex.filterMode = FilterMode.Bilinear;
        _loreSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
    }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        GameObject prefab = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Inv_Items/Geo").gameObject;
        GameObject hudCanvas = GameObject.Find("_GameCameras").transform.Find("HudCamera/Hud Canvas").gameObject;
        _localCounter = GameObject.Instantiate(prefab, hudCanvas.transform, true);
        _localCounter.name = "Local Lore";
        PositionHudElement(_localCounter, new Vector3(-2.6f, 8.05f), 3);
    }

    protected override void Enable()
    {
        _localCounter.SetActive(true);
    }

    protected override void Disable()
    {
        _localCounter.SetActive(false);
    }

    #endregion

    #region Private Methods

    private void PositionHudElement(GameObject go, Vector3 positionToAdd, int fontSize)
    {
        go.transform.position += positionToAdd;
        go.GetComponent<DisplayItemAmount>().playerDataInt = go.name;
        go.GetComponent<DisplayItemAmount>().textObject.text = "";
        go.GetComponent<DisplayItemAmount>().textObject.fontSize = fontSize;
        go.GetComponent<SpriteRenderer>().sprite = _loreSprite;
        go.GetComponent<BoxCollider2D>().size = new Vector2(1.5f, 1f);
        go.GetComponent<BoxCollider2D>().offset = new Vector2(0.5f, 0f);
    }

    public void UpdateLoreCounter(IEnumerable<Power> activePowers, IEnumerable<Power> allPowers, Area currentArea, bool globalActive)
    {
        try
        {
            TextMeshPro currentCounter = _localCounter.GetComponent<DisplayItemAmount>().textObject;
            currentCounter.text = _areas[currentArea] + ": " + activePowers.Count(x => x.Location == currentArea && x.Tag != PowerTag.Removed);
            currentCounter.text += "/" + allPowers.Count(x => x.Location == currentArea && x.Tag != PowerTag.Removed);
            if (globalActive)
                currentCounter.text = "<color=#7FFF7B>" + currentCounter.text + "</color>";

            string globalPart = "All: "+ activePowers.Count(x => x.Tag != PowerTag.Removed) + "/" + allPowers.Count(x => x.Tag != PowerTag.Removed);
            if (activePowers.Count(x => x.Tag != PowerTag.Removed) == allPowers.Count(x => x.Tag != PowerTag.Removed))
                globalPart = "<color=#7FFF7B>" + globalPart + "</color>";

            currentCounter.text += Environment.NewLine + globalPart;
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error while loading counter: " + exception.Message);
            LoreMaster.Instance.LogError("Error while loading counter: " + exception.StackTrace);
        }
    }

    #endregion
}
