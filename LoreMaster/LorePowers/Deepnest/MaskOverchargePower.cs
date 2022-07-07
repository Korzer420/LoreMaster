using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.Deepnest;

public class MaskOverchargePower : Power
{
    #region Members

    private GameObject _overcharge;

    private int _overchargeHealth = -1;

    #endregion

    #region Constructors

    public MaskOverchargePower() : base("Mask Overcharge Power", Area.Deepnest)
    {

        /* Mask elements:
         * Under _GameCameras/HudCamera/Hud Canvas/Health
         * Health -> Background
         * Idle -> Normal health
         * Idle Hive -> Hive blood health
         * Idle Bound -> Health Binding GG DLC
         * Hive Recovery Blob -> Hive Blood Recover Sprite
         */
    }

    #endregion

    #region Public Methods

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
        On.HeroController.FixedUpdate += HeroController_FixedUpdate;
        LoreMaster.Instance.Handler.StartCoroutine(SelectOverchargeHealth());
    }

    private void HeroController_FixedUpdate(On.HeroController.orig_FixedUpdate orig, HeroController self)
    {
        orig(self);
        if (_overchargeHealth != -1)
            _overcharge.SetActive(PlayerData.instance.GetInt(nameof(PlayerData.instance.health)) == _overchargeHealth);
    }

    protected override void Disable()
    {
        On.HeroController.FixedUpdate -= HeroController_FixedUpdate;
        HeroController.instance.StopCoroutine(SelectOverchargeHealth());
        _overcharge.SetActive(false);
        _overchargeHealth = -1;
    }

    #endregion

    #region Private Methods

    private IEnumerator SelectOverchargeHealth()
    {
        yield return new WaitForSeconds(1f);
        GameObject parent = GameObject.Find("_GameCameras/HudCamera/Hud Canvas/Health");
        tk2dSprite[] sprites = new tk2dSprite[4];
        Color[] colors = new Color[] { Color.yellow, Color.blue, Color.red, Color.cyan, Color.green, new(1f, 0.4f, 0f), new(1f, 0f, 1f), Color.black };
        bool firstTime = true;
        try
        {
            sprites[3] = parent.transform.Find("Hive Recovery Blob").GetComponent<tk2dSprite>();

        }
        catch (Exception exception)
        {
            LoreMaster.Instance.LogError("Error outside while loop " + exception.Message);
        }

        while (true)
        {
            // Reset sprites
            try
            {
                _overchargeHealth = -1;
                if (!firstTime)
                    foreach (tk2dSprite sprite in sprites)
                        sprite.color = Color.white;
                firstTime = false;

                int playerMaxHealth = PlayerData.instance.GetInt(nameof(PlayerData.instance.maxHealth));
                if (PlayerData.instance.GetBool("equippedCharm_27") || playerMaxHealth < 2)
                    continue;

                // This excludes full health (we don't want to overcharge the last mask)
                _overchargeHealth = UnityEngine.Random.Range(1, playerMaxHealth);

                // We honestly probably don't need to use transform find here, but in case the hud is unavailable we account for that, I guess.
                GameObject health = parent.transform.Find("Health " + _overchargeHealth).gameObject;
                sprites[0] = health.GetComponent<tk2dSprite>();
                sprites[1] = health.transform.Find("Idle").GetComponent<tk2dSprite>();
                sprites[2] = health.transform.Find("Idle Hive").GetComponent<tk2dSprite>();
            }
            catch (Exception exception)
            {
                LoreMaster.Instance.LogError("Error inside while loop: " + exception.Message);
            }

            float passedTime = 0f;
            while (passedTime < 30f)
            {
                yield return new WaitForSeconds(.25f);
                try
                {
                    passedTime += .25f;
                    Color rolledColor = colors[UnityEngine.Random.Range(0, 7)];
                    foreach (tk2dSprite sprite in sprites)
                        sprite.color = rolledColor;
                }
                catch (Exception exception)
                {
                    LoreMaster.Instance.LogError("Error inside second while loop: " + exception.Message);
                }
            }
        }
    }

    #endregion
}

public class MaskCharge : MonoBehaviour
{
    private GameObject _rune;

    private GameObject _hitbox;

    private void Start()
    {
        _hitbox = new("Hitbox");
        _hitbox.transform.SetParent(transform);
        _hitbox.transform.localPosition = new(0, 0);
        foreach (Transform thorn in transform.parent.Find("Charm Effects/Thorn Hit"))
        {
            GameObject hitbox = GameObject.Instantiate(thorn.gameObject, _hitbox.transform);
            hitbox.transform.localScale = new(1.2f, 1.2f);

            PlayMakerFSM damageFSM = hitbox.LocateMyFSM("damages_enemy");
            // Remove the knockback
            damageFSM.FsmVariables.FindFsmFloat("magnitudeMult").Value = 0f;
        }

        GameObject ring = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Battle Scene/HK Prime/Focus Blast/focus_ring"], transform);
        ring.name = "Charge Ring";
        ring.SetActive(true);
        ring.transform.localPosition = new(0, 0, 0);
        ring.transform.localScale = new(1.8f, 1.8f, 1.8f);

        _rune = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Battle Scene/HK Prime/Focus Blast/focus_rune"], transform);
        _rune.name = "Charge Rune";
        _rune.SetActive(true);
        _rune.transform.localPosition = new(0, 0, 0);
        _rune.transform.localScale = new(1.8f, 1.8f, 1.8f);

    }

    private void OnEnable()
    {
        LoreMaster.Instance.Log("Enable Ring");
        StartCoroutine(RotateRune());
        StartCoroutine(EmitDamage());
    }

    private void OnDisable()
    {
        LoreMaster.Instance.Log("Disable Ring");
        StopAllCoroutines();
    }

    private IEnumerator RotateRune()
    {
        int currentRotation = 0;
        while (true)
        {
            yield return new WaitForSeconds(.02f);
            currentRotation += 2;
            if (currentRotation >= 360)
                currentRotation = 0;
            _rune.transform.SetRotationZ(currentRotation);
        }
    }

    private IEnumerator EmitDamage()
    {
        while (true)
        {
            yield return new WaitForSeconds(.2f);
            _hitbox.SetActive(true);
            HeroController.instance.AddMPCharge(2);
            yield return null;
            _hitbox.SetActive(false);
        }
    }


}
