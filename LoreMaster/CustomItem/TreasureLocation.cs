using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Components;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Util;
using LoreMaster.LorePowers.CityOfTears;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoreMaster.CustomItem
{
    internal class TreasureLocation : ContainerLocation
    {
        private int _treasureIndex;
        private static readonly List<(string, Vector3)> _coordinates = new()
        {
            ("Cliffs_01", new(106.41f, 127.41f)),
            ("Crossroads_42", new (7.052f, 13.41f)),
            ("Fungus1_Slug", new (44.78f, 14.41f)),
            ("Fungus2_10",new (5.8f, 14.41f)),
            ("Fungus3_Archive_02",new (96f, 92.41f)) ,
            ("Ruins2_05",new(27.42f,68.41f)) ,
            ("Waterways_13",new(43.625f, 47.41f)) ,
            ("Deepnest_30",new(40.1f, 138.41f)) ,
            ("Abyss_06_Core",new(94.6f, 108.41f)) ,
            ("GG_Lurker",new(124.3f, 52.41f)) ,
            ("Mines_02",new(155.1f, 30.41f)) ,
            ("RestingGrounds_08",new(7.74f,58.41f)) ,
            ("Fungus3_04",new(6f, 5.41f)) ,
            ("White_Palace_01",new(99.3f, 129.41f))
        };

        protected override void OnLoad()
        {
            Events.AddSceneChangeEdit(sceneName, SceneChange);
        }

        protected override void OnUnload()
        {
            Events.RemoveSceneChangeEdit(sceneName, SceneChange);
        }

        private void SceneChange(UnityEngine.SceneManagement.Scene currentScene)
        {
            // Only spawn the treasure, if the chart has been obtained
            if (TreasureHunterPower.HasCharts[_treasureIndex] && !Placement.Items.All(x => x.IsObtained()))
            {
                // Since I'm too incompetent to setup my own fsm, I just take the quake floor one and strip all features off (:
                GameObject markedGround = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Quake Floor"]);
                markedGround.SetActive(false);
                markedGround.name = "Treasure Ground";
                markedGround.transform.position = _coordinates[_treasureIndex].Item2;
                markedGround.transform.localScale = new(3f, 1f, 1f);
                foreach (Transform child in markedGround.transform)
                    GameObject.Destroy(child.gameObject);

                // Remove unnecessary components.
                Component.Destroy(markedGround.GetComponent<PersistentBoolItem>());
                Component.Destroy(markedGround.LocateMyFSM("quake_floor_shake"));
                Component.Destroy(markedGround.GetComponent<PlayMakerCollisionExit2D>());
                Component.Destroy(markedGround.GetComponent<AudioSource>());
                PlayMakerFSM fsm = markedGround.LocateMyFSM("quake_floor");
                fsm.FsmName = "Treasure Ground";
                fsm.AddState(new FsmState(fsm.Fsm)
                {
                    Name = "Throw Item",
                    Actions = new FsmStateAction[]
                    {
                       new Lambda(() =>
                       {
                           Container container = Container.GetContainer(Container.Shiny);
                           GameObject treasure = container.GetNewContainer(new ContainerInfo(container.Name, Placement, FlingType.StraightUp));
                           ShinyUtility.FlingShinyRandomly(treasure.LocateMyFSM("Shiny Control"));
                           container.ApplyTargetContext(markedGround, treasure, 0);
                       }),
                       new Lambda(() => GameObject.Destroy(markedGround))
                    }
                });
                fsm.GetState("Pause").ClearTransitions();
                fsm.GetState("Pause").AddTransition("FINISHED", "Solid");
                fsm.GetState("Transient").ClearTransitions();
                fsm.GetState("Transient").AddTransition("DESTROY", "Throw Item");
                markedGround.SetActive(true);
            }
        }

        internal static TreasureLocation GenerateLocation(int index) => new()
        {
            flingType = FlingType.StraightUp,
            sceneName = _coordinates[index].Item1,
            _treasureIndex = index,
            name = "Treasure_" + (index + 1)
        };
    }
}
