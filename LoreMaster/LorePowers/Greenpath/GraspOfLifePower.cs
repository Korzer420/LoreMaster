
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;

using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoreMaster.LorePowers.Greenpath;

public class GraspOfLifePower : Power
{
    #region Members

    private GameObject _normalGrass;

    private GameObject _paleGrass;

    private GameObject _scream;

    private GameObject _shriek;

    private List<GameObject> _grasses = new();

    private float _foolEaterTimer = 2;

    #endregion

    #region Constructors

    public GraspOfLifePower() : base("Grasp of Life", Area.Greenpath) { }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public override Action SceneAction => () =>
    {
        _grasses.Clear();
        _foolEaterTimer = 2f;
    };

    public GameObject Scream 
    { 
        get 
        {
            if(_scream == null)
            {
                _scream = GameObject.Instantiate(HeroController.instance.transform.Find("Spells/Scr Heads").gameObject);
                GameObject.DontDestroyOnLoad(_scream);
            }
            return _scream;
        }
    }

    public GameObject Shriek
    {
        get
        {
            if(_shriek == null)
            {
                _shriek = GameObject.Instantiate(HeroController.instance.transform.Find("Spells/Scr Heads 2").gameObject);
                GameObject.DontDestroyOnLoad(_shriek);
            }
            return _shriek;
        }
    }

    #endregion

    #region Event handler

    private void GrassSpriteBehaviour_OnTriggerEnter2D(On.GrassSpriteBehaviour.orig_OnTriggerEnter2D orig, GrassSpriteBehaviour self, Collider2D collision)
    {
        if (string.Equals(self.gameObject.name,"Power grass"))
        {
            bool isCut = ReflectionHelper.GetField<GrassSpriteBehaviour, bool>(self, "isCut");
            orig(self, collision);
            if (!isCut && ReflectionHelper.GetField<GrassSpriteBehaviour, bool>(self, "isCut"))
            {
                foreach (GameObject go in _grasses)
                {
                    if (go == null)
                        continue;
                    try
                    {
                        bool hasUnn = PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_28));
                        GameObject cheer = GameObject.Instantiate(hasUnn ? Shriek : Scream);
                        cheer.LocateMyFSM("FSM").FsmVariables.FindFsmBool("Unparent").Value = false;
                        cheer.LocateMyFSM("FSM").FsmVariables.FindFsmBool("Reposition").Value = false;
                        cheer.GetComponent<tk2dSprite>().color = Color.green;
                        cheer.transform.localPosition = go.transform.localPosition;
                        Component.Destroy(cheer.LocateMyFSM("Deactivate on Hit"));
                        foreach (Transform child in cheer.transform)
                            child.gameObject.LocateMyFSM("damages_enemy").GetState("Send Event").ReplaceAction(new Lambda(() =>
                            {
                                child.gameObject.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt").Value = hasUnn ? 20 : 10;
                            }), 0);
                        cheer.SetActive(true);
                        // We destroy the object instead of disabling it.
                        cheer.LocateMyFSM("FSM").GetState("Destroy").ReplaceAction(new Lambda(() => GameObject.Destroy(cheer)), 0);
                    }
                    catch (Exception exception)
                    {
                        LoreMaster.Instance.LogError(exception.Message);
                    }
                    GameObject.Destroy(go);
                }
                _grasses.Clear();
            }
            return;
        }
        else
            orig(self, collision);
        if (string.Equals(self.gameObject.name,"Grasp grass"))
            GameObject.Destroy(self.gameObject);
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        _normalGrass = LoreMaster.Instance.PreloadedObjects["green_grass_1"];
        _paleGrass = LoreMaster.Instance.PreloadedObjects["ash_grass_02"];
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(SpawnGrass());
        // We use an on hook because I currently don't know how to get the object values in an IL context.
        On.GrassSpriteBehaviour.OnTriggerEnter2D += GrassSpriteBehaviour_OnTriggerEnter2D;
    }

    /// <inheritdoc/>
    protected override void Disable() => On.GrassSpriteBehaviour.OnTriggerEnter2D -= GrassSpriteBehaviour_OnTriggerEnter2D;

    /// <inheritdoc/>
    protected override void TwistEnable() => _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(SpawnFoolEater());
    
    #endregion

    #region Private Methods

    /// <summary>
    /// Controls the grass (de)spawn.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnGrass()
    {
        int currentTick = 1;
        float passedTime = 0f;
        while (true)
        {
            if (HeroController.instance.hero_state == GlobalEnums.ActorStates.running)
                passedTime += Time.deltaTime;
            else
                passedTime = 0f;
            if (passedTime >= 1f)
            {
                GameObject grass = GameObject.Instantiate(currentTick == 10 ? _paleGrass : _normalGrass);
                grass.transform.localPosition = new(HeroController.instance.transform.localPosition.x, HeroController.instance.transform.Find("HeroBox").GetComponent<BoxCollider2D>().bounds.min.y + 1f, -0.06f);
                grass.transform.localScale = new(1f, 1f, 1f);
                grass.name = currentTick == 10 ? "Power grass" : "Grasp grass";
                grass.SetActive(true);
                _grasses.Add(grass);
                currentTick++;
                if (currentTick > 10)
                    currentTick = 1;
                _grasses.RemoveAll(x => x == null);
                if (_grasses.Count > 5)
                {
                    GameObject.Destroy(_grasses[0]);
                    _grasses.RemoveAt(0);
                }
                passedTime = 0f;
            }
            yield return null;
        }
    }

    private IEnumerator SpawnFoolEater()
    {
        float passedTime = 0f;
        while(true)
        {
            if(HeroController.instance.cState.onGround && !PlayerData.instance.GetBool("atBench"))
                passedTime += Time.deltaTime;
            if (passedTime >= _foolEaterTimer)
            {
                _foolEaterTimer += .5f;
                passedTime = 0f;
                if (_grasses.Count >= 10)
                {
                    GameObject.Destroy(_grasses[0]);
                    _grasses.RemoveAt(0);
                }
                GameObject fool = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Plant Trap"]);
                fool.SetActive(true);
                fool.transform.position = new(HeroController.instance.transform.localPosition.x, HeroController.instance.transform.Find("HeroBox").GetComponent<BoxCollider2D>().bounds.min.y + 2f, -0.06f);
                fool.transform.localScale = new(1f, 1f, 1f);
                _grasses.Add(fool);
            }
            yield return null;
        }
    }

    #endregion
}
