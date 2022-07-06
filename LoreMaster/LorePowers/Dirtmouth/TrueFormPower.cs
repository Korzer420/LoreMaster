using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vasi;

namespace LoreMaster.LorePowers
{
    public class TrueFormPower : Power
    {
        private int _shadeState = 0;

        private List<Transform> _attackTransform = new List<Transform>();

        public TrueFormPower() : base("TUT_TAB_03", Area.Dirtmouth)
        {
            Hint = "<br>[True Form]<br>While the true form is revealed, its vessels nail gets more powerful. Especially near your true self.";
            
        }

        protected override void Initialize()
        {
            GameObject attackDirections = GameObject.Find("Knight/Attacks");
            _attackTransform.Add(attackDirections.transform.Find("Slash"));
            _attackTransform.Add(attackDirections.transform.Find("AltSlash"));
            _attackTransform.Add(attackDirections.transform.Find("UpSlash"));
            _attackTransform.Add(attackDirections.transform.Find("DownSlash"));
        }

        protected override void Enable()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            ModHooks.GetPlayerIntHook += NailDamageUpdate;
        }

        private int NailDamageUpdate(string name, int orig)
        {
            if (name.Equals("nailDamage"))
            {
                int dmgIncrease = 0;
                if (_shadeState != 0)
                    dmgIncrease = Convert.ToInt32(orig * (_shadeState == 1 ? .3f : .6f));
                orig += dmgIncrease;
            }

            
            return orig;
        }

        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            float multiplier = .25f;
            if (!PlayerData.instance.shadeScene.Equals("None"))
            {
                // If we are going in the shade room, we increase the range buff
                if (PlayerData.instance.shadeScene.Equals(arg1.name))
                {
                    // If we have been already in the shade room (like dying in a room with the save bench), we ignore the multiplier.
                    if (_shadeState == 2)
                        return;
                    ModifyShade();
                    multiplier *= _shadeState == 1 ? 1 : 2;
                    _shadeState = 2;
                }
                else if (_shadeState != 1)
                {
                    // Depending where we are getting from, we lower or increase the range
                   multiplier *= _shadeState == 2 ? -1 : 1;
                    _shadeState = 1;
                }
                else
                    return;
                ModifyNailLength(multiplier);
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                return;
            }

            // If we are entering jiji's room we need to account for spawning the shade. Otherwise it causes the bonus not to disappear.

            if(PlayerData.instance.shadeScene.Equals("Room_Ouiji"))
            {
                PlayMakerFSM fsm = FsmHelper.GetFSM("Jiji NPC", "Conversation Control");

                fsm.GetState("Spawn").AddMethod(() =>
                {
                    ModifyShade();
                    _shadeState = 2;
                    PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                    ModifyNailLength(multiplier);
                });
            }
        }

        protected override void Disable()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
            ModHooks.GetPlayerIntHook -= NailDamageUpdate;
        }

        /// <summary>
        /// Attaches an action on the shade, to remove the buffs from the player in case they kill the shade.
        /// </summary>
        private void ModifyShade()
        {
            GameObject shade = GameObject.Find("Hollow Shade(Clone)");

            if (shade == null)
                return;

            PlayMakerFSM fsm = GameObject.Find("Hollow Shade(Clone)").LocateMyFSM("Shade Control");

            // Add the action on the killed state
            fsm.GetState("Killed").InsertMethod(5, () =>
            {
                ModifyNailLength(-.5f);
                _shadeState = 0;
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            });
        }

        /// <summary>
        /// Change the nail hit ranges.
        /// </summary>
        /// <param name="multiplier"></param>
        private void ModifyNailLength(float multiplier)
        {
            for (int index = 0; index < 4; index++)
            {
                Vector3 currentScale = _attackTransform[index].GetComponent<NailSlash>().scale;
                _attackTransform[index].GetComponent<NailSlash>().scale = new Vector3(currentScale.x + multiplier, currentScale.y + multiplier, currentScale.z + multiplier);
            }
        }
    }
}
