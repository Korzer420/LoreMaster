using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using LoreMaster.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears;

public class MarissasAudiencePower : Power
{
    #region Members

    private GameObject[] _companions = new GameObject[3];

    private List<GameObject> _extraCompanions = new();

    private GameObject _revek;

    private int[] _minCompanionAmount = new int[3] { 1, 2, 4 };

    private int[] _maxCompanionAmounts = new int[3] { 3, 6, 12 };

    #endregion

    #region Constructors

    public MarissasAudiencePower() : base("Marissas Audience", Area.CityOfTears)
    {
        Hint = "While Marissa sings on stage, occasionally spawns a crowd that helps you. If you killed her however... you will be haunted by her biggest fan.";
        Description = "After 20 to 60 seconds spawn multiple companions (Weavers, Hatchlings, Grimmchilds) that persist in the current room or for 30 to 90 seconds. If Marissa is dead," +
            " spawns Revek each 45 to 180 seconds, that persist in the current room or 20 to 60 seconds.";
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the flag, that indicates if the player has killed Marissa.
    /// </summary>
    public bool IsMarissaDead => SceneData.instance.FindMyState(new PersistentBoolData()
    {
        sceneName = "Ruins_Bathhouse",
        id = "Ghost NPC",
        semiPersistent = false
    })?.activated ?? false;

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        GameObject charmEffects = GameObject.Find("Charm Effects");
        _companions[0] = charmEffects.LocateMyFSM("Spawn Grimmchild").GetState("Spawn").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
        _companions[1] = charmEffects.LocateMyFSM("Weaverling Control").GetState("Spawn").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
        _companions[2] = charmEffects.LocateMyFSM("Hatchling Spawn").GetState("Hatch").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;
    }

    protected override void Enable() =>  _runningCoroutine = LoreMaster.Instance.Handler.StartCoroutine(GatherAudience());

    protected override void Disable()
    {
        LoreMaster.Instance.Handler.StopCoroutine(_runningCoroutine);
        if (_extraCompanions.Any())
            foreach (GameObject companion in _extraCompanions)
                GameObject.Destroy(companion);
        _extraCompanions.Clear();
        if (_revek != null)
            GameObject.Destroy(_revek);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Spawns occasionally a crowd of companions or revek if marissa is dead.
    /// </summary>
    /// <returns></returns>
    private IEnumerator GatherAudience()
    {
        while (true)
        {
            if (IsMarissaDead)
            {
                yield return new WaitForSeconds(LoreMaster.Instance.Generator.Next(45, 181));
                _revek = GameObject.Instantiate(LoreMaster.Instance.PreloadedObjects["Ghost Battle Revek"], HeroController.instance.transform.position, Quaternion.identity);
                _revek.SetActive(true);
                PlayMakerFSM revekFSM = _revek.LocateMyFSM("Control");

                yield return null;

                revekFSM.SetState("Appear Pause");
                yield return new WaitForSeconds(LoreMaster.Instance.Generator.Next(20, 61));
                if (_revek != null)
                    GameObject.Destroy(_revek);
            }
            else
            {
                yield return new WaitForSeconds(LoreMaster.Instance.Generator.Next(20, 61));
                for (int companionIndex = 0; companionIndex < 3; companionIndex++)
                {
                    for (int companionCopy = 0; companionCopy < LoreMaster.Instance.Generator.Next(_minCompanionAmount[companionIndex], _maxCompanionAmounts[companionIndex]); companionCopy++)
                    {
                        GameObject newCompanion = GameObject.Instantiate(_companions[companionIndex]
                            , new Vector3(HeroController.instance.transform.GetPositionX()
                            , HeroController.instance.transform.GetPositionY()), Quaternion.identity);
                        GameObject.DontDestroyOnLoad(newCompanion);
                        _extraCompanions.Add(newCompanion);
                    }
                }

                yield return new WaitForSeconds(LoreMaster.Instance.Generator.Next(30, 91));

                foreach (GameObject companion in _extraCompanions)
                    GameObject.Destroy(companion);
                _extraCompanions.Clear();
            }
        }
    } 

    #endregion
}

