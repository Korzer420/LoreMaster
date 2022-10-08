using ItemChanger;
using ItemChanger.Extensions;
using LoreMaster.Enums;
using LoreMaster.ItemChangerData.Locations.SpecialLocations;
using LoreMaster.Manager;
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
        ("Ruins1_02","/RestBench/Quirrel Bench"),
        ("Mines_13","Quirrel Mines"),
        ("Deepnest_30" ,"Quirrel Spa"),
        ("Fungus3_47" ,"Quirrel Archive Ext"),
        ("Fungus3_archive_02","/Dreamer Monomon/Quirrel Wounded"),
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
        ("Fungus1_20_v02", "Zote Buzzer Convo(Clone)"),
        ("Town","/_NPCs/Zote Town"),
        ("Ruins1_06","Zote Ruins"),
        ("Deepnest_33","/Zote Deepnest/Faller/NPC"),
        ("Room_Colosseum_02","Zote Colosseum"),
        ("Town","/_NPCs/Zote Final Scene/Zote Final")
    };

    private static readonly (string, string)[] _tisoNames = new (string, string)[]
    {
        ("Town","/_NPCs/Tiso Town NPC"),
        ("Crossroads_47","/_NPCs/Tiso Bench NPC"),
        ("Crossroads_50","Tiso Lake NPC"),
        ("Room_Colosseum_02","Tiso Col NPC"),
        ("Deepnest_East_07","tiso_corpse")
    };

    #endregion

    public Traveller TravellerName { get; set; }

    protected override void OnLoad()
    {
        Events.AddSceneChangeEdit(sceneName, ControlSpawn);
        base.OnLoad();
    }

    protected override void OnUnload()
    {
        Events.RemoveSceneChangeEdit(sceneName, ControlSpawn);
        base.OnUnload();
    }

    /// <summary>
    /// Controls if the npc should spawn at all.
    /// </summary>
    protected virtual void ControlSpawn(Scene scene)
    {
        string objectName = ObjectName.Contains("Final Scene") ? "_NPCs/Zote Final Scene" : ObjectName;
        GameObject npc = GameObject.Find(objectName);
        if (npc == null)
        {
            npc = CheckComponent<DeactivateIfPlayerdataTrue>(objectName);
            if (npc == null)
                npc = CheckComponent<DeactivateIfPlayerdataFalse>(objectName);
            if (npc == null)
            {
                LoreMaster.Instance.LogError("Couldn't find " + objectName + ".");
                return;
            }
        }
        // Remove all components which affect the spawn.
        foreach (DeactivateIfPlayerdataTrue item in npc.GetComponents<DeactivateIfPlayerdataTrue>())
            Component.Destroy(item);
        foreach (DeactivateIfPlayerdataFalse item in npc.GetComponents<DeactivateIfPlayerdataFalse>())
            Component.Destroy(item);
        foreach (PlayMakerFSM item in npc.GetComponents<PlayMakerFSM>()?.Where(x => x.FsmName.StartsWith("Destroy") || x.FsmName == "FSM"
        || x.FsmName == "Death" || x.FsmName.StartsWith("Leave")
        || x.FsmName == "deactivate" || x.FsmName == "Appear"))
            Component.Destroy(item);
        if (Placement.Items.All(x => x.IsObtained()))
        {
            npc.SetActive(false);
            return;
        }
        else
        {
            if (TravellerName == Traveller.Quirrel && ObjectName == "Quirrel Wounded" && !PlayerData.instance.GetBool(nameof(PlayerData.instance.summonedMonomon)))
                npc.SetActive(false);
            else
            {
                int npcIndex;
                // Cloth town and her ghost share a placement, which is why we take the ghost for town Cloth.
                if (TravellerName == Traveller.Cloth && scene.name == "Town")
                    npcIndex = LoreManager.Instance.Traveller[TravellerName].Locations.IndexOf("Cloth Ghost NPC");
                else
                    npcIndex = LoreManager.Instance.Traveller[TravellerName].Locations.IndexOf(ObjectName);
                npc.SetActive(npcIndex <= LoreManager.Instance.Traveller[TravellerName].CurrentStage);
            }
        }
    }

    /// <summary>
    /// Tries to find the corresponding game object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private GameObject CheckComponent<T>(string name) where T : Component
    {
        // Since some traveller objects are child objects, we extract their simple name.
        string normalName = name.Split('/').Last();
        return Object.FindObjectsOfType<T>(true).FirstOrDefault(x => x.gameObject.name == normalName)?.gameObject;
    }

    /// <summary>
    /// Shuffle the order of the traveller stages.
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="shuffle">Determines if the order should be shuffled.</param>
    internal static List<string> DetermineOrder(Traveller npc, bool shuffle)
    {
        List<string> result = new();
        List<string> order = npc switch
        {
            Traveller.Quirrel => _quirrelNames.Select(x => x.Item2).ToList(),
            Traveller.Cloth => _clothNames.Select(x => x.Item2).ToList(),
            Traveller.Tiso => _tisoNames.Select(x => x.Item2).ToList(),
            _ => _zoteNames.Select(x => x.Item2).ToList()
        };

        if (!shuffle)
            return order;
        else
        {
            if (npc == Traveller.Cloth)
                order.RemoveAt(5);
            do
            {
                int selectedValue = LoreMaster.Instance.Generator.Next(0, order.Count - 1);
                result.Add(order[selectedValue]);
                order.RemoveAt(selectedValue);
            }
            while (order.Count > 1);
            // The last location stays always the last, even if randomized.
            result.Add(order[0]);
            foreach (string item in result)
                LoreMaster.Instance.Log("Location: " + item);
        }
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

        if (traveller == Traveller.Quirrel && index == 8)
            return new QuirrelArchiveLocation()
            {
                name = locationName,
                sceneName = data.Item1,
                TravellerName = traveller,
                FsmName = fsmName,
                ObjectName = data.Item2
            };
        else
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