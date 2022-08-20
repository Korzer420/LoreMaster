using LoreMaster.LorePowers.QueensGarden;
using System.Collections;
using UnityEngine;

namespace LoreMaster.UnityComponents;

internal class Bomb : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Ticking());
    }

    private IEnumerator Ticking()
    {
        float passedTime = 0f;
        float passedMilestone = 0f;
        Color bombColor = gameObject.name.Equals("Power bomb") ? Color.white : Color.green;
        Color currentColor = bombColor;
        GetComponent<SpriteRenderer>().color = currentColor;
        while (passedTime < 3f)
        {
            if ((passedMilestone >= 0.5f && passedTime < 1f)
                || (passedMilestone >= .25f && passedTime >= 1f && passedTime <= 2f)
                || (passedMilestone >= .125f && passedTime > 2f))
            {
                passedMilestone = 0f;
                currentColor = currentColor == bombColor ? Color.red : bombColor;
                GetComponent<SpriteRenderer>().color = currentColor;
            }
            passedMilestone += Time.deltaTime;
            passedTime += Time.deltaTime;
            yield return null;
        }

        // Boom
        GameObject explosion = GameObject.Instantiate(GrassBombardementPower.Explosion);
        explosion.name = "Bomb Explosion";
        explosion.SetActive(false);
        ParticleSystem.MainModule settings = explosion.GetComponentInChildren<ParticleSystem>().main;
        settings.startColor = new ParticleSystem.MinMaxGradient(string.Equals(gameObject.name, "Power bomb") ? Color.cyan : Color.green);
        explosion.LocateMyFSM("damages_enemy").FsmVariables.FindFsmInt("damageDealt").Value = PlayerData.instance.GetBool(nameof(PlayerData.instance.equippedCharm_19))
            ? (gameObject.name.Equals("Power bomb") ? 90 : 60)
            : (gameObject.name.Equals("Power bomb") ? 60 : 40);
        explosion.transform.localPosition = transform.localPosition;
        explosion.transform.localScale = gameObject.name.Equals("Power bomb")
            ? new(2f, 2f, explosion.transform.localScale.z)
            : new(1.2f, 1.2f, explosion.transform.localScale.z);

        explosion.GetComponent<CircleCollider2D>().isTrigger = true;
        explosion.AddComponent<Rigidbody2D>().gravityScale = 0f;
        explosion.SetActive(true);
        
        if(string.Equals(gameObject.name, "Power bomb"))
            PlayMakerFSM.BroadcastEvent("POWERBOMBED");
        else
            PlayMakerFSM.BroadcastEvent("BOMBED");
        LoreMaster.Instance.Handler.StartCoroutine(FixGround());
        GameObject.Destroy(gameObject);
    }

    private static IEnumerator FixGround()
    {
        yield return new WaitForSeconds(0.2f);
        PlayMakerFSM.BroadcastEvent("VANISHED");
    }
}
