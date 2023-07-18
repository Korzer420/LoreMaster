using LoreMaster.Enums;
using LoreMaster.Settings;
using System.Collections.Generic;

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
    /// Gets or sets the value that indicates, if the tracker from Greater Mind should be displayed permanently or fade away after 5 seconds.
    /// </summary>
    public bool TrackerPermanently { get; set; }
}
