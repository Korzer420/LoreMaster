using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.UnityComponents;

public class ConcussionEffect : MonoBehaviour
{
    #region Members
    
    private float _passedTime;

    private BoxCollider2D _parentCollider; 

    #endregion

    private void Start()
    {
        Destroy(GetComponent<PlayMakerFSM>());
        Destroy(GetComponent<TinkEffect>());
        Destroy(GetComponent<DamageHero>());
        transform.localScale = new(.5f, .5f);
        _parentCollider = transform.parent.GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        // Adjust the position
        transform.localPosition = new(0, 0 + _parentCollider.size.y / 2);
        _passedTime += Time.deltaTime;
        if (_passedTime >= ConcussiveTime)
            GameObject.Destroy(this.gameObject);
    }

    /// <summary>
    /// Gets or sets the time which has to pass, before an enemy recovers the concussion. Can be extendend by nail hits.
    /// </summary>
    public float ConcussiveTime { get; set; } = 3f;
}
