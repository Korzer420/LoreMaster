using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes
{
    public class OneOfUsPower : Power
    {
        private GameObject _cloud;
        private Coroutine _cloudRoutine;

        public OneOfUsPower(): base("FUNG_TAB_04", Area.FungalWastes)
        {
            Hint = "<br>[One of Us]<br>Occasionally you emit a spore cloud. (Hold the super dash button to prevent the cloud.)";
            
        }

        protected override void Initialize()
        {
            _cloud = GameObject.Find("_GameManager");
            _cloud = _cloud.transform.Find("GlobalPool/Knight Spore Cloud(Clone)").gameObject;
        }

        protected override void Enable()
        {
            _cloudRoutine = HeroController.instance.StartCoroutine(EmitCloud());
        }

        IEnumerator EmitCloud()
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

        protected override void Disable()
        {
            HeroController.instance.StopCoroutine(_cloudRoutine);
        }
    }
}
