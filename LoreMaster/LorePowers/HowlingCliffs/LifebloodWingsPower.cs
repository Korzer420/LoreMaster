using LoreMaster.Enums;
using Modding;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LoreMaster.LorePowers.HowlingCliffs;

public class LifebloodWingsPower : Power
{
    #region Constructors

    public LifebloodWingsPower() : base("Lifeblood Wings", Area.Cliffs)
    {
        Hint = "[Not Implemented] Currently does nothing.";
        Description = "[Not Implemented] Currently does nothing.";
    }

    #endregion
}
