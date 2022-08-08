using LoreMaster.Enums;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes;

public class GloryOfTheWealthPower : Power
{
    #region Members

    private Dictionary<string, int[]> _enemyGeoValues = new();
    private GameObject _payObject;
    private bool _active;

    #endregion

    #region Constructors

    public GloryOfTheWealthPower() : base("Glory of the Wealth", Area.FungalWastes)
    {
        Hint = "Your enemies may \"share\" more of their wealth with you. Your wealth may protect you from the hits of those peasant... for a fee of course. Press Quick Map + Up to activate the effect" +
            " and Quick Map + Down to cancel the effect. (In case you forget this input combination, the effect also deactivates itself once the leave the room). " +
            "The cost increase fast over time while the effect is active and slowly decrease once inactive. If you are really greedy, the cost decrease faster. The great civilians of Hallownest call this " +
            "power \"Pay 2 Win\".";
        Description = "Enemies drop double geo. Press Quick Map + Up to become invincible. For each second this is active it drains 1 more geo. Press Quick Map + Down to cancel the effect (or leave the room). " +
            "While the effect is inactive the cost decrease by 1 each 5 second (or each 2.5 seconds if you have Fragile/Unbreakable Greed equipped).";
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public override Action SceneAction => () =>
    {
        HealthManager[] enemies = GameObject.FindObjectsOfType<HealthManager>();

        foreach (HealthManager enemy in enemies)
        {
            // Get the enemy name. We need to use a regex to prevent flouding the dictionary with reduntant data. For example: If enemy is called Crawler 1, the entry for "Crawler" doesn't work.
            string enemyName = Regex.Match(enemy.name, @"^[^0-9]*").Value.Trim();

            // Check if we already have registered the enemy type. This action takes a lot of loading time, therefore we want to avoid it, as much as we can.
            if (!_enemyGeoValues.ContainsKey(enemyName))
            {
                int[] geoValues = new int[3];

                geoValues[0] = ReflectionHelper.GetField<HealthManager, int>(enemy, "smallGeoDrops");
                geoValues[1] = ReflectionHelper.GetField<HealthManager, int>(enemy, "mediumGeoDrops");
                geoValues[2] = ReflectionHelper.GetField<HealthManager, int>(enemy, "largeGeoDrops");

                _enemyGeoValues.Add(enemyName, geoValues);
            }

            int[] geoDrops = _enemyGeoValues[enemyName];
            // We only increase if it would drop geo anyway
            if (geoDrops.Any(x => x != 0))
            {
                enemy.SetGeoSmall(geoDrops[0] * 2);
                enemy.SetGeoMedium(geoDrops[1] * 2);
                enemy.SetGeoLarge(geoDrops[2] * 2);
            }
        }
        _active = false;
    };

    /// <summary>
    /// Gets the time it takes to decrease the geo cost of the invinciblity.
    /// </summary>
    public float DeflationTime => PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_21)) ? 2.5f : 5f;

    /// <summary>
    /// Gets the cost of the glory effect.
    /// </summary>
    public static int GloryCost { get; set; } = 0;

    #endregion

    #region Event handler

    private void ModHooks_HeroUpdateHook()
    {
        if (!_active && InputHandler.Instance.inputActions.quickMap.IsPressed && InputHandler.Instance.inputActions.up
            && PlayerData.instance.GetInt(nameof(PlayerData.instance.geo)) >= GloryCost)
            _active = true;

        if (_active && InputHandler.Instance.inputActions.quickMap.IsPressed && InputHandler.Instance.inputActions.down)
            _active = false;
    }

    private bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name.Equals(nameof(PlayerData.instance.isInvincible)))
            return orig || _active;
        return orig;
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        GameObject prefab = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Inv_Items/Geo").gameObject;
        GameObject hudCanvas = GameObject.Find("_GameCameras").transform.Find("HudCamera/Hud Canvas").gameObject;
        _payObject = GameObject.Instantiate(prefab, hudCanvas.transform, true);
        _payObject.transform.localPosition = new(-2.06f, -2.32f, 0f);
        _payObject.transform.localScale = new(1f, 1f, 1f);
        _payObject.GetComponent<DisplayItemAmount>().playerDataInt = _payObject.name;
        _payObject.GetComponent<DisplayItemAmount>().textObject.text = "(-) 0";
        _payObject.GetComponent<DisplayItemAmount>().textObject.color = new(1f, 0f, 1f);
        _payObject.GetComponent<DisplayItemAmount>().textObject.fontSize = 2.25f;
        _payObject.GetComponent<DisplayItemAmount>().textObject.gameObject.name = "Penalty";
        _payObject.GetComponent<SpriteRenderer>().color = Color.red;
        _payObject.GetComponent<BoxCollider2D>().size = new Vector2(1.5f, 1f);
        _payObject.GetComponent<BoxCollider2D>().offset = new Vector2(0.5f, 0f);
        _payObject.transform.GetChild(0).localPosition = new(1.5f, -.95f, 0f);
        _payObject.transform.GetChild(0).localScale = new(2f, 2f, 1f);
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        ModHooks.HeroUpdateHook += ModHooks_HeroUpdateHook;
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(Negotiation());
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ModHooks.HeroUpdateHook -= ModHooks_HeroUpdateHook;
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        _active = false;
        _payObject.SetActive(false);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Grants immunity, but drains geo rapidly.
    /// </summary>
    private IEnumerator Negotiation()
    {
        _payObject.SetActive(true);
        float passedTime = 0f;
        while (true)
        {
            // Even though this might seem useless, I'm not the biggest fan of setting the gameobject state each frame, which is why we only do it once when the effect is active.
            if(GloryCost > 0 || _active)
                _payObject.SetActive(true);
            while (GloryCost > 0 || _active)
            {
                
                passedTime += Time.deltaTime;
                if (_active && !PlayerData.instance.GetBool(nameof(PlayerData.instance.atBench)) && PlayerData.instance.GetInt(nameof(PlayerData.instance.geo)) >= GloryCost)
                {
                    if (passedTime >= 1f)
                    {
                        GloryCost++;
                        HeroController.instance.TakeGeo(GloryCost);
                        _payObject.GetComponent<DisplayItemAmount>().textObject.text = "- " + GloryCost;
                        _payObject.GetComponent<DisplayItemAmount>().textObject.color = new(1f, 0f, 1f);
                        passedTime = 0f;
                    }
                }
                else if(GloryCost > 0)
                {
                    _active = false;
                    if (passedTime >= DeflationTime)
                    {
                        GloryCost--;
                        _payObject.GetComponent<DisplayItemAmount>().textObject.text = "- " + GloryCost;
                        _payObject.GetComponent<DisplayItemAmount>().textObject.color = new(0f, .75f, 0f);
                        passedTime = 0f;
                    }
                }
                yield return null;
            }
            _active = false;
            _payObject.SetActive(false);
            yield return null;
        }
    }

    #endregion
}
