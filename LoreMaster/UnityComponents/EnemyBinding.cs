using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LoreMaster.LorePowers.RestingGrounds;

namespace LoreMaster.UnityComponents;

/// <summary>
/// Used by <see cref="DreamBlessingPower"/> Component to bind the enemy to the current position. This component removes itself after the cooldown.
/// </summary>
public class EnemyBinding : MonoBehaviour
{
    #region Members

    private float _cooldown = 0f;

    private Vector3 _positionToHold;

    #endregion

    void Start()
    {
        // Store current position
        _positionToHold = transform.position;
        StartCoroutine(PullBack());
    }

    void Update()
    {
        _cooldown += Time.deltaTime;
        if (_cooldown >= 15f)
            Component.Destroy(this);
    }

    private IEnumerator PullBack()
    {
        float passedTime = 0f;
        tk2dSprite sprite = gameObject.GetComponent<tk2dSprite>();
        // Purple blink color
        Color purple = new(1f, 0f, 1f);
        // This is used to let the enemy blink every 0.25 seconds.
        float blinkMilestone = 0f;

        while (passedTime <= (PlayerData.instance.GetBool(nameof(PlayerData.instance.dreamNailUpgraded)) ? 5f : 2.5f))
        {
            transform.position = _positionToHold;
            passedTime += Time.deltaTime;
            if (sprite != null && passedTime >= blinkMilestone + 0.25f)
            {
                sprite.color = sprite.color == Color.white ? purple : Color.white;
                blinkMilestone += .25f;
            }
            yield return null;
        }
        if (sprite != null)
            sprite.color = Color.white;
    }
}
