using LoreMaster.Enums;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.WhitePalace;

public class ShiningBoundPower : Power
{
    #region Constructors

    public ShiningBoundPower() : base("Shining Bound", Area.WhitePalace)
    {
        CustomText = "When the Wyrm arrised, all kinds of powerful relics were shattered around the world. If we could assemble them again, the energy would flood through us again.<br>";
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Enable() => _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(GatherShiningSoul());

    #endregion

    #region Private Methods

    /// <summary>
    /// Gather soul.
    /// </summary>
    /// <returns></returns>
    private IEnumerator GatherShiningSoul()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            HeroController.instance.AddMPCharge(PlayerData.instance.equippedCharms.Count);
        }
    }

    #endregion
}
