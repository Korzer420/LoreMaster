using LoreMaster.Enums;
using LoreMaster.Helper;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

internal class TreasureHunterPower : Power
{
    #region Members

    private static Sprite[] _chartImages = new Sprite[14];

    #endregion

    #region Constructors

    public TreasureHunterPower() : base("Treasure Hunter", Area.CityOfTears)
    {
        
    }

    #endregion

    #region Properties

    /// <summary>
    /// Contains the flags to indicate, if the charts have been obtained.
    /// </summary>
    public static bool[] HasCharts { get; set; } = new bool[14];

    #endregion

    #region Control

    #endregion

    #region Inventory Screen

    public static void BuildInventory(GameObject treasureChartPage)
    {
        // If more then the cursor exists
        if (treasureChartPage.transform.childCount > 1)
            return;
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        ModHooks.SetPlayerBoolHook += ListenForCharts;
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
        }
        chartImage.GetComponent<SpriteRenderer>().sprite = _chartImages[0];
        

        treasureChartPage.SetActive(false);
    }

    private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (string.Equals(name, "hasTreasureCharts") || string.Equals(name, "lemm_allow"))
            return true;
        return orig;
    }

    private static bool ListenForCharts(string name, bool orig)
    {
        if (name.Contains("Treasure_Chart_"))
        {
            int number = Convert.ToInt32(new string(name.Skip("Treasure_Chart_".Length).ToArray()));
            HasCharts[number -1] = orig;
        }
        return orig;
    }

    #endregion
}
