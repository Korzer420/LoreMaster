using ItemChanger;
using LoreMaster.LorePowers.CityOfTears;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster
{
    internal record Paypal : Cost
    {
        public bool ToTemple { get; set; }

        public override bool CanPay()
        => TouristPower.Inspected && PlayerData.instance.geo > 9;

        public override string GetCostText()
        {
            if (ToTemple)
            {
                if (!TouristPower.Inspected)
                    return "This firefly seems to be sleeping. Whatever it has to offer, doesn't seem available at the moment. You wouldn't dare to wake it up, right?";
                else
                    return "Want to inspect the glorious Temple of the Black egg? Traveling costs are 10 geo (drinks not included).";
            }

            if (!TouristPower.Inspected)
                return "Hey, my friend. I hope you enjoy the tour of this magnificent temple. I'd like to offer a trip back to the city, but unfortunately my stag friend is a bit" +
                    " exhausted. I don't want to push him too much. This would make me sad. You don't want me to be sad, right? :(";
            else
                return "Hey, my friend. I hope you enjoy the tour of this magnificent temple. If you want, I can take you back to the city, for the best view on the statue." +
                    " But my sister wants me to take geo for my service. I hope this isn't a problem for you. Sorry :(";
        }

        public override bool HasPayEffects()
        => false;

        public override void OnPay()
        {
            HeroController.instance.TakeGeo(10);
            HeroController.instance.StartCoroutine(Wait());
        }

        IEnumerator Wait()
        {
            yield return null;
            Paid = false;
        }
    }
}
