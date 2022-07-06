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
        
    }

    #endregion

    #region Public Methods

    protected override void Initialize()
    {
        _charmHolder = GameObject.Find("_GameCameras/HudCamera/Inventory").transform.Find("Charms/Equipped Charms/Charms").gameObject;
    }

    protected override void Enable() => HeroController.instance.StartCoroutine(GatherShiningSoul());
    
    protected override void Disable() => HeroController.instance.StopCoroutine(GatherShiningSoul());
    
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
