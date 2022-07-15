using ItemChanger;
using LoreMaster.LorePowers.CityOfTears;
using System.Collections;
using UnityEngine;

namespace LoreMaster.CustomItem;

internal record Paypal: Cost
{
    public bool ToTemple { get; set; }

    public override bool CanPay()
    => TouristPower.Inspected && PlayerData.instance.GetInt(nameof(PlayerData.instance.geo)) > 49;

    public override string GetCostText()
    => TouristPower.Inspected ? "Take a ticket? (50 Geo)" : "Currently closed";

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
