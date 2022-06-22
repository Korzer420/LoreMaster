using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modding;

namespace LoreMaster.LorePowers
{
    /// <summary>
    /// Class for the power to cast focus 30% faster.
    /// </summary>
    public class FokusPower : Power
    {
        private float _baseUnFocusSpeed;
        private float _baseFocusSpeed;

        public FokusPower() : base("TUT_TAB_01",Area.Dirtmouth)
        => Description = "<br>[Well Focused]<br>You focus a bit faster.";
        

        public override void Disable()
        {
            PlayMakerFSM playMakerFSM = HeroController.instance.spellControl;

            playMakerFSM.Fsm.GetFsmFloat("Time Per MP Drain UnCH").Value = _baseUnFocusSpeed;
            playMakerFSM.Fsm.GetFsmFloat("Time Per MP Drain CH").Value *= _baseFocusSpeed;
        }

        public override void Enable()
        {
            if (!Acquired)
                return;
            PlayMakerFSM playMakerFSM = HeroController.instance.spellControl;
            _baseUnFocusSpeed = playMakerFSM.Fsm.GetFsmFloat("Time Per MP Drain CH").Value;
            _baseFocusSpeed = playMakerFSM.Fsm.GetFsmFloat("Time Per MP Drain CH").Value;

            LoreMaster.Instance.Log("Old Speed: " + _baseFocusSpeed);
            playMakerFSM.Fsm.GetFsmFloat("Time Per MP Drain UnCH").Value *= 0.7f;
            playMakerFSM.Fsm.GetFsmFloat("Time Per MP Drain CH").Value *= 0.7f;

        }
    }
}
