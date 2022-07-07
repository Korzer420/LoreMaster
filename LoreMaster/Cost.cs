using ItemChanger;
using LoreMaster.LorePowers.CityOfTears;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster
{
    internal record Paypal: Cost
    {
        public bool ToTemple { get; set; }

        public override bool CanPay()
        => TouristPower.Inspected && PlayerData.instance.geo > 49;

        public override string GetCostText()
        => TouristPower.Inspected ? "Take a ticket?" : "Currently closed";

        public override int GetDisplayGeo() => 50;
        
        public override bool HasPayEffects()
        => false;

        public override void OnPay()
        {
            HeroController.instance.TakeGeo(50);
            LoreMaster.Instance.Handler.StartCoroutine(Wait());
        }

        IEnumerator Wait()
        {
            yield return null;
            Paid = false;

            string destinationScene = ToTemple ? "Room_Temple" : "Ruins1_27";
            GameManager.instance.cameraCtrl.FadeOut(GlobalEnums.CameraFadeType.JUST_FADE);
            yield return new WaitForSeconds(1f);
            GameManager.instance.ChangeToScene(destinationScene, ToTemple ? "left1" : "right1", .25f);

            yield return new WaitUntil(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == destinationScene);

            if (ToTemple)
                yield break;

            yield return new WaitForFinishedEnteringScene();

            HeroController.instance.transform.position = new(55.38f, 23.41f);
        }
    }
}
