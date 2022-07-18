using LoreMaster.Enums;
using LoreMaster.UnityComponents;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.Deepnest;

/* Mask elements:
         * Under _GameCameras/HudCamera/Hud Canvas/Health
         * Health -> Background
         * Idle -> Normal health
         * Idle Hive -> Hive blood health
         * Idle Bound -> Health Binding GG DLC (Needs to be considered)
         * Hive Recovery Blob -> Hive Blood Recover Sprite
         */
// Maske leuchtet noch
public class MaskOverchargePower : Power
{
    #region Members

    private GameObject _overcharge;

    private int _overchargeHealth = -1;

    private tk2dSprite[] _healthSprites = new tk2dSprite[4]; 

    #endregion

    #region Constructors

    public MaskOverchargePower() : base("Mask Overcharge", Area.Deepnest)
    {
        Hint = "Let one your mask occasionly overcharge. If it is the one that protects you, it emits a searing circle, that also absorbs the loose soul around you.";
        Description = "Overcharge one of your mask (it glows in different colors), while you have exactly that much health, a circle gathers around you that deal damage and restore 8 soul each second. "+
            "The overcharged mask changes every 30 seconds and may never select the full hp mask. Inactive while you have Joni's Blessing equipped.";
    }

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

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        _overcharge = new();
        _overcharge.transform.SetParent(GameObject.Find("Knight").transform);
        _overcharge.transform.localPosition = new(0, 0);
        _overcharge.name = "Mask Overcharge";
        _overcharge.AddComponent<MaskCharge>();
    }

    protected override void Enable()
    {
        if (_overcharge == null)
            Initialize();
        On.HeroController.FixedUpdate += HeroController_FixedUpdate;
         _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(SelectOverchargeHealth());
    }

    protected override void Disable()
    {
        On.HeroController.FixedUpdate -= HeroController_FixedUpdate;
        LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
        _overcharge.SetActive(false);
        // Reset mask color.
        foreach (tk2dSprite sprite in _healthSprites)
            sprite.color = Color.white;
        _overchargeHealth = -1;
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
        _healthSprites[3] = parent.transform.Find("Hive Recovery Blob").GetComponent<tk2dSprite>();

        while (true)
        {
            // We need to wait here, because otherwise this would freeze the game, if we have joni's and/or less then 2 hp.
            yield return null;
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

    #endregion
}
