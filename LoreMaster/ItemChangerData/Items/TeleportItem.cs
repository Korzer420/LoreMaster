using ItemChanger;
using System.Collections;
using UnityEngine;

namespace LoreMaster.ItemChangerData.Items;

internal class TeleportItem : AbstractItem
{
    public bool ToTemple { get; set; }

    public override void GiveImmediate(GiveInfo info)
    {
        LoreMaster.Instance.Handler.StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        // To prevent the player from being locked out of city. Taking the express automatically unlocks the store room stag.
        PlayerData.instance.SetBool(nameof(PlayerData.instance.openedRuins1), true);
        yield return null;

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
