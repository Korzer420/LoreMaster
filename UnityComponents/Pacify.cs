using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.UnityComponents;

internal class Pacify : MonoBehaviour
{
    #region Members

    private List<Collider2D> _colliders = new();

    #endregion

    void Start()
    {
        // Ignore gruz mother.
        if (string.Equals(gameObject.name, "Giant Fly"))
        { 
            Destroy(this);
            return;
        }

        if (gameObject.GetComponent<DamageHero>() == null)
        {
            _colliders.AddRange(gameObject.GetComponents<Collider2D>());
            _colliders.AddRange(gameObject.GetComponents<BoxCollider2D>());
            _colliders.AddRange(gameObject.GetComponents<CircleCollider2D>());
            _colliders.AddRange(gameObject.GetComponents<PolygonCollider2D>());
        }
        foreach (Transform child in transform)
        {
            if (child.GetComponent<DamageHero>() == null)
            {
                _colliders.AddRange(child.GetComponents<Collider2D>());
                _colliders.AddRange(child.GetComponents<BoxCollider2D>());
                _colliders.AddRange(child.GetComponents<CircleCollider2D>());
                _colliders.AddRange(child.GetComponents<PolygonCollider2D>());
            }
        }
    }

    void Update()
    {
        foreach (Collider2D collider in _colliders)
            collider.enabled = false;
    }

    void OnDestroy()
    {
        foreach (Collider2D collider in _colliders)
            collider.enabled = true;
    }
}
