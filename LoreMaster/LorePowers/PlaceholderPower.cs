namespace LoreMaster.LorePowers;

/// <summary>
/// "Fake Power" to allow none power tablets (like the dream warrior ones) to be accounted for the black egg temple door.
/// </summary>
internal class PlaceholderPower : Power
{
    public PlaceholderPower() : base("Placeholder", Enums.Area.None)
    {
        Tag = Enums.PowerTag.Remove;
        DefaultTag = Enums.PowerTag.Remove;
    }
}
