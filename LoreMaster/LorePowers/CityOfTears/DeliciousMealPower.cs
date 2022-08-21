using LoreMaster.Enums;
using Modding;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

public class DeliciousMealPower : Power
{
    #region Members

    private bool _selectedEgg;
    private bool _saturation;
    private GameObject _eggObject;

    #endregion

    #region Constructors

    public DeliciousMealPower() : base("Delicious Meal", Area.CityOfTears) { }

    #endregion

    #region Event Handler

    private string CheckForEgg(string key, string sheetTitle, string orig)
    {
        // This assumes, that the game only asks for this key, when the cursor moves to the rancid egg in the inventory.
        _selectedEgg = key.Equals("INV_DESC_RANCIDEGG");
        return orig;
    }

    private void ConsumeEgg()
    {
        if (!_saturation && InputHandler.Instance.inputActions.jump)
            EatEgg();
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        _eggObject = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Equipment/Rancid Egg").gameObject;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        ModHooks.HeroUpdateHook += ConsumeEgg;
        ModHooks.LanguageGetHook += CheckForEgg;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ModHooks.HeroUpdateHook -= ConsumeEgg;
        ModHooks.LanguageGetHook -= CheckForEgg;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Consumes the egg and start the saturation effect.
    /// </summary>
    private void EatEgg()
    {
        // The grand parent is the inventory
        if (_saturation || !_selectedEgg || !_eggObject.activeSelf || !_eggObject.transform.parent.parent.gameObject.activeSelf)
        {
            _selectedEgg = false;
            return;
        }
        if (PlayerData.instance.GetInt(nameof(PlayerData.instance.rancidEggs)) == 0)
            return;
        PlayerData.instance.SetInt(nameof(PlayerData.instance.rancidEggs), PlayerData.instance.GetInt(nameof(PlayerData.instance.rancidEggs)) - 1);
        _eggObject.GetComponentInChildren<TextMeshPro>().text = PlayerData.instance.GetInt(nameof(PlayerData.instance.rancidEggs)).ToString();
        _saturation = true;
        _selectedEgg = false;
        LoreMaster.Instance.Handler.StartCoroutine(Saturation());
    }

    /// <summary>
    /// The saturation event duration.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Saturation()
    {
        ModHooks.GetPlayerIntHook += ModHooks_GetPlayerIntHook;
        yield return null;
        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
        float passedTime = 0f;
        float healTimer = 0f;
        float blinkTime = 0f;
        bool isOrange = true;
        SpriteRenderer[] vesselElements = GameObject.Find("_GameCameras").transform.Find("HudCamera/Hud Canvas/Soul Orb").gameObject.GetComponentsInChildren<SpriteRenderer>(true);
        tk2dSprite[]  vesselSprites = GameObject.Find("_GameCameras").transform.Find("HudCamera/Hud Canvas/Soul Orb").gameObject.GetComponentsInChildren<tk2dSprite>(true);
        foreach (SpriteRenderer renderer in vesselElements)
            renderer.color = new(1f, 0.4f, 0f);
        foreach (tk2dSprite renderer in vesselSprites)
            renderer.color = new(1f, 0.4f, 0f);
        while (passedTime < 180f)
        {
            passedTime += Time.deltaTime;
            healTimer += Time.deltaTime;
            if(healTimer >= 8f)
            {
                healTimer = 0f;
                if (PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) < PlayerData.instance.GetInt(nameof(PlayerData.instance.maxHealth)))
                    HeroController.instance.AddHealth(1);
            }
            if(passedTime >= 165f)
            {
                blinkTime += Time.deltaTime;
                float blinkInterval = passedTime >= 175 ? .25f : .5f;
                if(blinkTime >= blinkInterval)
                {
                    blinkTime = 0f;
                    Color swapTo = isOrange ? Color.white : new(1f, 0.4f, 0f);
                    isOrange = !isOrange;
                    foreach (SpriteRenderer renderer in vesselElements)
                        renderer.color = swapTo;
                    foreach (tk2dSprite renderer in vesselSprites)
                        renderer.color = swapTo;
                }
            }
            yield return null;
        }
        foreach (SpriteRenderer renderer in vesselElements)
            renderer.color = Color.white;
        foreach (tk2dSprite renderer in vesselSprites)
            renderer.color = Color.white;
        _saturation = false;
        ModHooks.GetPlayerIntHook -= ModHooks_GetPlayerIntHook;
        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    private int ModHooks_GetPlayerIntHook(string name, int damage)
    {
        if (string.Equals(name, "nailDamage"))
            damage = Convert.ToInt32(damage * 1.2f);
        return damage;
    }

    #endregion
}
