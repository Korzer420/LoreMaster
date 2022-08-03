using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.HowlingCliffs;

public class LifebloodOmenPower : Power
{
    #region Members

    private GameObject[] _ghostPrefabs = new GameObject[3];

    private GameObject _ghost;

    #endregion

    #region Constructors

    public LifebloodOmenPower() : base("Lifeblood Omen", Area.Cliffs)
    {
        Hint = "[BETA] Sometimes you will be haunted by a ghost from a distant land. Killing it will grant you the essence of it's soul. A more powerful weapon may attract more powerful foes.";
        Description = "[BETA] Spawns a grimmkin every 180 seconds. Killing the ghost grants 3/6/9 lifeblood (based on ghost level). The ghost disappears if you leave the room or if 90 seconds passed. The chances for the ghost " +
            "adjust based on your current nail level. I'll will not list the chances here. Look at the repository, if you want to know.";
    }

    #endregion

    #region Protected Methods

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

    #endregion

    #region Private Methods

    private IEnumerator Haunt()
    {
        while (true)
        {
            yield return new WaitForSeconds(120f);
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
            fsm.GetState("Destroy").AddFirstAction(new Lambda(() =>
            {
                for (int i = 0; i < 3 * (index + 1); i++)
                    EventRegister.SendEvent("ADD BLUE HEALTH");
            }));
            fsm.SendEvent("START");
            float activeTime = 0f;
            while(activeTime < 90f && _ghost != null)
            {
                activeTime += Time.deltaTime;
                yield return null;
            }
            if (_ghost != null)
            {
                PlayMakerFSM.BroadcastEvent("DREAM AREA DISABLE");
                GameObject.Destroy(_ghost);
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
