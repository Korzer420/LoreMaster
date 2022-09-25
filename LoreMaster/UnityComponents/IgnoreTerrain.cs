using UnityEngine;

namespace LoreMaster.UnityComponents;

public class IgnoreTerrain : MonoBehaviour
{
    void Start()
    {
        if(transform.Find("Terrain Buffer") != null)
        transform.Find("Terrain Buffer").GetComponent<BoxCollider2D>().enabled = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Allow the fleeing shade to move through terrain.
        if (collision.gameObject.layer == 8)
            Physics2D.IgnoreCollision(collision.collider, GetComponent<BoxCollider2D>());
    }
}
