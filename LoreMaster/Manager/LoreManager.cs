namespace LoreMaster.Manager;

/// <summary>
/// Manager for handling the lore related logic.
/// </summary>
internal class LoreManager
{
    #region Constructors

    public LoreManager() => Instance = this;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets that powers should display their custom text (if available)
    /// </summary>
    public bool UseCustomText { get; set; } = true;

    /// <summary>
    /// Gets or sets if hints should be displayed instead of clear descriptions.
    /// </summary>
    public bool UseHints { get; set; } = true;

    /// <summary>
    /// Gets or sets the value, that indicates if the player can read lore tablets. (Rando only)
    /// </summary>
    public bool CanRead { get; set; } = true;

    /// <summary>
    /// Gets or sets the value, that indicates if the player can listen to npc.
    /// </summary>
    public bool CanListen { get; set; } = true;

    /// <summary>
    /// Gets or sets the amount of joker scrolls, that the player can use to obtain a power of their choice.
    /// </summary>
    public int JokerScrolls { get; set; } = 3;

    /// <summary>
    /// Gets or sets the amount of cleansing scrolls, that the player can use to undo a twisted obtain power of their choice.
    /// </summary>
    public int CleansingScrolls { get; set; } = 3;

    public static LoreManager Instance { get; set; }

    #endregion
}
