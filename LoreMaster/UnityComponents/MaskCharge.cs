using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Extensions;
using System.Collections;
using UnityEngine;

namespace LoreMaster.UnityComponents;

public class MaskCharge : MonoBehaviour
{
    #region Members

    private GameObject[] _rune = new GameObject[2];

    private GameObject _hitbox;

    #endregion

    #region Unity Methods

    private void Start()
    {
        _hitbox = new("Hitbox");
        _hitbox.transform.SetParent(transform);
        _hitbox.transform.localPosition = new(0, 0);
        foreach (Transform thorn in transform.parent.Find("Charm Effects/Thorn Hit"))
        {
            GameObject hitbox = GameObject.Instantiate(thorn.gameObject, _hitbox.transform);
            hitbox.transform.localScale = new(1f, 1f);

            PlayMakerFSM damageFSM = hitbox.LocateMyFSM("damages_enemy");
            // Remove the knockback
            damageFSM.FsmVariables.FindFsmFloat("magnitudeMult").Value = 0f;

            // The ring damage count as spells.
            damageFSM.FsmVariables.FindFsmInt("attackType").Value = 2;

            PlayMakerFSM fsm = hitbox.LocateMyFSM("set_thorn_damage");
            fsm.FsmName = "set_ring_damage";
            fsm.GetState("Set").ReplaceAction(new Lambda(() =>
            {
                int nailDamage = PlayerData.instance.GetInt(nameof(PlayerData.instance.nailDamage)) / 2;
                if (nailDamage > 20)
                    nailDamage = 20;
                fsm.FsmVariables.FindFsmInt("Damage").Value = nailDamage;
            }), 0);
        }

        _rune[0] = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Battle Scene/HK Prime/Focus Blast/focus_ring"], transform);
        _rune[0].name = "Charge Ring";
        _rune[0].transform.localPosition = new(0, 0, 0);
        _rune[0].transform.localScale = new(1.5f, 1.5f, 1.5f);
        _rune[0].SetActive(true);
        _rune[0].GetComponent<SpriteRenderer>().color = new(1f, 1f, 1f);

        _rune[1] = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Battle Scene/HK Prime/Focus Blast/focus_rune"], transform);
        _rune[1].name = "Charge Rune";
        _rune[1].transform.localPosition = new(0, 0, 0);
        _rune[1].transform.localScale = new(1.5f, 1.5f, 1.5f);
        _rune[1].SetActive(true);
        _rune[1].GetComponent<SpriteRenderer>().color = new(1f, 1f, 1f, .1f);
    }

    private void OnEnable()
    {
        StartCoroutine(RotateRune());
        StartCoroutine(EmitDamage());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    #endregion

    #region Private Methods

    private IEnumerator RotateRune()
    {
        int currentRotation = 0;
        while (true)
        {
            yield return new WaitForSeconds(.02f);
            currentRotation += 2;
            if (currentRotation >= 360)
                currentRotation = 0;
            _rune[1].transform.SetRotationZ(currentRotation);
        }
    }

    private IEnumerator EmitDamage()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            _hitbox.SetActive(true);
            HeroController.instance.AddMPCharge(8);
            yield return new WaitForSeconds(0.1f);
            _hitbox.SetActive(false);
        }
    }

    #endregion
}
