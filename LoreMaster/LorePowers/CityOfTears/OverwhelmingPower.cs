using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.CityOfTears
{
    public class OverwhelmingPower : Power
    {
        private bool _hasFullSoulMeter;
        private Dictionary<string, PlayMakerFSM> _spellFSMs = new();
        private List<GameObject> _spells = new();

        public OverwhelmingPower() : base("MAGE_COMP_03", Area.CityOfTears)
        {
            
        }

        protected override void Initialize()
        {
            GameObject spell = GameObject.Find("Knight/Spells");

            PlayMakerFSM spellControl = FsmHelper.GetFSM("Knight", "Spell Control");
            spellControl.GetState("Spell Choice").InsertAction(new Lambda(() =>
            {
                _hasFullSoulMeter = PlayerData.instance.MPCharge == 99;
            }), 0);
            spellControl.GetState("QC").InsertAction(new Lambda(() =>
            {
                _hasFullSoulMeter = PlayerData.instance.MPCharge == 99;
            }), 0);

            // Get all spell objects (all objects with the hero spell tag, should have the Set Damage fsm)
            foreach (Transform child in spell.transform)
            {
                if (child.tag.Equals("Hero Spell"))
                    _spells.Add(child.gameObject);
                foreach (Transform subChild in child)
                    if (subChild.tag.Equals("Hero Spell"))
                        _spells.Add(subChild.gameObject);

            }

            // Modify the damage fsm of each spell
            foreach (GameObject spellObject in _spells)
            {
                PlayMakerFSM spellDamageFSM = spellObject.LocateMyFSM("Set Damage");
                if (spellDamageFSM == null)
                    continue;
                spellDamageFSM.GetState("Finished").AddFirstAction(new Lambda(() =>
                {
                    if (_hasFullSoulMeter && Active)
                        spellObject.LocateMyFSM("damages_enemy").FsmVariables.GetFsmInt("damageDealt").Value *= 2;
                }));
            }
        }

        protected override void Enable()
        {
            if (!_initialized)
                Initialize();
            On.PlayMakerFSM.OnEnable += CheckFireball;
        }

        protected override void Disable()
        {
            On.PlayMakerFSM.OnEnable -= CheckFireball;
        }

        private void CheckFireball(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            if (!self.FsmName.Equals("Fireball Control"))
            {
                orig(self);
                return;
            }

            FsmState setDamageState = self.GetState("Init");
            if (setDamageState.Actions[0].Name.Equals("Overpower Fireball"))
                LoreMaster.Instance.Log("Already added action");
            else
                setDamageState.InsertAction(new Lambda(() =>
                {
                    if (_hasFullSoulMeter && Active)
                    { 
                        self.gameObject.LocateMyFSM("damages_enemy").FsmVariables.GetFsmInt("damageDealt").Value *= 2;
                        self.transform.localScale = new(self.transform.localScale.x + .8f, self.transform.localScale.y + .8f);
                    }
                })
                { 
                    IsAutoNamed = false,
                    Name = "Overpower Fireball"
                }, 0);
            orig(self);
        }
    }
}
