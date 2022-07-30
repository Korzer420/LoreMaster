using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.HowlingCliffs;

public class LifebloodOmenPower : Power
{
    #region Members

    private GameObject _ghostPrefab;

    private GameObject _ghost;

    #endregion

    #region Constructors

    public LifebloodOmenPower() : base("Lifeblood Wings", Area.Cliffs)
    {
        Hint = "[BETA] Sometimes you will be haunted by a ghost from a distant land. Killing it will grant you the essence of it's soul.";
        Description = "[BETA] Spawns a grimmkin every 180 seconds. Killing the ghost grants 3 lifeblood. The ghost disappears if you leave the room or if 60 seconds passed.";
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override void Initialize() => _ghostPrefab = LoreMaster.Instance.PreloadedObjects["Flamebearer Spawn"].LocateMyFSM("Spawn Control").FsmVariables.FindFsmGameObject("Grimmkin Obj").Value;
    
    /// <inheritdoc/>
    protected override void Enable() => _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(Haunt());
    
    #endregion

    #region Private Methods

    private IEnumerator Haunt()
    {
        while (true)
        {
            yield return new WaitForSeconds(120f);
            if (PlayerData.instance.GetBool(nameof(PlayerData.instance.atBench)))
                yield return new WaitUntil(() => !PlayerData.instance.GetBool(nameof(PlayerData.instance.atBench)));
            _ghost = GameObject.Instantiate(_ghostPrefab);
            _ghost.transform.localPosition = HeroController.instance.transform.localPosition + new Vector3(0f, 3f, 0f);
            _ghost.name = "Lifeblood Ghost";
            _ghost.GetComponent<tk2dSprite>().color = Color.cyan;
            PlayMakerFSM fsm = _ghost.LocateMyFSM("Control");
            fsm.GetState("Set Level").RemoveAction(0);
            fsm.GetState("Set Level").AddLastAction(new Lambda(() => 
            {
                fsm.FsmVariables.FindFsmInt("Grimmchild Level").Value = 1;
                fsm.SendEvent("LEVEL 1");
            }));
            // Prevent grimm music playing.
            fsm.GetState("Alert Pause").RemoveTransitionsTo("Music");
            fsm.GetState("Alert Pause").AddTransition("FINISHED", "Set Angle");

            // Remove accordion fanfare
            fsm.GetState("Fanfare 1").RemoveAction(0);

            // Reward
            fsm.GetState("Destroy").AddFirstAction(new Lambda(() =>
            {
                for (int i = 0; i < 3; i++)
                    EventRegister.SendEvent("ADD BLUE HEALTH");
            }));
            fsm.SendEvent("START");
            yield return new WaitForSeconds(60f);
            if (_ghost != null)
            {
                PlayMakerFSM.BroadcastEvent("DREAM AREA DISABLE");
                GameObject.Destroy(_ghost);
            }
        }
    }

    #endregion
}
