using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.FungalWastes
{
    public class MantisStylePower : Power
    {
        private Transform[] _attackTransform = new Transform[4];

        public MantisStylePower() : base("MANTIS_PLAQUE_01", Area.FungalWastes)
        {
            Description = "<br>[Mantis Style]<br>Your Nail range is increased.";
            GameObject attackDirections = GameObject.Find("Knight/Attacks");
            _attackTransform[0] = attackDirections.transform.Find("Slash");
            _attackTransform[1] = attackDirections.transform.Find("AltSlash");
            _attackTransform[2] = attackDirections.transform.Find("UpSlash");
            _attackTransform[3] = attackDirections.transform.Find("DownSlash");
        }

        public override void Disable()
        {
            for (int i = 0; i < 4; i++)
            {
                if (i < 2)
                {
                    Vector3 currentScale = _attackTransform[i].GetComponent<NailSlash>().scale;
                    _attackTransform[i].GetComponent<NailSlash>().scale = new Vector3(currentScale.x - .35f, currentScale.y, currentScale.z);
                }
                else
                {
                    Vector3 currentScale = _attackTransform[i].GetComponent<NailSlash>().scale;
                    _attackTransform[i].GetComponent<NailSlash>().scale = new Vector3(currentScale.x, currentScale.y - .35f, currentScale.z);
                }
            }
        }

        public override void Enable()
        {
            for (int i = 0; i < 4; i++)
            {
                if(i < 2)
                {
                    Vector3 currentScale = _attackTransform[i].GetComponent<NailSlash>().scale;
                    _attackTransform[i].GetComponent<NailSlash>().scale = new Vector3(currentScale.x + .5f, currentScale.y, currentScale.z);
                }
                else
                {
                    Vector3 currentScale = _attackTransform[i].GetComponent<NailSlash>().scale;
                    _attackTransform[i].GetComponent<NailSlash>().scale = new Vector3(currentScale.x, currentScale.y + .5f, currentScale.z);
                }
            }
        }
    }
}
