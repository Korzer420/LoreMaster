using LoreMaster.Enums;
using System.Linq;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes;

public class MantisStylePower : Power
{
    #region Members

    private GameObject[] _attachObjects = new GameObject[5];

    #endregion

    #region Constructors

    public MantisStylePower() : base("Mantis Style", Area.FungalWastes) { }

    #endregion

    #region Properties

    public GameObject[] Attacks
    {
        get
        {
            if (_attachObjects.Any(x => x == null))
                Initialize();
            return _attachObjects;
        }
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        GameObject attackDirections = GameObject.Find("Knight/Attacks");
        _attachObjects[0] = attackDirections.transform.Find("Slash").gameObject;
        _attachObjects[1] = attackDirections.transform.Find("AltSlash").gameObject;
        _attachObjects[2] = attackDirections.transform.Find("UpSlash").gameObject;
        _attachObjects[3] = attackDirections.transform.Find("DownSlash").gameObject;
        _attachObjects[4] = attackDirections.transform.Find("WallSlash").gameObject;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3 currentScale = Attacks[i].GetComponent<NailSlash>().scale;
            if (i < 2)
                Attacks[i].GetComponent<NailSlash>().scale = new Vector3(currentScale.x + .4f, currentScale.y, currentScale.z);
            else
                Attacks[i].GetComponent<NailSlash>().scale = new Vector3(currentScale.x, currentScale.y + .4f, currentScale.z);
        }
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3 currentScale = Attacks[i].GetComponent<NailSlash>().scale;
            if (i < 2)
                Attacks[i].GetComponent<NailSlash>().scale = new Vector3(currentScale.x - .4f, currentScale.y, currentScale.z);
            else
                Attacks[i].GetComponent<NailSlash>().scale = new Vector3(currentScale.x, currentScale.y - .4f, currentScale.z);
        }
    }

    #endregion
}
