using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Helper;
using Modding;
using System;
using UnityEngine;

namespace LoreMaster.LorePowers.HowlingCliffs;

public class StagAdoptionPower : Power
{
    #region Members

    private PlayMakerFSM _menuFsm;

    #endregion

    #region Constructors

    public StagAdoptionPower() : base("Stag Adoption", Area.Cliffs)
    {
        ModHooks.SetPlayerBoolHook += ModHooks_SetPlayerBoolHook;
        Instance = this;
    }

    #endregion

    #region Properties

    public static StagAdoptionPower Instance { get; set; }

    public bool CanSpawnStag { get; set; }

    internal Sprite[] InventorySprites { get; } = new Sprite[]
    {
        SpriteHelper.CreateSprite("Stag_Egg"),
        SpriteHelper.CreateSprite("Stag_Egg_Broken")
    };

    public PlayMakerFSM MenuFsm => _menuFsm == null ? _menuFsm = GameObject.Find("_GameCameras").transform.Find("HudCamera/Menus").gameObject.LocateMyFSM("Open Stag") : _menuFsm;

    public override Action SceneAction => () => MenuFsm.SendEvent("DESPAWN");

    /// <summary>
    /// Gets or sets the last hatch moment to determine if the egg should respawn.
    /// </summary>
    public double HatchMoment { get; set; }

    #endregion

    #region Control

    protected override void Initialize() => LorePage.ActivateStagEgg();
    
    #endregion

    private bool ModHooks_SetPlayerBoolHook(string name, bool orig)
    {
        if (name == "hasStagEgg")
        {
            CanSpawnStag = orig;
            LorePage.ActivateStagEgg();
        }
        return orig;
    }

    internal void SpawnStag()
    {
        if (!CanSpawnStag)
            return;
        GameObject miniStag = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Stag"]);
        miniStag.SetActive(true);
        MenuFsm.SendEvent("SPAWN");
        
        // Adjust the fsm to prevent the stag from not appearing and modifying data supposed for normal stags.
        PlayMakerFSM fsm = miniStag.LocateMyFSM("Stag Control");
        fsm.GetState("Reset HUD and Flower Check").ClearTransitions();
        fsm.GetState("Reset HUD and Flower Check").AddTransition("FINISHED", "Activate");
        fsm.GetState("Activate 2").ClearTransitions();
        fsm.GetState("Activate 2").AddTransition("FINISHED", "Start Audio");
        fsm.GetState("Convo?").ClearTransitions();
        fsm.GetState("Convo?").AddTransition("FINISHED", "Open map");
        fsm.GetState("In Range").AddFirstAction(new Lambda(() => 
        {
            GameObject prompt = fsm.FsmVariables.FindFsmGameObject("Prompt").Value;
            prompt.transform.position = miniStag.transform.position + new Vector3(0f, 2f, 0f);
        }));
        fsm.FsmVariables.FindFsmInt("Current Position").Value = -1;
        FsmState mapState = fsm.GetState("Open map");
        mapState.Actions = new FsmStateAction[]
        {
            mapState.Actions[0],
            mapState.Actions[1],
            new Lambda(() => 
            {
                MenuFsm.FsmVariables.FindFsmGameObject("Requester").Value = miniStag;
                MenuFsm.SendEvent("OPEN STAG MENU");
            })
        };
        fsm.GetState("Check Result").AddTransition("FINISHED", "HUD Return");
        fsm.GetState("Check Result").AddFirstAction(new Lambda(() =>
        {
            if (string.IsNullOrEmpty(fsm.FsmVariables.FindFsmString("Selection Result").Value))
                fsm.SendEvent("FINISHED");
        }));
        miniStag.transform.localPosition = HeroController.instance.transform.position;
        miniStag.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        // Remove the talk collider
        Component.Destroy(miniStag.GetComponent<BoxCollider2D>());
        GameObject travel = miniStag.transform.Find("Travel Range").gameObject;
        travel.transform.localPosition = new(-12.6f, -2.6f, 1f);
        travel.transform.localScale = new(2f, 1f, 1f);
        travel.LocateMyFSM("Travel Range").GetState("Init").InsertAction(new Lambda(() => travel.LocateMyFSM("Travel Range").FsmVariables.FindFsmGameObject("Parent").Value = miniStag), 3);
        CanSpawnStag = false;
        HatchMoment = Time.realtimeSinceStartupAsDouble;
    }
}
