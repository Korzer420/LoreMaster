using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace LoreMaster.LorePowers.HowlingCliffs;

public class LifebloodOmenPower : Power
{
    #region Members

    private GameObject[] _ghostPrefabs = new GameObject[3];

    private GameObject _ghost;

    #endregion

    #region Constructors

    public LifebloodOmenPower() : base("Lifeblood Omen", Area.Cliffs) { }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        _ghostPrefabs[0] = LoreMaster.Instance.PreloadedObjects["Small Ghost"];
        _ghostPrefabs[1] = LoreMaster.Instance.PreloadedObjects["Medium Ghost"];
        _ghostPrefabs[2] = LoreMaster.Instance.PreloadedObjects["Large Ghost"];
    }

    /// <inheritdoc/>
    protected override void Enable() => _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(Haunt());

    /// <inheritdoc/>
    protected override void Disable()
    {
        if (_ghost != null)
        {
            PlayMakerFSM.BroadcastEvent("DREAM AREA DISABLE");
            GameObject.Destroy(_ghost);
        }
    }

    /// <inheritdoc/>
    protected override void TwistEnable() => Enable();

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        Disable();
        PlayMakerFSM.BroadcastEvent("DREAM GATE OPEN");
    }

    #endregion

    #region Private Methods

    private IEnumerator Haunt()
    {
        while (true)
        {
            float passedTime = 0f;
            do
            {
                yield return null;
                if (PlayerData.instance.GetInt("health") == PlayerData.instance.GetInt("maxHealth"))
                    passedTime += Time.deltaTime;
            } 
            while (passedTime < 180f);
            // If a player sits a bench herocontroller doesn't accept input, which makes the first part redundant... I think. I still keep it, just in case.
            if (PlayerData.instance.GetBool(nameof(PlayerData.instance.atBench)) || !HeroController.instance.acceptingInput)
                yield return new WaitUntil(() => !PlayerData.instance.GetBool(nameof(PlayerData.instance.atBench)) && HeroController.instance.acceptingInput);
            int index = DetermineGhost();
            _ghost = GameObject.Instantiate(_ghostPrefabs[index]);
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
            fsm.GetState("Explode").ReplaceAction(new Lambda(() =>
            {
                fsm.FsmVariables.FindFsmGameObject("Explode Effects").Value.SetActive(true);
                if (State == PowerState.Twisted)
                    PlayMakerFSM.BroadcastEvent("DREAM GATE OPEN");
                else
                    for (int i = 0; i < 3 * (index + 1); i++)
                        EventRegister.SendEvent("ADD BLUE HEALTH");

            }), 4);
            fsm.SendEvent("START");
            if (State == PowerState.Twisted)
            {
                foreach (TransitionPoint transition in GameObject.FindObjectsOfType<TransitionPoint>())
                {
                    if (!transition.isADoor)
                    {
                        GameObject blocker = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Dream Gate"]);
                        blocker.SetActive(true);
                        GameObject.Destroy(blocker.transform.GetChild(0));
                        blocker.LocateMyFSM("Control").GetState("Init").RemoveAction(1);
                        blocker.transform.localScale = new(1f, 1f);
                        // Remove camera lock.
                        GameObject.Destroy(blocker.transform.GetChild(0)); 
                        if (transition.name.Contains("left"))
                        {
                            blocker.transform.eulerAngles = new Vector3(0f, 0f, 0f);
                            blocker.transform.position = transition.transform.position + new Vector3(1f, 0f);
                            blocker.GetComponent<BoxCollider2D>().size = transition.GetComponent<BoxCollider2D>().size;
                        }
                        else if (transition.name.Contains("right"))
                        {
                            blocker.transform.eulerAngles = new Vector3(0f, 0f, 180f);
                            blocker.transform.position = transition.transform.position - new Vector3(1f, 0f);
                            blocker.GetComponent<BoxCollider2D>().size = transition.GetComponent<BoxCollider2D>().size;
                        }
                        else if (transition.name.Contains("top"))
                        {
                            blocker.transform.eulerAngles = new Vector3(0f, 0f, 270f);
                            Vector3 vector3 = new(transition.GetComponent<BoxCollider2D>().size.y, transition.GetComponent<BoxCollider2D>().size.x);
                            blocker.GetComponent<BoxCollider2D>().size = vector3;
                            blocker.transform.position = transition.transform.position - new Vector3(0f, 1f);
                        }
                        else
                        {
                            blocker.transform.eulerAngles = new Vector3(0f, 0f, 90f);
                            Vector3 vector3 = new(transition.GetComponent<BoxCollider2D>().size.y, transition.GetComponent<BoxCollider2D>().size.x);
                            blocker.GetComponent<BoxCollider2D>().size = vector3;
                            blocker.transform.position = transition.transform.position + new Vector3(0f, 1f);
                        }
                        blocker.GetComponent<tk2dSprite>().color = new(0f, .5f, 1f);
                    }
                }
                PlayMakerFSM.BroadcastEvent("DREAM GATE CLOSE");
            }
            float activeTime = 0f;
            while ((activeTime < 90f || State == PowerState.Twisted) && _ghost != null)
            {
                activeTime += Time.deltaTime;
                yield return null;
            }
            if (_ghost != null)
            {
                PlayMakerFSM.BroadcastEvent("DREAM AREA DISABLE");
                GameObject.Destroy(_ghost);
                if (State == PowerState.Twisted)
                    PlayMakerFSM.BroadcastEvent("DREAM GATE OPEN");
            }
        }
    }

    /// <summary>
    /// Determines which of the three ghosts should spawn.
    /// </summary>
    /// <returns>The index of the ghost in the array.</returns>
    private int DetermineGhost()
    {
        int nailLevel = PlayerData.instance.GetInt(nameof(PlayerData.instance.nailSmithUpgrades));
        switch (nailLevel)
        {
            case 1:
                // 20% chance for medium ghost.
                return LoreMaster.Instance.Generator.Next(1, 6) == 1 ? 1 : 0;
            case 2:
                // 40 % chance for medium ghost.
                return LoreMaster.Instance.Generator.Next(1, 6) <= 2 ? 1 : 0;
            case 3:
                // 50 % chance for medium, 35 % for small and 15 % for large ghost.
                int result = LoreMaster.Instance.Generator.Next(1, 21);
                return result <= 10 ? 1 : (result <= 17 ? 0 : 2);
            case 4:
                // 55 % for medium, 35% chance for large and 10% for small ghost.
                int highRoll = LoreMaster.Instance.Generator.Next(1, 21);
                return highRoll <= 7 ? 2 : (highRoll <= 18 ? 1 : 0);
            default:
                return 0;
        }

    }

    #endregion
}
