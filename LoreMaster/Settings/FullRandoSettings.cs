using LoreMaster.Enums;
using System.Collections.Generic;

namespace LoreMaster.Settings;

/// <summary>
/// Wrapper class to allow RSM to set the tags of the individual powers.
/// </summary>
public class FullRandoSettings
{
    public RandomizerSettings BaseSettings;

    public List<PowerTag> Tags { get; set; } = new();
}
