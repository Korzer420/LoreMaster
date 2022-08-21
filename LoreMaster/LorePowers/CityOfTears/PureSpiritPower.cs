using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

internal class PureSpiritPower : Power
{
    #region Members

    private readonly Vector3[] _positions = new Vector3[]
    {
        new(0, 2f, 0.0054f),
        new(Mathf.Sqrt(3f), -1f, 0.0054f),
        new(-Mathf.Sqrt(3f), -1f, 0.0054f)
    };

    private GameObject _orbContainer;

    private Transform _followed;

    private GameObject _fireball;

    private GameObject _hitbox;

    private bool _canThrow;

    #endregion

    #region Constructor

    public PureSpiritPower() : base("Pure Spirit", Area.CityOfTears) { }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        if (_orbContainer == null)
        {
            _orbContainer = new("Focus Orb Holder");
            GameObject.DontDestroyOnLoad(_orbContainer);
            _orbContainer.transform.localScale = new(1f, 1f, 1f);
        }
        _hitbox = HeroController.instance.transform.Find("Charm Effects/Thorn Hit/Hit D").gameObject;

        PlayMakerFSM fsm = HeroController.instance.spellControl;
        fsm.GetState("Spell End").ReplaceAction(new Lambda(() =>
        {
            if (Active && PlayerData.instance.GetInt(nameof(PlayerData.instance.MPCharge)) == 0
            && _orbContainer.transform.childCount < 3 && _followed == HeroController.instance.transform)
                SpawnOrb();
            LoreMaster.Instance.Handler.StartCoroutine(WaitForSpell());
        })
        {
            Name = "Is Empty?"
        });
    }

    protected override void Enable()
    {
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(FollowHero());
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
    }

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (string.Equals(self.FsmName, "Fireball Control"))
        {
            self.GetState("Set Damage").ReplaceAction(new Lambda(() =>
            {
                if (Active && _canThrow)
                    _followed = self.transform;
                else
                    _fireball = self.gameObject;
                if (!PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_19)))
                    self.SendEvent("FINISHED");
            })
            { Name = "Throw orbs?" }, self.gameObject.name.Contains("Spiral") ? 4 : 3);

            if (self.gameObject.name.Contains("Spiral"))
            {
                self.GetState("Shrink").AddLastAction(new Lambda(() =>
                {
                    if (Active && _followed == self.transform)
                    {
                        _followed = null;
                        _canThrow = false;
                    }
                }));
                self.GetState("Recycle").AddFirstAction(new Lambda(() => _fireball = null));
            }
            else
            {
                self.GetState("Wall Impact").AddLastAction(new Lambda(() =>
                {
                    if (Active && _followed == self.transform)
                        _followed = null;
                }));
                self.GetState("Dissipate").AddLastAction(new Lambda(() =>
                {
                    if (Active && _followed == self.transform)
                        _followed = null;
                }));
                self.GetState("Diss R").AddFirstAction(new Lambda(() => _fireball = null));
                self.GetState("Break R").AddFirstAction(new Lambda(() => _fireball = null));
            }
        }
        orig(self);
    }

    protected override void Disable()
    {
        foreach (Transform child in _orbContainer.transform)
            GameObject.Destroy(child.gameObject);
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
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
