using ItemChanger;
using LoreMaster.LorePowers.CityOfTears;
using Modding;
using System.Collections;
using UnityEngine;

namespace LoreMaster.ItemChangerData;

internal record Paypal: Cost
{
    public Paypal() 
    { }

    public bool ToTemple { get; set; }

    public override bool CanPay()
    => TouristPower.Inspected && PlayerData.instance.GetInt(nameof(PlayerData.instance.geo)) >= 50;

    public override string GetCostText()
    => TouristPower.Inspected ? "Take a ticket? (50 Geo)" : "Currently closed";

    public override bool HasPayEffects()
    => true;

    public override void OnPay()
    {
        HeroController.instance.TakeGeo(50);
        Paid = false;
    }
}
