using LoreMaster.Enums;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes;

public class MantisStylePower : Power
{
    #region Members
    
    private Transform[] _attackTransform = new Transform[4]; 

    #endregion

    #region Constructors

    public MantisStylePower() : base("Mantis Style", Area.FungalWastes)
    {
        Hint = "Your weapon may reach further aways foes.";
        Description = "Increase your nail range by 50% (from base).";
    } 

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        GameObject attackDirections = GameObject.Find("Knight/Attacks");
        _attackTransform[0] = attackDirections.transform.Find("Slash");
        _attackTransform[1] = attackDirections.transform.Find("AltSlash");
        _attackTransform[2] = attackDirections.transform.Find("UpSlash");
        _attackTransform[3] = attackDirections.transform.Find("DownSlash");
    }

    protected override void Enable()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3 currentScale = _attackTransform[i].GetComponent<NailSlash>().scale;
            if (i < 2)
                _attackTransform[i].GetComponent<NailSlash>().scale = new Vector3(currentScale.x + .5f, currentScale.y, currentScale.z);
            else
                _attackTransform[i].GetComponent<NailSlash>().scale = new Vector3(currentScale.x, currentScale.y + .5f, currentScale.z);
        }
    }

    protected override void Disable()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3 currentScale = _attackTransform[i].GetComponent<NailSlash>().scale;
            if (i < 2)
                _attackTransform[i].GetComponent<NailSlash>().scale = new Vector3(currentScale.x - .5f, currentScale.y, currentScale.z);
            else
                _attackTransform[i].GetComponent<NailSlash>().scale = new Vector3(currentScale.x, currentScale.y - .5f, currentScale.z);
        }
    } 

    #endregion
}
