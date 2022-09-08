using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.Enums;

/// <summary>
/// Gets the current state of the buried treasure from Treasure Hunter.
/// </summary>
public enum TreasureState
{
    /// <summary>
    /// Didn't get the map yet, preventing the treasure ground from spawning.
    /// </summary>
    NoMap,

    /// <summary>
    /// Obtained the map, but didn't acquire the treasure yet.
    /// </summary>
    ObtainedMap,

    /// <summary>
    /// Obtained the map and their treasure.
    /// </summary>
    ObtainedTreasure
}
