using LoreMaster.Enums;
using LoreMaster.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LoreMaster.LorePowers.Crossroads;

public class GreaterMindPower : Power
{
    #region Members

    private Sprite _loreSprite;

    private GameObject _loreTracker;

    private Dictionary<Area, string> _areas = new()
    {
        {Area.None, "Menu" },
        {Area.AncientBasin, "Ancient Basin" },
        {Area.CityOfTears, "City of Tears" },
        {Area.Dirtmouth, "Dirtmouth"},
        {Area.Crossroads, "Crossroads"},
        {Area.Greenpath, "Greenpath"},
        {Area.Deepnest, "Deepnest" },
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
        _loreSprite = SpriteHelper.CreateSprite("Lore");
    }

    #endregion

    /// <summary>
    /// Desperate attempt to make the tracker work, if it doesn't work before...
    /// </summary>
    public override Action SceneAction => () => 
    {
        try
        {
            if (_loreTracker == null)
                Initialize();
        }
        catch (Exception)
        { }
    };

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        GameObject prefab = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Inv_Items/Geo").gameObject;
        GameObject hudCanvas = GameObject.Find("_GameCameras").transform.Find("HudCamera/Hud Canvas").gameObject;
        _loreTracker = GameObject.Instantiate(prefab, hudCanvas.transform, true);
        _loreTracker.name = "Lore Tracker";
        PositionHudElement(_loreTracker, 3);
    }

    /// <inheritdoc/>
    protected override void Enable() 
    {
        if(_loreTracker != null)
        {
            _loreTracker.SetActive(true);
            UpdateLoreCounter(LoreMaster.Instance.ActivePowers.Values, LoreMaster.Instance.AllPowers.Values, LoreMaster.Instance.CurrentArea, LoreMaster.Instance.IsAreaGlobal(LoreMaster.Instance.CurrentArea));
        }
    }

    /// <inheritdoc/>
    protected override void Disable() => _loreTracker?.SetActive(false);

    #endregion

    #region Private Methods

    private void PositionHudElement(GameObject go, int fontSize)
    {
        try
        {
            go.transform.localPosition = new(-3.66f, -4.32f, 0f);
            go.transform.localScale = new(1.3824f, 1.3824f, 1.3824f);
            go.GetComponent<DisplayItemAmount>().playerDataInt = go.name;
            go.GetComponent<DisplayItemAmount>().textObject.text = "";
            go.GetComponent<DisplayItemAmount>().textObject.fontSize = fontSize;
            go.GetComponent<DisplayItemAmount>().textObject.gameObject.name = "Counter";
            go.GetComponent<SpriteRenderer>().sprite = _loreSprite;
            go.GetComponent<BoxCollider2D>().size = new Vector2(1.5f, 1f);
            go.GetComponent<BoxCollider2D>().offset = new Vector2(0.5f, 0f);
        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error in Position Hud: " + exception.Message);
        }
    }

    public void UpdateLoreCounter(IEnumerable<Power> activePowers, IEnumerable<Power> allPowers, Area currentArea, bool globalActive)
    {
        if (_loreTracker == null)
        {
            LoreMaster.Instance.LogError("The lore tracker doesn't exist");
            return;
        }
        try
        {
            TextMeshPro currentCounter = _loreTracker.GetComponent<DisplayItemAmount>().textObject;
            currentCounter.text = _areas[currentArea] + ": " + activePowers.Count(x => x.Location == currentArea && x.Tag != PowerTag.Remove);
            currentCounter.text += "/" + allPowers.Count(x => x.Location == currentArea && x.Tag != PowerTag.Remove);
            if (globalActive)
                currentCounter.text = "<color=#7FFF7B>" + currentCounter.text + "</color>";

            string globalPart = "All: " + activePowers.Count(x => x.Tag != PowerTag.Remove) + "/" + allPowers.Count(x => x.Tag != PowerTag.Remove);
            if (activePowers.Count(x => x.Tag != PowerTag.Remove) == allPowers.Count(x => x.Tag != PowerTag.Remove))
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
