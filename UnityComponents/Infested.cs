using LoreMaster.LorePowers.Deepnest;
using UnityEngine;

namespace LoreMaster.UnityComponents;

/// <summary>
/// Used for <see cref="InfestedPower"/>.
/// </summary>
public class Infested : MonoBehaviour
{
    private int _eggs = 1;

    /// <summary>
    /// Gets or sets the eggs amount (determines the weavers spawn). Capped at 5.
    /// </summary>
    public int Eggs
    {
        get => _eggs;
        set
        {
            if (value > 5)
                _eggs = 5;
            else
                _eggs = value;
        }
    }
}
