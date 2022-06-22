using GlobalEnums;
using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using Modding;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster
{
    class TouristMagnetItem : AbstractItem
    {
        private bool _toTemple;
        private string _description = "Want to see the mighty temple? I'll bring you there, for a small fee.";
        private static FastReflectionDelegate HCFinishedEnteringScene = typeof(HeroController)
            .GetMethod("FinishedEnteringScene", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .GetFastDelegate();

        public TouristMagnetItem(bool toTemple, string itemName)
        {
            if (!toTemple)
                _description = "The second text";
            _toTemple = toTemple;
            name = itemName;
            UIDef = new MsgUIDef()
            {
                name = new BoxedString(_description),
                shopDesc = new BoxedString(_description),
                sprite = new EmbeddedSprite("Lore")
            };
            tags = new List<Tag>()
            {
                new PersistentItemTag() { Persistence = Persistence.Persistent},
                new CompletionWeightTag() { Weight = 0}
            };
        }

        public override void GiveImmediate(GiveInfo info)
        {
            HeroController.instance.StartCoroutine(Travel());
        }

        
        IEnumerator Travel()
        {
            string destinationScene = _toTemple ? "ROOM_TEMPLE" : "Ruins1_27";

            HeroController.instance.IgnoreInputWithoutReset();

            //yes this is a savestate load
            GameManager.instance.entryGateName = "dreamGate";
            GameManager.instance.startedOnThisScene = true;

            GameManager.instance.BeginSceneTransition
            (
                new GameManager.SceneLoadInfo
                {
                    SceneName = destinationScene,
                    HeroLeaveDirection = GatePosition.unknown,
                    EntryGateName = "dreamGate",
                    EntryDelay = 0f,
                    WaitForSceneTransitionCameraFade = false,
                    Visualization = 0,
                    AlwaysUnloadUnusedAssets = true
                }
            );

            Modding.ReflectionHelper.SetField(GameManager.instance.cameraCtrl, "isGameplayScene", true);

            GameManager.instance.cameraCtrl.PositionToHero(false);

            yield return new WaitUntil(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == destinationScene);

            GameManager.instance.cameraCtrl.FadeSceneIn();

            GameCameras.instance.hudCanvas.gameObject.SetActive(true);

            Modding.ReflectionHelper.SetField(GameManager.instance.cameraCtrl, "isGameplayScene", true);

            yield return null;

            //// City Spawn Point = 50,3785X  23,4081Y
            //// Temple Item Point = 56,6141X 3,4081Y
            //// Temple Spawn Point = 17,5454X 3,4081Y
            Vector3? HeroPos = _toTemple ? new(17.55f, 3.41f) : new(50.38f, 23.41f);

            HeroController.instance.transform.position = HeroPos.Value;
            HeroController.instance.cState.inConveyorZone = false;
            HeroController.instance.cState.onConveyor = false;
            HeroController.instance.cState.onConveyorV = false;
            HCFinishedEnteringScene(HeroController.instance, true, false);
            yield return null;

            GameCameras.instance.StopCameraShake();
        }
    }
}
