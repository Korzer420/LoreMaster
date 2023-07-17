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

    private GameObject _poggy;
    private float _hungerTimer = 120f;

    #endregion

    #region Constructors

    public DeliciousMealPower() : base("Delicious Meal", Area.CityOfTears) { }

    #endregion

    #region Properties

    public GameObject EggObject => _eggObject == null ? _eggObject = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Equipment/Rancid Egg").gameObject : _eggObject;

    #endregion

    #region Event Handler

    private string CheckForEgg(string key, string sheetTitle, string orig)
    {
        // This assumes, that the game only asks for this key, when the cursor moves to the rancid egg in the inventory.
        if (key == "INV_DESC_RANCIDEGG")
        {
            _selectedEgg = true;
        }
        else
            _selectedEgg = false;
        return orig;
    }

    private void ConsumeEgg()
    {
        if (!_saturation && InputHandler.Instance.inputActions.jump)
            EatEgg();
    }

    private int ModHooks_GetPlayerIntHook(string name, int damage)
    {
        if (string.Equals(name, "nailDamage"))
            damage = Convert.ToInt32(damage * 1.2f);
        return damage;
    }

    #endregion

    #region Control

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

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        ModHooks.HeroUpdateHook += ConsumeEgg;
        ModHooks.LanguageGetHook += CheckForEgg;
        _poggy = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Ghost NPC"].transform.Find("Character Sprite").gameObject);
        GameObject.DontDestroyOnLoad(_poggy);
        _poggy.SetActive(true);
        Component.Destroy(_poggy.GetComponent<PlayMakerFSM>());
        Component.Destroy(_poggy.GetComponent<PlayMakerFSM>());
        StartRoutine(() => RumblingStomach());
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        ModHooks.HeroUpdateHook -= ConsumeEgg;
        ModHooks.LanguageGetHook -= CheckForEgg;
        GameObject.Destroy(_poggy);
        _poggy = null;
        _hungerTimer = 120f;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Consumes the egg and start the saturation effect.
    /// </summary>
    private void EatEgg()
    {
        // The grand parent is the inventory
        if (_saturation || !_selectedEgg || !EggObject.activeSelf || !EggObject.transform.parent.parent.gameObject.activeSelf)
        {
            _selectedEgg = false;
            return;
        }
        if (PlayerData.instance.GetInt(nameof(PlayerData.instance.rancidEggs)) == 0)
            return;
        PlayerData.instance.SetInt(nameof(PlayerData.instance.rancidEggs), PlayerData.instance.GetInt(nameof(PlayerData.instance.rancidEggs)) - 1);
        LoreMaster.Instance.Handler.StartCoroutine(UpdateUI());
        _saturation = true;
        _selectedEgg = false;
        if (State == PowerState.Twisted)
        {
            _hungerTimer = 120f;
            return;
        }
        
        LoreMaster.Instance.Handler.StartCoroutine(Saturation());
    }

    /// <summary>
    /// The saturation event duration.
    /// </summary>
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

    private IEnumerator UpdateUI()
    {
        TextMeshPro rancidEggCount = EggObject.GetComponentInChildren<TextMeshPro>();
        rancidEggCount.text = PlayerData.instance.GetInt(nameof(PlayerData.instance.rancidEggs)).ToString();
        float size = 2f;
        while (size > 1f)
        {
            size -= Time.deltaTime * 2;
            rancidEggCount.transform.localScale = new(size, size, size);
            rancidEggCount.color = new Color(size - 1f, 2f - size, 2f - size);
            yield return null;
        }
        if (State == PowerState.Twisted)
            _saturation = false;
        // Just in case we set the values normally at the end
        rancidEggCount.transform.localScale = new(1f, 1f, 1f);
        rancidEggCount.color = Color.white;
    }

    private IEnumerator RumblingStomach()
    {
        _hungerTimer = 120f;
        tk2dSprite poggySprite = _poggy.GetComponent<tk2dSprite>();
        poggySprite.color = Color.white;
        // This should render the ghost in front of every object.
        poggySprite.SortingOrder = 1;
        _poggy.transform.localScale = new(-1.08f, 1.08f, 1.08f);
        while (true)
        {
            _hungerTimer -= Time.deltaTime;
            poggySprite.color = new Color(1f, _hungerTimer / 120, _hungerTimer / 120);
            _poggy.transform.position = HeroController.instance.transform.position - new Vector3(_hungerTimer / 120 * 12, 2 - (_hungerTimer / 30), 0f);
            if (!HeroController.instance.acceptingInput || PlayerData.instance.GetBool("atBench"))
                yield return new WaitUntil(() => HeroController.instance.acceptingInput && !PlayerData.instance.GetBool("atBench"));
            if (_hungerTimer <= 0f)
            {
                _hungerTimer = 120f;
                HeroController.instance.TakeDamage(null, GlobalEnums.CollisionSide.left, 99, 1);
                yield return new WaitForSeconds(4f);
            }
            yield return null;
        }
    }

    #endregion
}
