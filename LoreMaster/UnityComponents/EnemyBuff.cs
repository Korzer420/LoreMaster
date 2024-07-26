using UnityEngine;

namespace LoreMaster.UnityComponents;

/// <summary>
/// Dummy class that can be used by eventhandler of powers to determine if they should apply their effect.
/// </summary>
public class EnemyBuff : MonoBehaviour
{
    public string PowerName;

    public GameObject Enemy;
}
