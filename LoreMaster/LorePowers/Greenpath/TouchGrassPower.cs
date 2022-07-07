using System;
using Modding;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;

namespace LoreMaster.LorePowers.Greenpath;

public class TouchGrassPower : Power
{
    #region Members

    private bool _currentlyRunning;

    private Collider2D _heroCollider;

    private Collider2D _triggeredCollider;

    #endregion

    #region Constructors

    public TouchGrassPower() : base("", Area.Greenpath)
    {

    }

    #endregion

    #region Properties

    public float HealTime => PlayerData.instance.GetBool("equippedCharm_28") ? 2.5f : 5f;

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

    #region Public Methods

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

    protected override void Disable()
    {
        On.Grass.OnTriggerEnter2D -= Grass_OnTriggerEnter2D;
        On.GrassBehaviour.OnTriggerEnter2D -= GrassBehaviour_OnTriggerEnter2D;
        On.TownGrass.OnTriggerEnter2D -= TownGrass_OnTriggerEnter2D;
        On.GrassSpriteBehaviour.OnTriggerEnter2D -= GrassSpriteBehaviour_OnTriggerEnter2D;
        On.GrassWind.OnTriggerEnter2D -= GrassWind_OnTriggerEnter2D;
        On.GrassCut.OnTriggerEnter2D -= GrassCut_OnTriggerEnter2D;
        // This forces the coroutine to stop (if it's still running)
        _currentlyRunning = false;
    }

    #endregion

    #region Private Methods

    private void CheckForHeroEnter(Collider2D collider, Collider2D sourceCollider)
    {
        if (!_currentlyRunning && (collider.tag.Equals("Player") || collider.gameObject.name.Equals("HeroBox")))
        {
            if (_heroCollider == null)
                _heroCollider = collider;
            _triggeredCollider = sourceCollider;
            LoreMaster.Instance.Handler.StartCoroutine(TouchGrass());
        }
    }

    private IEnumerator TouchGrass()
    {
        _currentlyRunning = true;
        while (true)
        {
            float passedTime = 0f;
            while (passedTime <= HealTime)
            {
                if (_triggeredCollider.IsTouching(_heroCollider) || _heroCollider.IsTouching(_triggeredCollider))
                {
                    yield return null;
                    passedTime += Time.deltaTime;
                }
                else
                {
                    _currentlyRunning = false;
                    yield break;
                }
            }
            if (PlayerData.instance.GetInt("Health") < PlayerData.instance.maxHealth)
                HeroController.instance.AddHealth(1);
        }
    }

    #endregion
}

