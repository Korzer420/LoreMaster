using HutongGames.PlayMaker;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LoreMaster.ItemChanger.Locations;

public class InspectLocation : AutoLocation
{
    #region Control

    protected override void OnLoad()
    {
        if (NeedInspectRegion)
            Events.AddSceneChangeEdit(sceneName, GenerateInspectRegion);
        else
            Events.AddFsmEdit(sceneName, new FsmID(GameObjectName, "Conversation Control"), ModifyInspect);
    }

    protected override void OnUnload()
    {
        if (NeedInspectRegion)
            Events.RemoveSceneChangeEdit(sceneName, GenerateInspectRegion);
        else
            Events.RemoveFsmEdit(sceneName, new FsmID(GameObjectName, "Conversation Control"), ModifyInspect);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the gameobject which should be inspectable.
    /// </summary>
    public string GameObjectName { get; set; }

    /// <summary>
    /// Gets or sets the flag that indicates if the mod has to manually generate an inspect region.
    /// </summary>
    public bool NeedInspectRegion { get; set; }

    /// <summary>
    /// Gets or sets the elevation the generated inspect region should be placed. Only needed if <see cref="NeedInspectRegion"/> is true.
    /// </summary>
    public float Elevation { get; set; }

    #endregion

    #region Methods

    private void ModifyInspect(PlayMakerFSM fsm)
    {
        FsmState startState = fsm.GetState("Hero Anim");
        startState.ClearTransitions();
        startState.AddTransition("FINISHED", "Talk Finish");
        fsm.GetState("Anim End").AddLastAction(new AsyncLambda(callback => ItemUtility.GiveSequentially(Placement.Items, Placement, new GiveInfo
        {
            FlingType = flingType,
            Container = Container.Tablet,
            MessageType = MessageType.Any,
        }, callback), "CONVO FINISHED"));
        fsm.GetState("Anim End").ClearTransitions();
        fsm.GetState("Anim End").AddTransition("CONVO FINISHED", "Box Down");
    }

    private void GenerateInspectRegion(Scene scene)
    {
        GameObject inspect = UnityEngine.Object.Instantiate(LoreMaster.Instance.PreloadedObjects["Inspect Region"], GameObject.Find(GameObjectName).transform);
        inspect.transform.localPosition += new Vector3(0f, Elevation, 0f);
    }

    #endregion
}
