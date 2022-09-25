using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

internal class PureSpiritPower : Power
{
    #region Members

    private GameObject _orbContainer;

    private Transform _followed;

    private GameObject _fireball;

    private GameObject _hitbox;

    private bool _canThrow;

    #endregion

    #region Constructor

    public PureSpiritPower() : base("Pure Spirit", Area.CityOfTears) { }

    #endregion

    #region Properties

    public GameObject Hitbox => _hitbox == null ? _hitbox = HeroController.instance.transform.Find("Charm Effects/Thorn Hit/Hit D").gameObject : _hitbox;

    #endregion

    #region Event handler

    private void SetGravity2dScale_OnEnter(On.HutongGames.PlayMaker.Actions.SetGravity2dScale.orig_OnEnter orig, SetGravity2dScale self)
    {
        orig(self);
        if (self.IsCorrectContext("Spell Control", "Knight", "Spell End"))
        {
            if (State == PowerState.Active)
            {
                if (Active && PlayerData.instance.GetInt(nameof(PlayerData.instance.MPCharge)) == 0
                    && _orbContainer.transform.childCount < 3 && _followed == HeroController.instance.transform)
                    SpawnOrb();
                LoreMaster.Instance.Handler.StartCoroutine(WaitForSpell());
            }
            else if (State == PowerState.Twisted)
            {
                HeroController.instance.TakeReserveMP(100);
                HeroController.instance.TakeMP(100);
            }
        }

    }

    private void RecycleSelf_OnEnter(On.HutongGames.PlayMaker.Actions.RecycleSelf.orig_OnEnter orig, RecycleSelf self)
    {
        if (self.IsCorrectContext("Fireball Control", null, null))
            if (_followed == self.Fsm.FsmComponent.transform)
            {
                _followed = null;
                _canThrow = false;
                _fireball = null;
            }
        orig(self);
    }

    private void PlayerDataBoolTest_OnEnter(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, PlayerDataBoolTest self)
    {
        orig(self);
        if (self.IsCorrectContext("Fireball Control", null, "Set Damage"))
            if (Active && _canThrow)
                _followed = self.Fsm.FsmComponent.transform;
            else
                _fireball = self.Fsm.FsmComponent.gameObject;
    }

    #endregion

    #region Control

    protected override void Initialize()
    {
        if (_orbContainer == null)
        {
            _orbContainer = new("Focus Orb Holder");
            GameObject.DontDestroyOnLoad(_orbContainer);
            _orbContainer.transform.localScale = new(1f, 1f, 1f);
        }
        On.HutongGames.PlayMaker.Actions.SetGravity2dScale.OnEnter += SetGravity2dScale_OnEnter;
        
    }

    protected override void Terminate()
    {
        if (_orbContainer != null)
            GameObject.Destroy(_orbContainer);
        On.HutongGames.PlayMaker.Actions.SetGravity2dScale.OnEnter -= SetGravity2dScale_OnEnter;

    }

    protected override void Enable()
    {
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(FollowHero());
        On.HutongGames.PlayMaker.Actions.RecycleSelf.OnEnter += RecycleSelf_OnEnter;
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += PlayerDataBoolTest_OnEnter;
    }

