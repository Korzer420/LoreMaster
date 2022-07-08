using LoreMaster.Enums;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes;

public class OneOfUsPower : Power
{
    #region Members

    private GameObject _cloud;
    private Coroutine _cloudRoutine;

    #endregion
    
    #region Constructors

    public OneOfUsPower() : base("One of Us", Area.FungalWastes)
    {
        Hint = "Occasionally you emit a spore cloud. (Hold the super dash button to prevent the cloud.)";
        Description = "Every twelve seconds you cast the deep focus spore cloud. Hold the crystal dash button to prevent that (in case you want to do pogos for example).";
    }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        _cloud = GameObject.Find("_GameManager");
        _cloud = _cloud.transform.Find("GlobalPool/Knight Spore Cloud(Clone)").gameObject;
    }

    protected override void Enable() => _cloudRoutine = LoreMaster.Instance.Handler.StartCoroutine(EmitCloud());

    protected override void Disable() => LoreMaster.Instance.Handler.StopCoroutine(_cloudRoutine);

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
                var newc = GameObject.Instantiate(_cloud, HeroController.instance.transform.position,
                Quaternion.identity);
                newc.SetActive(true);
                newc.SetActiveChildren(true);
                yield return new WaitForSeconds(4.5f);
                GameObject.Destroy(newc);
            }
        }
    }

    #endregion

}
