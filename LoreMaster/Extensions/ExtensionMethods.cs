using HutongGames.PlayMaker;
using SFCore.Utils;
using System.Linq;

namespace LoreMaster.Extensions;

public static class ExtensionMethods
{
    /// <summary>
    /// Replaces an action in a fsm state.
    /// </summary>
    /// <param name="actionToAdd">The action you want to add. If an action with the same name already exists, nothing happens.</param>
    /// <param name="actionToRemove">If default, the action will be appended as well.</param>
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
}
