using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KorzUtils.Helper;
using LoreMaster.Enums;
using LoreMaster.Manager;
using LoreMaster.UnityComponents;
using System.Collections;
using UnityEngine;

namespace LoreMaster.LorePowers.QueensGarden;

// Mit freundlicher Grüßen von Apo... ähm ich meine Red. Beim Insi Modus verwechsel ich gerne was sorry.^^
public class GrassBombardementPower : Power
{
    #region Members

    private GameObject _bombPrefab;

    private GameObject _activeBomb;

    private int _neededJumps;

    private int _jumped;

    #endregion

    #region Constructors

    public GrassBombardementPower() : base("Grass Bombardement", Area.QueensGarden)
    {
        CustomText = "Hey hey, just one more time ok?<page>No, we can't do this anymore, you have to stop.<page>Oh come on, it's so much fun. Don't you feel the satisfaction?<page> " +
            "I said no! We don't blow up anymore buildings, creatures, plants or ANYTHING else, ok?<page>Man, you're lame. Why can't you appreciate the art that I'm doing here?<page>" +
            "You call this ART!? You just let your bombs explode!<page>Hey, as long as it works...";
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the explosion prefab.
    /// </summary>
    public static GameObject Explosion =>
        LoreMaster.Instance.PreloadedObjects["Ceiling Dropper"].LocateMyFSM("Ceiling Dropper").GetState("Explode").GetFirstActionOfType<SpawnObjectFromGlobalPool>().gameObject.Value;

    /// <summary>
    /// Gets or sets the needed jumps for a bomb drop (twisted only)
    /// </summary>
    public int Jumps { get; set; }

    #endregion

    #region Event handler

    private void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);
        ModifyHero();
    }

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        try
        {
            if (string.Equals(self.FsmName, "quake_floor"))
            {
                self.AddState(new FsmState(self.Fsm)
                {
                    Name = "Explosion Quake",
                    Actions = new FsmStateAction[]
                    {
                    self.GetState("Transient").Actions[0],
                    // Take this as an example why working with fsm can be absolute bs.
                    new Trigger2dEvent()
                    {
                        collideTag = new() { Value = "Wall Breaker", RawValue = "Wall Breaker"},
                        sendEvent = self.GetState("Transient").GetFirstActionOfType<Trigger2dEvent>().sendEvent,
                        storeCollider = new("None"),
                        collideLayer = new()
                    }
                    }
                });

                // To prevent the "not-destruction" of dive or bomb if the other one is active, we add a additional state that handles both.
                self.AddState(new FsmState(self.Fsm)
                {
                    Name = "Multiple Breaker",
                    Actions = new FsmStateAction[]
                    {
                    self.GetState("Transient").Actions[1],
                    new Trigger2dEvent()
                    {
                        collideTag = new() { Value = "Wall Breaker", RawValue = "Wall Breaker"},
                        sendEvent = self.GetState("Transient").GetFirstActionOfType<Trigger2dEvent>().sendEvent,
                        storeCollider = new("None"),
                        collideLayer = new()
                    }
                    }
                });

                FsmState explosionState = self.GetState("Explosion Quake");
                explosionState.AddTransition("VANISHED", "Solid");
                explosionState.AddTransition("DESTROY", "PD Bool?");
                explosionState.AddTransition("QUAKE FALL START", "Multiple Breaker");

                FsmState multipleBreak = self.GetState("Multiple Breaker");
                multipleBreak.AddTransition("DESTROY", "PD Bool?");
                multipleBreak.AddTransition("QUAKE FALL END", "Explosion Quake");
                multipleBreak.AddTransition("VANISHED", "Transient");

                self.GetState("Solid").AddTransition("BOMBED", explosionState);
                self.GetState("Solid").AddTransition("POWERBOMBED", "PD Bool?");
                self.GetState("Transient").AddTransition("BOMBED", multipleBreak);
                self.GetState("Transient").AddTransition("POWERBOMBED", "PD Bool?");
            }
            else if (string.Equals(self.FsmName, "Detect Quake"))
            {
                self.GetState("Detect").AddTransition("POWERBOMBED", "Quake Hit");
                self.GetState("Check Quake").AddTransition("POWERBOMBED", "Quake Hit");
            }
            else if (string.Equals(self.FsmName, "break_floor"))
                self.GetState("Idle").AddTransition("POWERBOMBED", "PlayerData");
            else if (string.Equals(self.FsmName, "breakable_wall_v2"))
                self.GetState("Idle").AddTransition("POWERBOMBED", "PD Bool?");
        }
        catch (System.Exception)
        {
        }
        orig(self);
    }

    private void ListenForUp_OnEnter(On.HutongGames.PlayMaker.Actions.ListenForUp.orig_OnEnter orig, ListenForUp self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "QC") && string.Equals(self.isPressed.Name, "SCREAM") && SettingManager.Instance.BombQuickCast)
        {
            if (InputHandler.Instance.inputActions.left.IsPressed)
                self.Fsm.FsmComponent.SendEvent("BOMB");
            else if (InputHandler.Instance.inputActions.right.IsPressed)
                self.Fsm.FsmComponent.SendEvent("POWERBOMB");
        }
        orig(self);
    }

    private void BoolTest_OnEnter(On.HutongGames.PlayMaker.Actions.BoolTest.orig_OnEnter orig, BoolTest self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Spell Choice") && string.Equals(self.isTrue.Name, "SCREAM"))
        {
            if (InputHandler.Instance.inputActions.left.IsPressed)
                self.Fsm.FsmComponent.SendEvent("BOMB");
            else if (InputHandler.Instance.inputActions.right.IsPressed)
                self.Fsm.FsmComponent.SendEvent("POWERBOMB");
        }
        orig(self);
    }

    private void IntCompare_OnEnter(On.HutongGames.PlayMaker.Actions.IntCompare.orig_OnEnter orig, IntCompare self)
    {
        if ((self.IsCorrectContext("Spell Control", "Knight", "Can Cast? QC") && SettingManager.Instance.BombQuickCast)
            || self.IsCorrectContext("Spell Control", "Knight", "Can Cast?"))
        {
            if (InputHandler.Instance.inputActions.right.IsPressed && PlayerData.instance.GetInt("healthBlue") > 0)
                self.Fsm.FsmComponent.SendEvent("POWERBOMB");
            else if (InputHandler.Instance.inputActions.left.IsPressed && self.integer1.Value >= self.integer2.Value)
                self.Fsm.FsmComponent.SendEvent("BOMB");
        }
        orig(self);
    }

    private void HeroController_CancelJump(On.HeroController.orig_CancelJump orig, HeroController self)
    {
        orig(self);
        _jumped++;
        if (_jumped >= _neededJumps)
        {
            _jumped = 0;
            StartRoutine(() => PoopBomb());
        }
    }

    #endregion

    #region Control

    /// <inheritdoc/>
    protected override void Initialize()
    {
        On.HeroController.Start += HeroController_Start;
        ModifyHero();
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter += IntCompare_OnEnter;
        On.HutongGames.PlayMaker.Actions.BoolTest.OnEnter += BoolTest_OnEnter;
        On.HutongGames.PlayMaker.Actions.ListenForUp.OnEnter += ListenForUp_OnEnter;
    }

    /// <inheritdoc/>
    protected override void Disable()
    { 
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
        On.HutongGames.PlayMaker.Actions.IntCompare.OnEnter -= IntCompare_OnEnter;
        On.HutongGames.PlayMaker.Actions.BoolTest.OnEnter -= BoolTest_OnEnter;
        On.HutongGames.PlayMaker.Actions.ListenForUp.OnEnter -= ListenForUp_OnEnter;
    }

    /// <inheritdoc/>
    protected override void TwistEnable()
    {
        On.HeroController.CancelJump += HeroController_CancelJump;
        _jumped = 0;
        _neededJumps = 12;
    }

    /// <inheritdoc/>
    protected override void TwistDisable()
    {
        On.HeroController.CancelJump -= HeroController_CancelJump;
        _jumped = 0;
    }

    /// <inheritdoc/>
    protected override void Terminate()
    => On.HeroController.Start -= HeroController_Start;

    #endregion

    #region Methods

    /// <summary>
    /// Modify the spell fsm.
    /// </summary>
    private void ModifyHero()
    {
        PlayMakerFSM fsm = HeroController.instance.spellControl;
        FsmState normalBomb = new(fsm.Fsm)
        {
            Name = "Normal bomb",
            Actions = new FsmStateAction[]
            {
                new Lambda(()=>
                {
                    HeroController.instance.TakeMP(fsm.FsmVariables.FindFsmInt("MP Cost").Value);
                    _activeBomb = GameObject.Instantiate(_bombPrefab);
                    _activeBomb.transform.localPosition = HeroController.instance.transform.localPosition - new Vector3(2f,0f,0f);
                    _activeBomb.transform.localScale = new(2f,2f,1f);
                    _activeBomb.name = "Grass bomb";
                    _activeBomb.SetActive(true);
                })
            }
        };
        normalBomb.AddTransition("FINISHED", "Spell End");

        FsmState powerBomb = new(fsm.Fsm)
        {
            Name = "Power bomb",
            Actions = new FsmStateAction[]
            {
                new Lambda(() =>
                {
                    FakeDamage = true;
                    HeroController.instance.TakeHealth(1);
                    _activeBomb = GameObject.Instantiate(_bombPrefab);
                    _activeBomb.transform.localPosition = HeroController.instance.transform.localPosition + new Vector3(2f,0f,0f);
                    _activeBomb.transform.localScale = new(2f,2f,1f);
                    _activeBomb.name = "Power bomb";
                    _activeBomb.SetActive(true);
                })
            }
        };
        powerBomb.AddTransition("FINISHED", "Spell End");

        fsm.GetState("Spell Choice").AddTransition("BOMB", normalBomb);
        fsm.GetState("Spell Choice").AddTransition("POWERBOMB", powerBomb);
        fsm.GetState("QC").AddTransition("BOMB", normalBomb);
        fsm.GetState("QC").AddTransition("POWERBOMB", powerBomb);

        if (_bombPrefab == null)
        {
            _bombPrefab = new("Bomb");
            _bombPrefab.SetActive(false);
            _bombPrefab.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<LoreMaster>("Base.LifebloodBomb");
            Rigidbody2D rigidbody = _bombPrefab.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 1f;
            rigidbody.mass = 200f;
            _bombPrefab.AddComponent<BoxCollider2D>();
            _bombPrefab.AddComponent<Bomb>();
            GameObject.DontDestroyOnLoad(_bombPrefab);
        }
    }

    private IEnumerator PoopBomb()
    {
        yield return new WaitForSeconds(0.2f);
        _activeBomb = GameObject.Instantiate(_bombPrefab);
        _activeBomb.transform.localPosition = HeroController.instance.transform.localPosition + (HeroController.instance.cState.facingRight ? new Vector3(-2f, 0f, 0f) : new(2f, 0f, 0f));
        _activeBomb.transform.localScale = new(2f, 2f, 1f);
        _activeBomb.name = "Grass bomb";
        _activeBomb.SetActive(true);
        _jumped = 0;
        _neededJumps = LoreMaster.Instance.Generator.Next(25, 100);
    }

    #endregion
}