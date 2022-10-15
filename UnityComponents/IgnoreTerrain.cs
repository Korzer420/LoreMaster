using UnityEngine;

namespace LoreMaster.UnityComponents;

public class IgnoreTerrain : MonoBehaviour
{
    void Start()
    {
        if(transform.Find("Terrain Buffer") != null)
            transform.Find("Terrain Buffer").GetComponent<BoxCollider2D>().enabled = false;
        if (transform.Find("TileDetector") != null)
            transform.Find("TileDetector").GetComponent<CircleCollider2D>().enabled = false;
        // The increased sorting layer should render the sprite in front of terrain.
        if (GetComponent<tk2dSprite>() is tk2dSprite sprite)
            sprite.SortingOrder = 1;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Allow the fleeing shade to move through terrain.
        if (collision.gameObject.layer == 8)
            Physics2D.IgnoreCollision(collision.collider, GetComponent<BoxCollider2D>());
    }
}
