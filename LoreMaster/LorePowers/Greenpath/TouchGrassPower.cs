using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.Manager;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.Greenpath;

public class TouchGrassPower : Power
{
    #region Members

    private bool _currentlyRunning;

    private Collider2D _triggeredCollider;

    #endregion

    #region Constructors

    public TouchGrassPower() : base("Touch Grass", Area.Greenpath) { }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the needed time to stand on grass to heal.
    /// </summary>
    public float HealTime => State == PowerState.Active ? (PlayerData.instance.GetBool("equippedCharm_28") ? 4f : 8f) : 1.5f;

    #endregion

    #region Event handler

    private void Grass_OnTriggerEnter2D(On.Grass.orig_OnTriggerEnter2D orig, Grass self, Collider2D other)
    {
        orig(self, other);
        CheckForHeroEnter(self.GetComponent<BoxCollider2D>(), other);
    }

    private void GrassCut_OnTriggerEnter2D(On.GrassCut.orig_OnTriggerEnter2D orig, GrassCut self, Collider2D collision)
    {
        orig(self, collision);
        CheckForHeroEnter(collision, self.GetComponent<BoxCollider2D>());
    }

    private void GrassWind_OnTriggerEnter2D(On.GrassWind.orig_OnTriggerEnter2D orig, GrassWind self, Collider2D collision)
    {
        orig(self, collision);
        CheckForHeroEnter(collision, self.GetComponent<BoxCollider2D>());
    }

    private void TownGrass_OnTriggerEnter2D(On.TownGrass.orig_OnTriggerEnter2D orig, TownGrass self, Collider2D collision)
    {
        orig(self, collision);
        CheckForHeroEnter(collision, self.GetComponent<BoxCollider2D>());
    }

    private void GrassSpriteBehaviour_OnTriggerEnter2D(On.GrassSpriteBehaviour.orig_OnTriggerEnter2D orig, GrassSpriteBehaviour self, Collider2D collision)
    {
        orig(self, collision);
        CheckForHeroEnter(collision, self.GetComponent<BoxCollider2D>());
    }

    private void GrassBehaviour_OnTriggerEnter2D(On.GrassBehaviour.orig_OnTriggerEnter2D orig, GrassBehaviour self, Collider2D collision)
    {
        orig(self, collision);
        CheckForHeroEnter(collision, self.GetComponent<BoxCollider2D>());
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable()
    {
        // Grass is a bit... special. We hook onto every grass class. This (hopefully) catches all grasses. -.- 
        On.Grass.OnTriggerEnter2D += Grass_OnTriggerEnter2D;
        On.GrassBehaviour.OnTriggerEnter2D += GrassBehaviour_OnTriggerEnter2D;
        On.TownGrass.OnTriggerEnter2D += TownGrass_OnTriggerEnter2D;
        On.GrassSpriteBehaviour.OnTriggerEnter2D += GrassSpriteBehaviour_OnTriggerEnter2D;
        On.GrassWind.OnTriggerEnter2D += GrassWind_OnTriggerEnter2D;
        On.GrassCut.OnTriggerEnter2D += GrassCut_OnTriggerEnter2D;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        On.Grass.OnTriggerEnter2D -= Grass_OnTriggerEnter2D;
        On.GrassBehaviour.OnTriggerEnter2D -= GrassBehaviour_OnTriggerEnter2D;
        On.TownGrass.OnTriggerEnter2D -= TownGrass_OnTriggerEnter2D;
        On.GrassSpriteBehaviour.OnTriggerEnter2D -= GrassSpriteBehaviour_OnTriggerEnter2D;
        On.GrassWind.OnTriggerEnter2D -= GrassWind_OnTriggerEnter2D;
        On.GrassCut.OnTriggerEnter2D -= GrassCut_OnTriggerEnter2D;
        _currentlyRunning = false;
    }

    /// <inheritdoc/>
    protected override void TwistEnable() => Enable();

    /// <inheritdoc/>
    protected override void TwistDisable() => Disable();

    #endregion

    #region Private Methods

    /// <summary>
    /// Check if the hero touches a grass object.
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="sourceCollider"></param>
    private void CheckForHeroEnter(Collider2D collider, Collider2D sourceCollider)
    {
        if (!_currentlyRunning && (collider.tag.Equals("Player") || collider.gameObject.name.Equals("HeroBox")))
        {
            _triggeredCollider = sourceCollider;
            _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(TouchGrass());
        }
    }

    /// <summary>
    /// Coroutine to wait for standing long enough on grass to heal.
    /// </summary>
    private IEnumerator TouchGrass()
    {
        _currentlyRunning = true;
        while (true)
        {
            float passedTime = 0f;
            while (passedTime <= HealTime)
            {
                if (_triggeredCollider == null || HeroHelper.Collider == null || (!_triggeredCollider.IsTouching(HeroHelper.Collider) && !HeroHelper.Collider.IsTouching(_triggeredCollider)))
                {
                    _currentlyRunning = false;
                    yield break;
                }
                else
                {
                    yield return null;
                    passedTime += Time.deltaTime;
                }
            }
            if (State == PowerState.Twisted)
            {
                if (PlayerData.instance.GetInt("health") + PlayerData.instance.GetInt("healthBlue") == 1)
                    HeroController.instance.TakeDamage(null, GlobalEnums.CollisionSide.top, 1, 1);
                else
                {
                    FakeDamage = true;
                    HeroController.instance.TakeHealth(1);
                    FakeDamage = false;
                }
            }
            else if (PlayerData.instance.GetInt("Health") < PlayerData.instance.GetInt(nameof(PlayerData.instance.maxHealth)))
                HeroController.instance.AddHealth(1);
        }
    }

    #endregion
}

