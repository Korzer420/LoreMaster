using ItemChanger;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LoreMaster.ItemChangerData.Locations.SpecialLocations;

internal class QuirrelArchiveLocation : TravellerLocation
{
    protected override void ControlSpawn(Scene scene)
    {
        GameObject npc = GameObject.Find(ObjectName);
        base.ControlSpawn(scene);
        if (npc.activeSelf)
            npc.SetActive(PlayerData.instance.GetBool(nameof(PlayerData.instance.summonedMonomon)));
    }
}
