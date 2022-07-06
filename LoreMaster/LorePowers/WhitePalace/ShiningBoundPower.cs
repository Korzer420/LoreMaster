using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.WhitePalace;

public class ShiningBoundPower : Power
{
    #region Members

    private GameObject _charmHolder;

    #endregion

    #region Constructors

    public ShiningBoundPower() : base("", Area.WhitePalace)
    {
        _charmHolder = GameObject.Find("_GameCameras/HudCamera/Inventory").transform.Find("Charms/Equipped Charms/Charms").gameObject;
    }

    #endregion

    #region Public Methods

    public override void Enable() => HeroController.instance.StartCoroutine(GatherShiningSoul());
    
    public override void Disable() => HeroController.instance.StopCoroutine(GatherShiningSoul());
    
    #endregion

    #region Private Methods

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
