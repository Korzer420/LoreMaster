using HutongGames.PlayMaker;
using ItemChanger.Extensions;
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
                state.AddLastAction(actionToAdd);
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
