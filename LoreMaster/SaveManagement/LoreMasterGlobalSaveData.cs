using LoreMaster.Randomizer;

namespace LoreMaster.SaveManagement;

public class LoreMasterGlobalSaveData
{
    /// <summary>
    /// Gets or sets the flag that indicates if custom text should be displayed (if it exists).
    /// </summary>
    public bool EnableCustomText { get; set; }

    /// <summary>
    /// Gets or sets the flag that indicates if hints should be displayed instead of clear descriptions.
    /// </summary>
    public bool ShowHint { get; set; }

    /// <summary>
    /// Gets or sets the flag that indicates if the nausea effect of the yellow mushroom should be disabled.
    /// </summary>
    public bool DisableNausea { get; set; }

    /// <summary>
    /// Gets or sets the flag that indicates if the bomb spells can be cast via quick cast.
    /// </summary>
    public bool BombQuickCast { get; set; }

    /// <summary>
    /// Gets or sets the setting for rando.
    /// </summary>
    public RandomizerSettings RandoSettings { get; set; }
}
