using LoreMaster.Enums;
using LoreMaster.UnityComponents;
using SFCore.Utils;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes;

public class OneOfUsPower : Power
{
    #region Members

    private GameObject _cloud;

    #endregion

    #region Constructors

    public OneOfUsPower() : base("One of Us", Area.FungalWastes) { }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the cloud object
    /// </summary>
    public GameObject Cloud => _cloud == null ? _cloud = GameObject.Find("_GameManager").transform.Find("GlobalPool/Knight Spore Cloud(Clone)").gameObject : _cloud;

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable() => StartRoutine(() => EmitCloud());

    /// <inheritdoc/>
    protected override void TwistEnable() => StartRoutine(() => EmitCloud());
    
    #endregion

    #region Private Methods

    /// <summary>
    /// Emits the spore cloud.
    /// </summary>
    private IEnumerator EmitCloud()
    {
        while (true)
        {
            yield return new WaitForSeconds(12f);
            if ((State == PowerState.Twisted && !PlayerData.instance.GetBool("atBench") && HeroController.instance.acceptingInput) || (!InputHandler.Instance.inputActions.superDash.IsPressed && !GameManager.instance.isPaused))
            {
                GameObject newCloud = GameObject.Instantiate(Cloud, HeroController.instance.transform.position,
                Quaternion.identity);
                if (State == PowerState.Twisted)
                {
                    Component.Destroy(newCloud.GetComponent<DamageEffectTicker>());
                    newCloud.AddComponent<VenomZone>();
                }
                newCloud.SetActive(true);
                newCloud.LocateMyFSM("Control").ChangeTransition("Init", "NORMAL", "Deep");
                yield return new WaitForSeconds(4.1f);
                GameObject.Destroy(newCloud);
            }
        }
    }

    #endregion
}
