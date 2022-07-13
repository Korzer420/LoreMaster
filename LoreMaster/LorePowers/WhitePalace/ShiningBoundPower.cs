using LoreMaster.Enums;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.WhitePalace;

public class ShiningBoundPower : Power
{
    #region Members

    private GameObject _charmHolder;

    #endregion

    #region Constructors

    public ShiningBoundPower() : base("Shining Bound", Area.WhitePalace)
    {
        CustomText = "When the Wyrm arrised, all kinds of powerful relics were shattered around the world. If we could assemble them again, the energy would flood through us again.<br>";
        Hint = "Every magic relic you currently wearing  shall gather more soul for you.";
        Description = "For each charm you're wearing, you gain 1 soul per 2 seconds.";
    }

    #endregion

    #region Protected Methods

    protected override void Initialize() => _charmHolder = GameObject.Find("_GameCameras/HudCamera/Inventory").transform.Find("Charms/Equipped Charms/Charms").gameObject;
    
    protected override void Enable() => LoreMaster.Instance.Handler.StartCoroutine(GatherShiningSoul());
    
    protected override void Disable() => LoreMaster.Instance.Handler.StopCoroutine("GatherShiningSoul");
    
    #endregion

    #region Private Methods

    /// <summary>
    /// Gather soul.
    /// </summary>
    /// <returns></returns>
    private IEnumerator GatherShiningSoul()
    {
        while(true)
        {
            yield return new WaitForSeconds(2f);
            HeroController.instance.AddMPCharge(_charmHolder.transform.childCount);
        }
    }

    #endregion
}
