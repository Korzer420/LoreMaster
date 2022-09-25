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
        Physics2D.IgnoreCollision(_cloudCollider, HeroManager.Collider, false);
    }

    void Update()
    {
        if (HeroManager.Collider.IsTouching(_cloudCollider) || _cloudCollider.IsTouching(HeroManager.Collider))
        {
            if (_poison == null)
                _poison = StartCoroutine(Poison());
        }
        else if (_poison != null)
        { 
            StopCoroutine(_poison);
            HeroManager.Sprite.color = Color.white;
        }
    }

    void OnDestroy() => HeroManager.Sprite.color = Color.white;

    private IEnumerator Poison()
    {
        LoreMaster.Instance.Log("Activate Poison");
        
        float passedTime = 0f;
        while(passedTime < 2.5f)
        {
            passedTime += Time.deltaTime;
            HeroManager.Sprite.color = new Color(1f,1f, 1f - passedTime / 2.5f);
            yield return null;
        }
        HeroController.instance.TakeDamage(null, GlobalEnums.CollisionSide.bottom, 1, 0);
        HeroManager.Sprite.color = Color.white;
    }
}
