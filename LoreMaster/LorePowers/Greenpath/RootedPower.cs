using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
using MonoMod.Cil;
using System;
using UnityEngine;

namespace LoreMaster.LorePowers.Greenpath;

public class RootedPower : Power
{
    #region Constructors

    public RootedPower() : base("Rooted", Area.Greenpath)
    {
        Hint = "[Not Implemented] Currently does nothing.";
        Description = "[Not Implemented] Currently does nothing.";
    }

    #endregion

}
