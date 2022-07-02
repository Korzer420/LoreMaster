using HutongGames.PlayMaker.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vasi;

namespace LoreMaster.LorePowers.CityOfTears;

internal class MarissasAudiencePower : Power
{
    private GameObject[] _companions = new GameObject[3];

    private List<GameObject> _extraCompanions = new();

    private GameObject _revek;

    private int[] _minAmounts = new int[3] { 1, 2, 4 };

    private int[] _maxAmounts = new int[3] { 3, 6, 12 };

    public MarissasAudiencePower() : base("Marissa Poster", Area.CityOfTears)
    {
        GameObject charmEffects = GameObject.Find("Charm Effects");

        _companions[0] = charmEffects.LocateMyFSM("Spawn Grimmchild").GetAction<SpawnObjectFromGlobalPool>("Spawn", 2).gameObject.Value;
        _companions[1] = charmEffects.LocateMyFSM("Weaverling Control").GetAction<SpawnObjectFromGlobalPool>("Spawn", 0).gameObject.Value;
        _companions[2] = charmEffects.LocateMyFSM("Hatchling Spawn").GetAction<SpawnObjectFromGlobalPool>("Hatch", 2).gameObject.Value;
    }

    public bool IsMarissaDead => SceneData.instance.FindMyState(new PersistentBoolData()
    {
        sceneName = "Ruins_Bathhouse",
        id = "Ghost NPC",
        semiPersistent = false
    })?.activated ?? false;

    public override void Enable()
    {
        HeroController.instance.StartCoroutine(GatherAudience());
    }

    IEnumerator GatherAudience()
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
                    for (int companionCopy = 0; companionCopy < LoreMaster.Instance.Generator.Next(_minAmounts[companionIndex], _maxAmounts[companionIndex]); companionCopy++)
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

    public override void Disable()
    {
        HeroController.instance.StopCoroutine(GatherAudience());
        if (_extraCompanions.Any())
            foreach (GameObject companion in _extraCompanions)
                GameObject.Destroy(companion);
        _extraCompanions.Clear();
        if (_revek != null)
            GameObject.Destroy(_revek);
    }
}

