using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.Enums;

/// <summary>
/// Determines, how travel npc (Cloth, Zote, Tiso and Quirrel) should appear.
/// </summary>
public enum TravelOrder
{
    /// <summary>
    /// Appear in the order that would be applied in the normal game. Note that the conditions when enemies disappear don't apply.
    /// </summary>
    Vanilla,

    /// <summary>
    /// 
    /// </summary>
    Shuffled
}
