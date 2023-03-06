namespace LoreMaster.LorePowers.FogCanyon;

internal class LifeInsurancePower : Power
{
    public LifeInsurancePower() : base("Life Insurance", Enums.Area.FogCanyon)
    {
        // FUngus3_35
        //PlayerData.instance.bankerSpaMet
    }

    protected override void Enable()
    {
        PlayerData.instance.SetBool(nameof(PlayerData.instance.bankerSpaMet), false);
    }
}
