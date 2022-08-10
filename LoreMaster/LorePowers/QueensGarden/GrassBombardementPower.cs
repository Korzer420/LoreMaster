using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreMaster.LorePowers.QueensGarden;

public class GrassBombardementPower : Power
{
    #region Constructors

    public GrassBombardementPower() : base("Grass Bombardement", Area.QueensGarden)
    {
        Hint = "Forms the grass in your bag to a \"special delivery\" which explodes after 3 seconds, dealing huge damage. The disruption may break loose walls and the floor. " +
            "Requires soul to construct the bomb. Press (Quick)cast and left to drop the bomb. They say, that you also can channel the blue plague, to create an even stronger bomb. " +
            "Press (Quick)cast and right to consume the blue blood and spawn the powerful nuke.";
        Description = "Pressing left while casting spawns a bomb which explodes after 3 seconds, that deals 60 damage and breaks damaged walls/ground. Pressing right, will consum a " +
            "lifeblood mask to spawn a more powerful bomb, with 1.5x times the radius and twice the damage.";
        CustomText = "Hey hey, just one more time ok?<page>No, we can't do this anymore, you have to stop.<page>Oh come on, it's so much fun. Don't you feel the satisfaction?<page> " +
            "I said no! We don't blow up anymore buildings, creatures, plants or ANYTHING else, ok?<page>Man, you're lame. Why can't you appreciate the art that I do here?<page>" +
            "You call this ART!? You just let your bombs explode!<page>Hey, as long as it works...";
    }

    #endregion

    #region Protected Methods

    protected override void Initialize()
    {
        PlayMakerFSM fsm = HeroController.instance.spellControl;
        FsmState normalBomb = new(fsm.Fsm) 
        { 
            Name = "Normal Bomb"
        };
        fsm.GetState("QC").AddTransition("BOMB", normalBomb);
        fsm.GetState("QC").ReplaceAction(new Lambda(() => 
        {
            if (Active && InputHandler.Instance.inputActions.left.IsPressed)
                fsm.SendEvent("BOMB");
            else if (InputHandler.Instance.inputActions.down.IsPressed)
                fsm.SendEvent("QUAKE");
            else
                fsm.SendEvent("FIREBALL");
        }) 
        { 
            Name = "Check for Left"
        },3);
    }

    #endregion
}
