using LoreMaster.Enums;
using LoreMaster.UnityComponents;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.Deepnest;

public class MaskOverchargePower : Power
{
    #region Members

    private GameObject _overcharge;

    private int _overchargeHealth = -1;

    private tk2dSprite[] _healthSprites = new tk2dSprite[3];

    #endregion

    #region Constructors

    public MaskOverchargePower() : base("Mask Overcharge", Area.Deepnest) { }

    #endregion

    #region Properties

    public GameObject Overcharge
    {
        get
        {
            if (_overcharge == null)
                Initialize();
            return _overcharge;
        }
    }

    public override PowerRank Rank => PowerRank.Greater;

    #endregion

    #region Event Handler

    /// <summary>
    /// Event handler when the hero updates.
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    private void HeroController_FixedUpdate(On.HeroController.orig_FixedUpdate orig, HeroController self)
    {
        orig(self);
        if (_overchargeHealth != -1)
            _overcharge.SetActive(PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) == _overchargeHealth);
    }

    private void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        _overcharge = new("Mask Overcharge");
        _overcharge.transform.SetParent(GameObject.Find("Knight").transform);
        _overcharge.transform.localPosition = new(0, 0);
        _overcharge.AddComponent<MaskCharge>();
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
    {
        _overcharge = new("Mask Overcharge");
        _overcharge.transform.SetParent(GameObject.Find("Knight").transform);
        _overcharge.transform.localPosition = new(0, 0);
        _overcharge.AddComponent<MaskCharge>();
        On.HeroController.FixedUpdate += HeroController_FixedUpdate;
        StartRoutine(() => SelectOverchargeHealth());
        On.HeroController.Start += HeroController_Start;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        if (_overcharge != null)
            GameObject.Destroy(_overcharge);
        On.HeroController.FixedUpdate -= HeroController_FixedUpdate;
        Overcharge.SetActive(false);
        // Reset mask color.
        foreach (tk2dSprite sprite in _healthSprites)
            sprite.color = Color.white;
        _overchargeHealth = -1;
        On.HeroController.Start -= HeroController_Start;
    }

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        StartRoutine(() => SelectOverchargeHealth());
        LoreMaster.Instance.Handler.StartCoroutine(NotOvercharged());
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Select a mask which should be overcharged.
    /// </summary>
    private IEnumerator SelectOverchargeHealth()
    {
        yield return new WaitForSeconds(1f);
        GameObject parent = GameObject.Find("_GameCameras/HudCamera/Hud Canvas/Health");
        Color[] colors = new Color[] { Color.yellow, Color.blue, Color.red, Color.cyan, Color.green, new(1f, 0.4f, 0f), new(1f, 0f, 1f), Color.black };
        bool firstTime = true;

        while (true)
        {
            // We need to wait here, because otherwise this would freeze the game, if we have joni's and/or less then 2 hp.
            yield return null;
            if (_overcharge == null)
                Initialize();
            // Reset sprites
            _overchargeHealth = -1;
            if (!firstTime)
                foreach (tk2dSprite sprite in _healthSprites)
                    sprite.color = Color.white;
            firstTime = false;

            int playerMaxHealth = PlayerData.instance.GetInt(nameof(PlayerData.instance.maxHealth));
            if (PlayerData.instance.GetBool("equippedCharm_27") || playerMaxHealth < 2)
                continue;

            // This excludes full health (we don't want to overcharge the last mask)
            _overchargeHealth = Random.Range(1, playerMaxHealth);

            // We honestly probably don't need to use transform find here, but in case the hud is unavailable we account for that, I guess.
            GameObject health = parent.transform.Find("Health " + _overchargeHealth).gameObject;
            _healthSprites[0] = health.GetComponent<tk2dSprite>();
            _healthSprites[1] = health.transform.Find("Idle").GetComponent<tk2dSprite>();
            _healthSprites[2] = health.transform.Find("Idle Hive").GetComponent<tk2dSprite>();

            float passedTime = 0f;
            while (passedTime < 30f)
            {
                yield return new WaitForSeconds(.25f);
                passedTime += .25f;
                Color rolledColor = colors[Random.Range(0, 7)];
                foreach (tk2dSprite sprite in _healthSprites)
                    sprite.color = rolledColor;
            }
        }
    }

    /// <summary>
    /// If the player is not at the overcharged health they loose soul quickly.
    /// </summary>
    private IEnumerator NotOvercharged()
    {
        float passedTime = 0f;
        while(State == PowerState.Twisted)
        {
            passedTime += Time.deltaTime;
            if (passedTime >= 1f)
            {
                passedTime = 0f;
                if (PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) != _overchargeHealth)
                    HeroController.instance.TakeMP(2);
            }
            yield return null;
        }
    }

    #endregion
}
