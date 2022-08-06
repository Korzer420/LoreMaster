using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes;

public class OneOfUsPower : Power
{
    #region Members

    private GameObject _cloud;

    #endregion

    #region Constructors

    public OneOfUsPower() : base("One of Us", Area.FungalWastes)
    {
        Hint = "Occasionally you emit a spore cloud. (Hold the super dash button to prevent the cloud.)";
        Description = "Every twelve seconds you cast the deep focus spore cloud. Hold the crystal dash button to prevent that (in case you want to do pogos for example).";
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize()
    {
        _cloud = GameObject.Find("_GameManager");
        _cloud = _cloud.transform.Find("GlobalPool/Knight Spore Cloud(Clone)").gameObject;
    }

    /// <inheritdoc/>
    protected override void Enable() => _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(EmitCloud());

    /// <inheritdoc/>
    protected override void Disable()
    {
        if (_runningCoroutine != null)
            LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
    }

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
                GameObject newCloud = GameObject.Instantiate(_cloud, HeroController.instance.transform.position,
                Quaternion.identity);
                newCloud.LocateMyFSM("Control").GetState("Init").ReplaceAction(new Lambda(() => 
                {
                    newCloud.LocateMyFSM("Control").SendEvent("DEEP");
                }) { Name = "Force Deep Cloud" }, 2);
                newCloud.SetActive(true);
                yield return new WaitForSeconds(4.5f);
                GameObject.Destroy(newCloud);
            }
        }
    }

    #endregion
}
