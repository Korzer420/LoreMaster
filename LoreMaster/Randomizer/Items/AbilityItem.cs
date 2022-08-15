using ItemChanger;
using ItemChanger.Items;
using LoreMaster.Enums;

namespace LoreMaster.Randomizer.Items;

internal class AbilityItem : CustomSkillItem
{
    /// <summary>
    /// Gets the flag, that indicates if this is the reading or talking item.
    /// </summary>
    public CustomItemType Item { get; set; }

    /// <summary>
    ///  Placeholder to prevent invalid operation exception
    /// </summary>
    protected override void OnLoad() { }

    public override void GiveImmediate(GiveInfo info)
    {
        if (Item == CustomItemType.Reading)
            LoreMaster.Instance.CanRead = true;
        else if (Item == CustomItemType.Listening)
            LoreMaster.Instance.CanListen = true;
        else
            PlayerData.instance.SetBool(nameof(PlayerData.instance.metElderbug), true);
    }

    public override bool Redundant()
    {
        return (Item == CustomItemType.Reading && LoreMaster.Instance.CanRead) || (Item == CustomItemType.Listening && LoreMaster.Instance.CanListen);
    }
}
