using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.Helper;

/// <summary>
/// Offers functions to help with modifying fsms.
/// </summary>
public static class FsmHelper
{
    /// <summary>
    /// Check if the passed action is in the correct context for modifications. Leave unneeded values null.
    /// </summary>
    /// <param name="fsmName">The fsm in which the action should be.</param>
    /// <param name="gameObjectName">The name of the gameobject on which the fsm of this action is attached.</param>
    /// <param name="stateName">The name of the state in which this action should be.</param>
    /// <param name="action">The action for the comparison.</param>
    /// <returns></returns>
    public static bool IsCorrectContext(string fsmName, string gameObjectName, string stateName, FsmStateAction action)
    {
        if (action == null)
            return false;
        if (!string.IsNullOrEmpty(fsmName) && !string.Equals(fsmName, action.Fsm.Name, StringComparison.OrdinalIgnoreCase))
            return false;
        if (!string.IsNullOrEmpty(gameObjectName) && !string.Equals(gameObjectName, action.Fsm.GameObjectName, StringComparison.OrdinalIgnoreCase))
            return false;
        if (!string.IsNullOrEmpty(stateName) && !string.Equals(stateName, action.State.Name, StringComparison.OrdinalIgnoreCase))
            return false;
       return true;
    }

    /// <summary>
    /// Move a transition from one state to another.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="transitionName"></param>
    /// <param name="newState"></param>
    public static void AdjustTransition(this FsmState state, string transitionName, string newState)
    {
        FsmTransition transition = state.Transitions.FirstOrDefault(x => string.Equals(x.FsmEvent.Name, transitionName));
        if (transition == null)
            throw new Exception($"Couldn't find transition {transitionName} in state {state.Name}");
        FsmState destination = state.Fsm.States.FirstOrDefault(x => string.Equals(x.Name, newState));
        if (destination == null)
            throw new Exception("Couldn't find destination state.");
        transition.SetToState(destination);
    }
}
