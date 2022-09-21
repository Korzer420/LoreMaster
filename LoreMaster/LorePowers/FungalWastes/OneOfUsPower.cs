using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using System.Collections;
using UnityEngine;
using SFCore.Utils;

namespace LoreMaster.LorePowers.FungalWastes;

public class OneOfUsPower : Power
{
    #region Members

    private GameObject _cloud = GameObject.Find("_GameManager").transform.Find("GlobalPool/Knight Spore Cloud(Clone)").gameObject;

    #endregion

    #region Constructors

    public OneOfUsPower() : base("One of Us", Area.FungalWastes) { }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the cloud object
    /// </summary>
    public GameObject Cloud => _cloud == null ? GameObject.Find("_GameManager").transform.Find("GlobalPool/Knight Spore Cloud(Clone)").gameObject : _cloud;

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Enable() => _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(EmitCloud());

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
            if (!InputHandler.Instance.inputActions.superDash.IsPressed && !GameManager.instance.isPaused)
            {
                GameObject newCloud = GameObject.Instantiate(Cloud, HeroController.instance.transform.position,
                Quaternion.identity);
                newCloud.SetActive(true);
                newCloud.LocateMyFSM("Control").ChangeTransition("Init", "NORMAL", "Deep");
                yield return new WaitForSeconds(4.1f);
                GameObject.Destroy(newCloud);
            }
        }
    }

    #endregion
}