    protected override void Disable()
    {
        foreach (Transform child in _orbContainer.transform)
            GameObject.Destroy(child.gameObject);
        _canThrow = false;
        _followed = null;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Manually sets the position of the container.
    /// <para>The container is not part of the hero to prevent flipping the scale/rotation.</para>
    /// </summary>
    private IEnumerator FollowHero()
    {
        _followed = HeroController.instance.transform;
        float rotation = 0f;
        while (true)
        {
            if (_followed == null || _orbContainer.transform.childCount == 0)
            {
                foreach (Transform child in _orbContainer.transform)
                    GameObject.Destroy(child.gameObject);
                if (HeroController.instance == null)
                {
                    LoreMaster.Instance.LogError("Couldn't find a hero to follow. Disable Pure Focus.");
                    DisablePower();
                    // This should never be reached, but just to make sure.
                    yield break;
                }
                // Refollow the hero.
                _followed = HeroController.instance.transform;
            }
            _orbContainer.transform.localPosition = _followed.position;
            _orbContainer.transform.SetPositionY(_orbContainer.transform.localPosition.y - .8f);
            rotation += Time.deltaTime * (120f + PlayerData.instance.GetInt(nameof(PlayerData.instance.MPCharge)) * 2);
            if (rotation >= 360f)
                rotation = 0;
            _orbContainer.transform.SetRotationZ(rotation);
            yield return null;
        }
    }

    /// <summary>
    /// Delay the check for spell
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForSpell()
    {
        yield return new WaitUntil(() => _fireball == null);
        yield return new WaitForSeconds(2f);
        _canThrow = _orbContainer.transform.childCount == 3;
    }

    private void SpawnOrb()
    {
        GameObject orb = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Mage"], _orbContainer.transform);
        orb.transform.localScale = new(1.25f, 1.25f, 1f);
        orb.transform.localPosition = CalculatePosition(_orbContainer.transform.childCount - 1);
        // Since creating a copy of a modified copy would be too easy, we have to modify it EVERY SINGLE TIME WE SPAWN IT. (:
        Component.Destroy(orb.LocateMyFSM("Orb Control"));
        // Prevent the reappearing of the damage hero hitbox.
        Component.Destroy(orb.GetComponent<PlayMakerFixedUpdate>());
        Component.Destroy(orb.GetComponent<AudioSource>());
        Component.Destroy(orb.GetComponent<Rigidbody2D>());
        Component.Destroy(orb.GetComponent<CircleCollider2D>());
        GameObject.Destroy(orb.transform.Find("Hero Hurter").gameObject);
        orb.layer = 17;
        orb.SetActive(true);
        GameObject.DontDestroyOnLoad(orb);
        foreach (Transform child in _orbContainer.transform)
            child.transform.localPosition = CalculatePosition(child.transform.GetSiblingIndex());

        GameObject hitbox = GameObject.Instantiate(_hitbox, orb.transform);
        Component.Destroy(hitbox.GetComponent<PolygonCollider2D>());
        hitbox.AddComponent<CircleCollider2D>().isTrigger = true;
        hitbox.name = "Orb Hitbox";
        PlayMakerFSM fsm = hitbox.LocateMyFSM("damages_enemy");
        fsm.FsmName = "Orb Damage";
        fsm.GetState("Send Event").InsertAction(new Lambda(() =>
        {
            fsm.FsmVariables.FindFsmInt("damageDealt").Value = PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_19))
            ? 4
            : 2;
            fsm.FsmVariables.FindFsmInt("attackType").Value = 2;
            fsm.FsmVariables.FindFsmFloat("magnitudeMult").Value = 0f;
            if (_followed != HeroController.instance.transform && !fsm.FsmVariables.FindFsmGameObject("Collider").Value.gameObject.name.Contains("Fireball"))
                SpawnExplosion(fsm.transform.parent);
        }
        ), 7);
        hitbox.transform.localPosition = new(0, 0);
        LoreMaster.Instance.Handler.StartCoroutine(BlinkHitbox(hitbox));
    }

    private Vector3 CalculatePosition(int index)
    {
        Vector3 vector3 = new();
        float circleHeight = 2f + _orbContainer.transform.childCount;
        if (index == 0)
        {
            vector3.x = 0f;
            vector3.y = circleHeight;
        }
        else if (index == 1)
            // If only two orbs exists, the second one is placed at the other side, otherwise further to the left so that all three have the same distance.
            if (circleHeight == 4)
            {
                vector3.x = 0;
                vector3.y = -circleHeight;
            }
            else
            {
                vector3.x = -(circleHeight * Mathf.Sqrt(3) / 2);
                vector3.y = -(circleHeight / 2);
            }
        else
        {
            vector3.x = circleHeight * Mathf.Sqrt(3) / 2;
            vector3.y = -(circleHeight / 2);
        }
        return vector3;
    }

    private void SpawnExplosion(Transform orb)
    {
        GameObject explosion = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Ceiling Dropper"]
            .LocateMyFSM("Ceiling Dropper")
            .GetState("Explode")
            .GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value);

        explosion.name = "Bomb Explosion";
        explosion.SetActive(false);
        ParticleSystem.MainModule settings = explosion.GetComponentInChildren<ParticleSystem>().main;
        settings.startColor = new ParticleSystem.MinMaxGradient(Color.red);
        explosion.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt").Value = PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_19))
            ? 30
            : 20;
        explosion.transform.localPosition = orb.position;
        explosion.transform.localScale = PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_19))
            ? new Vector3(1.35f, 1.35f)
            : new Vector3(1f, 1f);

        explosion.GetComponent<CircleCollider2D>().isTrigger = true;
        explosion.AddComponent<Rigidbody2D>().gravityScale = 0f;
        explosion.SetActive(true);
        int soulGain = 10;
        // Soul catcher
        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_20)))
            soulGain += 5;
        // Soul eater
        if (PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_21)))
            soulGain += 10;
        HeroController.instance.AddMPCharge(soulGain);
        GameObject.Destroy(orb.gameObject);
    }

    private IEnumerator BlinkHitbox(GameObject hitbox)
    {
        while (true)
        {
            yield return new WaitForSeconds(.5f);
            if (hitbox != null)
                hitbox.SetActive(true);
            else
                yield break;
            yield return new WaitForSeconds(0.1f);
            if (hitbox != null)
                hitbox.SetActive(false);
            else
                yield break;
        }
    }

    #endregion
}
