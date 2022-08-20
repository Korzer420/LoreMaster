using LoreMaster.LorePowers;
using LoreMaster.Manager;

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
    => PowerManager.AddPower(key, power);
    
}
