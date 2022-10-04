using ItemChanger;
using ItemChanger.Extensions;
using LoreMaster.Enums;
using LoreMaster.Randomizer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LoreMaster.ItemChangerData.Locations;

internal class TravellerLocation : DialogueLocation
{
    #region Members

    private static readonly (string, string)[] _quirrelNames = new (string, string)[]
{
        ("Room_temple", "Quirrel"),
        ("Room_Slug_Shrine", "Quirrel Slug Shrine"),
        ("Fungus2_01","Quirrel Station NPC"),
        ("Fungus2_14","Quirrel Mantis NPC"),
        ("Ruins1_02","RestBench/Quirrel Bench"),
        ("Deepnest_30" ,"Quirrel Spa"),
        ("Mines_13","Quirrel Mines"),
        ("Fungus3_47" ,"Quirrel Archive Ext"),
        ("Fungus3_archive_02","Dreamer Monomon/Quirrel Wounded"),
        ("Crossroads_50","Quirrel Lakeside")
};

    private static readonly (string, string)[] _clothNames = new (string, string)[]
    {
        ("Fungus2_09","Cloth NPC 1"),
        ("Abyss_17","Cloth NPC Tramway"),
        ("Deepnest_14","Cloth NPC 2"),
        ("Fungus3_34","Cloth NPC QG Entrance"),
        ("Fungus3_23", "Cloth Ghost NPC"),
        ("Town","Cloth NPC Town")
    };

    private static readonly (string, string)[] _zoteNames = new (string, string)[]
    {
        ("Fungus1_20_v02", "Zote Buzzer Convo"),
        ("Town","_NPCs/Zote Town"),
        ("Ruins1_06","Zote Ruins"),
        ("Deepnest_33","Zote Deepnest"),
        ("Room_Colosseum_02","Zote Colosseum"),
        ("Town","_NPCs/Zote Final Scene")
    };

    private static readonly (string, string)[] _tisoNames = new (string, string)[]
    {
        ("Town","_NPCs/Tiso Town NPC"),
        ("Crossroads_47","_NPCs/Tiso Bench NPC"),
        ("Crossroads_50","Tiso Lake NPC"),
        ("Room_Colosseum_02","Tiso Col NPC"),
        ("Deepnest_East_07","tiso_corpse")
    }; 

    #endregion

    public Traveller TravellerName { get; set; }

    public string GameObjectName { get; set; }

    protected override void OnLoad()
    {
        Events.AddSceneChangeEdit(sceneName, ControlSpawn);
        base.OnLoad();
    }

    protected override void OnUnload()
    {
        Events.RemoveSceneChangeEdit(sceneName, ControlSpawn);
        base.OnLoad();
    }

    /// <summary>
    /// Controls if the npc should spawn at all.
    /// </summary>
    private void ControlSpawn(Scene scene)
    {
        GameObject npc = GameObject.Find(GameObjectName);
        // Remove all components which affect the spawn.
        foreach (DeactivateIfPlayerdataTrue item in npc.GetComponents<DeactivateIfPlayerdataTrue>())
            Component.Destroy(item);
        foreach (DeactivateIfPlayerdataFalse item in npc.GetComponents<DeactivateIfPlayerdataFalse>())
            Component.Destroy(item);
        foreach (PlayMakerFSM item in npc.GetComponents<PlayMakerFSM>()?.Where(x => x.FsmName.StartsWith("Destroy") || x.FsmName == "FSM" 
        || x.FsmName == "Death" || x.FsmName.StartsWith("Leave") || x.FsmName == "deactivate"))
            Component.Destroy(item);
        if (Placement.Items.All(x => x.IsObtained()))
        {
            npc.SetActive(false);
            return;
        }
        else
        {
            int npcIndex;
            // Cloth town and her ghost share a placement, which is why we take the ghost for town Cloth.
            if (TravellerName == Traveller.Cloth && sceneName == "Town")
                npcIndex = RandomizerManager.TravellerOrder[TravellerName].IndexOf("Cloth Ghost NPC");
            else
                npcIndex = RandomizerManager.TravellerOrder[TravellerName].IndexOf(GameObjectName);
            npc.SetActive(npcIndex <= RandomizerManager.TravellerStages[TravellerName]);
        }
    }

    /// <summary>
    /// Shuffle the order of the traveller stages.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="shuffle">Determines if the order should be shuffled.</param>
    /// <returns></returns>
    internal static List<string> DetermineOrder(Traveller npc, bool shuffle)
    {
        List<string> result = new();
        List<string> order = npc switch 
        {
            Traveller.Quirrel => _quirrelNames.Select(x => x.Item2).ToList(),
            Traveller.Cloth => _clothNames.Select(x => x.Item2).Take(5).ToList(),
            Traveller.Tiso => _tisoNames.Select(x => x.Item2).ToList(),
            _ => _zoteNames.Select(x => x.Item2).ToList()
        };
        
        if (!shuffle)
            return order;
        else
            do
            {
                int selectedValue = LoreMaster.Instance.Generator.Next(0, order.Count);
                result.Add(order[selectedValue]);
                order.RemoveAt(selectedValue);
            }
            while (order.Any());
        return result;
    }

    internal static TravellerLocation CreateTravellerLocation(string locationName, int index, Traveller traveller, string fsmName = "Conversation Control")
    {
        (string, string) data = traveller switch
        {
            Traveller.Quirrel => _quirrelNames[index],
            Traveller.Tiso => _tisoNames[index],
            Traveller.Cloth => _clothNames[index],
            _ => _zoteNames[index]
        };

        return new()
        {
            name = locationName,
            sceneName = data.Item1,
            TravellerName = traveller,
            FsmName = fsmName,
            ObjectName = data.Item2
        };
    }
}