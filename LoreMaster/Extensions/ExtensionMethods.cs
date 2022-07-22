using HutongGames.PlayMaker;
using SFCore.Utils;
using System.Linq;

namespace LoreMaster.Extensions;

public static class ExtensionMethods
{
    public static void ReplaceAction(this FsmState state, FsmStateAction actionToAdd, int actionToRemove = -1)
    {
        try
        {
            if (state.Actions.Any(x => string.Equals(x.Name, actionToAdd.Name)))
                return;
            if (actionToRemove == -1)
                state.AddAction(actionToAdd);
            else
            {
                state.RemoveAction(actionToRemove);
                state.InsertAction(actionToAdd, actionToRemove);
            }
        }
        catch (System.Exception exception)
        {
            LoreMaster.Instance.LogError("Couldn't replace method: "+exception.Message);
        }
    }

    public static void AddGTransition(this PlayMakerFSM fsm, string eventName, string stateName)
    {
        fsm.AddGlobalTransition(eventName, stateName);
    }
}
