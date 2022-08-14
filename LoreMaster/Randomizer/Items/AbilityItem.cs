using ItemChanger;
using ItemChanger.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.Randomizer.Items;

internal class AbilityItem : CustomSkillItem
{
    /// <summary>
    /// Gets the flag, that indicates if this is the reading or talking item.
    /// </summary>
    public bool IsReading { get; set; }

    /// <summary>
    ///  Placeholder to prevent invalid operation exception
    /// </summary>
    protected override void OnLoad() { }

    public override void GiveImmediate(GiveInfo info)
    {
        if (IsReading)
            LoreMaster.Instance.CanRead = true;
        else
            LoreMaster.Instance.CanListen = true;
    }

    public override bool Redundant()
    {
        return (IsReading && LoreMaster.Instance.CanRead) || (!IsReading && LoreMaster.Instance.CanListen);
    }
}
