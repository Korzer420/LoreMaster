using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.Manager;

/// <summary>
/// Stores references from the hero.
/// </summary>
public static class HeroManager
{
    #region Members

    private static Rigidbody2D _rigidBody;

    private static GameObject[] _nailAttacks = new GameObject[5];

    private static GameObject[] _nailArts = new GameObject[3];

    private static BoxCollider2D _collider;

    private static tk2dSprite[] _dreamNailSprites = new tk2dSprite[4];

    private static tk2dSprite _sprite;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the rigidbody of the hero.
    /// </summary>
    public static Rigidbody2D RigidBody => _rigidBody == null ? _rigidBody = HeroController.instance.gameObject.GetComponent<Rigidbody2D>() : _rigidBody;

    /// <summary>
    /// Gets the normal nail attack objects.
    /// </summary>
    public static GameObject[] NailAttacks 
    { 
        get
        {
            if(_nailAttacks.Any(x => x == null))
            {
                GameObject attackDirections = GameObject.Find("Knight/Attacks");
                _nailAttacks[0] = attackDirections.transform.Find("Slash").gameObject;
                _nailAttacks[1] = attackDirections.transform.Find("AltSlash").gameObject;
                _nailAttacks[2] = attackDirections.transform.Find("UpSlash").gameObject;
                _nailAttacks[3] = attackDirections.transform.Find("DownSlash").gameObject;
                _nailAttacks[4] = attackDirections.transform.Find("WallSlash").gameObject;
            }
            return _nailAttacks;
        }
    }

    /// <summary>
    /// Gets the nail art objects.
    /// </summary>
    public static GameObject[] NailArts
    {
        get
        {
            if (_nailAttacks.Any(x => x == null))
            {
                GameObject attacks = GameObject.Find("Knight/Attacks");
                _nailArts[0] = attacks.transform.Find("Great Slash").gameObject;
                _nailArts[1] = attacks.transform.Find("Dash Slash").gameObject;
                _nailArts[2] = attacks.transform.Find("Cyclone Slash").gameObject;
            }
            return _nailArts;
        }
    }

    /// <summary>
    /// Gets the collider of the hero.
    /// </summary>
    public static BoxCollider2D Collider => _collider == null ? _collider = HeroController.instance.GetComponent<BoxCollider2D>() : _collider;

    public static tk2dSprite[] DreamNailSprites
    {
        get
        {
            if (_dreamNailSprites == null || _dreamNailSprites.Any(x => x == null))
                _dreamNailSprites = GameObject.Find("Knight/Dream Effects").GetComponentsInChildren<tk2dSprite>(true);
            return _dreamNailSprites;
        }
    }

    public static tk2dSprite Sprite => _sprite == null ? _sprite = HeroController.instance.GetComponent<tk2dSprite>() : _sprite;

    #endregion

    #region Event handler

    private static void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        _rigidBody = self.GetComponent<Rigidbody2D>();
        _collider = self.GetComponent<BoxCollider2D>();
    }

    #endregion

    #region Methods

    internal static void Initialize() => On.HeroController.Start += HeroController_Start;
    
    #endregion
}
