using KorzUtils.Helper;
using LoreMaster.Manager;
using System.Collections;
using UnityEngine;

namespace LoreMaster.UnityComponents;

internal class VenomZone : MonoBehaviour
{
    private CircleCollider2D _cloudCollider;
    private Coroutine _poison;

    void Start()
    {
        _cloudCollider = GetComponent<CircleCollider2D>();
        _cloudCollider.gameObject.layer = 8;
        Physics2D.IgnoreCollision(_cloudCollider, HeroHelper.Collider, false);
    }

    void Update()
    {
        if (HeroHelper.Collider.IsTouching(_cloudCollider) || _cloudCollider.IsTouching(HeroHelper.Collider))
        {
            if (_poison == null)
                _poison = StartCoroutine(Poison());
        }
        else if (_poison != null)
        { 
            StopCoroutine(_poison);
            HeroHelper.Sprite.color = Color.white;
        }
    }

    void OnDestroy() => HeroHelper.Sprite.color = Color.white;

    private IEnumerator Poison()
    {
        float passedTime = 0f;
        while(passedTime < 2.5f)
        {
            passedTime += Time.deltaTime;
            HeroHelper.Sprite.color = new Color(1f,1f, 1f - passedTime / 2.5f);
            yield return null;
        }
        HeroController.instance.TakeDamage(null, GlobalEnums.CollisionSide.bottom, 1, 0);
        HeroHelper.Sprite.color = Color.white;
    }
}
