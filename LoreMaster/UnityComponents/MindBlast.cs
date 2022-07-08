using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LoreMaster.LorePowers.Greenpath;

namespace LoreMaster.UnityComponents;

/// <summary>
/// Component used for <see cref="UnnMindblastPower"/> to amplify the damage dealt.
/// </summary>
public class MindBlast : MonoBehaviour
{
    public int ExtraDamage { get; set; } = 0;
}
