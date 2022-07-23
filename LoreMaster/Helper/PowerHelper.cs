using LoreMaster.LorePowers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.Helper;

/// <summary>
/// Helper class for other mods, to include their own power
/// </summary>
public static class PowerHelper
{
    /// <summary>
    /// Adds a power to power dictionary.
    /// </summary>
    /// <param name="power">The power that you want to add.</param>
    /// <param name="key">The text key which should activate the power ingame.</param>
    public static void AddPower(string key, Power power)
    => LoreMaster.Instance.AddPower(key, power);
    
}
