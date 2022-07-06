using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Internal;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster.LorePowers.Greenpath
{
    /// <summary>
    /// self power is just and adjustes version of skill upgrades, it could be possible, that both break active break something.
    /// </summary>
    public class RootedPower : Power
    {
        #region Constructors

        public RootedPower() : base("", Area.Greenpath)
        {
            
        }

        #endregion

        #region Properties

        public float ClimbSpeed { get; set; } = 7f;

        #endregion

        #region Public Methods

        protected override void Initialize()
        {
        }

        protected override void Enable()
        {
            // I just copypasted the code without exactly knowing why and how it works.^^

            // Replace WALLSLIDE_SPEED with 0 in the Fixed Update function, so the knight doesn't move down walls. 
            // Doing it like self allows the skill to be more easily toggled "live".
            IL.HeroController.FixedUpdate += SetWallslideSpeed;

            // When the knight is on the wall, we need to set gravity to 0 so it doesn't fall down 
            // (it would be very slow, though, because of the WALLSLIDE_SPEED being read as 0)
            IL.HeroController.LookForInput += SetGravityOnWallslide;
            IL.HeroController.RegainControl += SetGravityOnWallslide;
            IL.HeroController.FinishedDashing += SetGravityOnWallslide;
            // When the knight leaves the wall, return gravity
            IL.HeroController.CancelWallsliding += SetGravityOnWallslide;
            IL.HeroController.TakeDamage += SetGravityOnWallslide;

            // Stay wallsliding - "wallclinging" - when the player presses down
            IL.HeroController.LookForInput += StayOnWall;

            // If the game tries to reset the cState, make sure gravity is set to true - this probably won't actually ever matter
            On.HeroController.ResetState += HeroController_ResetState;

            // Allow the player to climb up and down
            On.HeroController.Update += MoveDown;

            // Allow the player to climb up and down, while on a conveyor
            IL.ConveyorMovementHero.LateUpdate += Conveyor_MoveDown;

            // Don't restore gravity if the player cancels a wall cdash
            On.HeroController.Start += SuperdashWallCancel;

            On.HeroController.CanFocus += HeroController_CanFocus;

            // We need to remove the Left Ground transition to prevent cancelling the focus.
            PlayMakerFSM playMakerFSM = FsmHelper.GetFSM("Knight", "Spell Control");
            playMakerFSM.GetState("Focus Start").RemoveTransitionsTo("Focus Cancel");
            playMakerFSM.GetState("Focus Start").AddTransition("BUTTON UP", "Focus Cancel");
            playMakerFSM.GetState("Focus").RemoveTransitionsTo("Grace Check");
            playMakerFSM.GetState("Focus").AddTransition("BUTTON UP", "Grace Check");
            //playMakerFSM.GetState("Focus").RemoveFirstActionOfType<Tk2dPlayAnimation>();
            //playMakerFSM.GetState("Focus Start").RemoveFirstActionOfType<Tk2dPlayAnimation>();
        }

        protected override void Disable()
        {
            IL.HeroController.FixedUpdate -= SetWallslideSpeed;
            IL.HeroController.LookForInput -= SetGravityOnWallslide;
            IL.HeroController.RegainControl -= SetGravityOnWallslide;
            IL.HeroController.FinishedDashing -= SetGravityOnWallslide;
            IL.HeroController.CancelWallsliding -= SetGravityOnWallslide;
            IL.HeroController.TakeDamage -= SetGravityOnWallslide;
            IL.HeroController.LookForInput -= StayOnWall;
            On.HeroController.ResetState -= HeroController_ResetState;
            On.HeroController.Update -= MoveDown;
            IL.ConveyorMovementHero.LateUpdate -= Conveyor_MoveDown;
            On.HeroController.Start -= SuperdashWallCancel;
            On.HeroController.CanFocus -= HeroController_CanFocus;
            PlayMakerFSM playMakerFSM = FsmHelper.GetFSM("Knight", "Spell Control");
            playMakerFSM.GetState("Focus Start").AddTransition("LEFT GROUND", "Focus Cancel");
            playMakerFSM.GetState("Focus").AddTransition("LEFT GROUND", "Grace Check");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adjusts the condition for focusing.
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <returns></returns>
        private bool HeroController_CanFocus(On.HeroController.orig_CanFocus orig, HeroController self)
        {
            bool result = orig(self);

            if (!result && !GameManager.instance.isPaused && self.cState.wallSliding)
                result = true;
            return result;
        }

        private void Conveyor_MoveDown(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            if (cursor.TryGotoNext
            (
                i => i.MatchLdfld<ConveyorMovementHero>("ySpeed"),
                i => i.MatchNewobj<Vector2>()
            ))
            {
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(ySpeed =>
                {
                    if (!Active)
                        return ySpeed;

                    if (InputHandler.Instance.inputActions.down.IsPressed && !HeroController.instance.CheckTouchingGround())
                        ySpeed -= ClimbSpeed;

                    return ySpeed;
                });
            }
        }

        private void SuperdashWallCancel(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            FsmState wallCancel = self.superDash.GetState("Charge Cancel Wall");
            if (wallCancel.Actions[2] is SendMessage _)
            {
                wallCancel.Actions[2] = new Lambda(() =>
                {
                    if (!Active)
                    {
                        HeroController.instance.AffectedByGravity(true);
                    }
                });
            }

            FsmState regainControl = self.superDash.GetState("Regain Control");
            if (regainControl.Actions[5] is SendMessage _)
                regainControl.Actions[5] = new Lambda(() =>
                {
                    if (!Active || !HeroController.instance.cState.wallSliding)
                        HeroController.instance.AffectedByGravity(true);
                });

        }

        private void StayOnWall(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            while (cursor.TryGotoNext
            (
                // There is only one check for down.WasPressed in LookForInput
                MoveType.After,
                i => i.MatchLdfld<InputHandler>(nameof(InputHandler.inputActions)),
                i => i.MatchLdfld<HeroActions>(nameof(HeroActions.down)),
                i => i.MatchCallvirt<InControl.OneAxisInputControl>("get_WasPressed")
            ))
            {
                cursor.EmitDelegate<Func<bool, bool>>(pressed => pressed && !Active);
            }
        }

        private void SetWallslideSpeed(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            while (cursor.TryGotoNext
            (
                MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<HeroController>(nameof(HeroController.WALLSLIDE_SPEED))
            ))
            {
                cursor.EmitDelegate<Func<float, float>>((s) => 0);
            }
        }

        private void MoveDown(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
            // This currently doesn't account for the GiftOnUnnPower. Needs further development.
            if (self.cState.wallSliding || (self.cState.focusing && PlayerData.instance.GetBool("equippedCharm_28") && self.cState.touchingWall))
            {
                Vector2 pos = HeroController.instance.transform.position;

                // Don't go down if touching ground because they'll go OOB
                if (InputHandler.Instance.inputActions.down.IsPressed && !self.CheckTouchingGround())
                    pos.y -= Time.deltaTime * ClimbSpeed;

                HeroController.instance.transform.position = pos;
            }
        }

        private void HeroController_ResetState(On.HeroController.orig_ResetState orig, HeroController self)
        {
            if (self.cState.wallSliding)
                HeroController.instance.AffectedByGravity(true);
            orig(self);
        }

        private void SetGravityOnWallslide(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);
            int matchedBool = -1;

            while (cursor.TryGotoNext
            (
                MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<HeroController>(nameof(HeroController.cState)),
                i => i.MatchLdcI4(out matchedBool),
                i => i.MatchStfld<HeroControllerStates>(nameof(HeroControllerStates.wallSliding))
            ))
            {
                switch (matchedBool)
                {
                    case 0:
                        cursor.EmitDelegate(() => HeroController.instance.AffectedByGravity(!HeroController.instance.cState.focusing));
                        break;
                    case 1:
                        cursor.EmitDelegate(() => HeroController.instance.AffectedByGravity(false));
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
