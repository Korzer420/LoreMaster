using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using LoreMaster.UnityComponents;
using LoreMaster.Enums;
using LoreMaster.Extensions;
using LoreMaster.Helper;
using UnityEngine;

namespace LoreMaster.LorePowers.QueensGarden;

// Mit freundlicher Grüßen von Apo... ähm ich meine Red. Beim Insi Modus verwechsel ich gerne was sorry.^^
public class GrassBombardementPower : Power
{
    #region Members

    private GameObject _bombPrefab;

    private GameObject _activeBomb;

    #endregion

    #region Constructors

    public GrassBombardementPower() : base("Grass Bombardement", Area.QueensGarden)
    {
        Hint = "Forms the grass in Hallownest with the power of the soul to a \"special delivery\" which explodes shortly after creation, dealing huge damage. The disruption may break loose walls and floors. " +
            "Requires soul to construct the bomb. Press (Quick)cast and left to drop the bomb. They say, that you also can channel the blue plague, to create an even stronger bomb which can even break the heaviest stones and emits a destruction shockwave through the room. " +
            "Press (Quick)cast and right to consume the blue blood and spawn the powerful nuke. You can disable quick cast for the bombs in the mod menu.";
        Description = "Pressing left while casting. spawns a bomb which explodes after 3 seconds, that deals 40 damage (60 with shaman stone) and breaks damaged walls/ground. Pressing right, will consum a " +
            "lifeblood mask to spawn a more powerful bomb, with a bigger radius, 50 % more damage and the ability to break ALL damaged floors/walls in the room, even heavy floors and one way walls. You can disable quick cast for the bombs in the mod menu.";
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

    #endregion

    #region Protected Methods

    protected override void Initialize()
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

        // The power bomb doesn't require soul, therefore we modify the can cast condition.
        fsm.GetState("Can Cast? QC").ReplaceAction(new Lambda(() =>
        {
            if (fsm.FsmVariables.FindFsmInt("MP").Value < fsm.FsmVariables.FindFsmInt("MP Cost").Value
            && !(LoreMaster.Instance.BombQuickCast && Active && InputHandler.Instance.inputActions.right.IsPressed 
            && PlayerData.instance.GetInt(nameof(PlayerData.instance.healthBlue)) > 0 && _activeBomb == null))
                fsm.SendEvent("CANCEL");
        })
        { Name = "Can cast?"}, 2);

        // The power bomb doesn't require soul, therefore we modify the can cast condition.
        fsm.GetState("Can Cast?").ReplaceAction(new Lambda(() =>
        {
            if (fsm.FsmVariables.FindFsmInt("MP").Value < fsm.FsmVariables.FindFsmInt("MP Cost").Value
            && !(Active && InputHandler.Instance.inputActions.right.IsPressed
            && PlayerData.instance.GetInt(nameof(PlayerData.instance.healthBlue)) > 0 && _activeBomb == null))
                fsm.SendEvent("CANCEL");
        })
        { Name = "Can cast?" }, 2);

        fsm.GetState("Spell Choice").AddTransition("BOMB", normalBomb);
        fsm.GetState("Spell Choice").AddTransition("POWERBOMB", powerBomb);
        fsm.GetState("Spell Choice").ReplaceAction(new Lambda(() =>
        {
            if (Active && InputHandler.Instance.inputActions.left.IsPressed && _activeBomb == null)
                fsm.SendEvent("BOMB");
            else if (Active && InputHandler.Instance.inputActions.right.IsPressed 
            && PlayerData.instance.GetInt(nameof(PlayerData.instance.healthBlue)) > 0 && _activeBomb == null)
                fsm.SendEvent("POWERBOMB");
            else if (InputHandler.Instance.inputActions.down.IsPressed)
                fsm.SendEvent("QUAKE");
            else
                fsm.SendEvent("FIREBALL");
        })
        {
            Name = "Check for Left"
        }, 1);

        fsm.GetState("QC").AddTransition("BOMB", normalBomb);
        fsm.GetState("QC").AddTransition("POWERBOMB", powerBomb);
        fsm.GetState("QC").ReplaceAction(new Lambda(() =>
        {
            if (Active && LoreMaster.Instance.BombQuickCast && InputHandler.Instance.inputActions.left.IsPressed && _activeBomb == null)
                fsm.SendEvent("BOMB");
            else if (Active && LoreMaster.Instance.BombQuickCast && InputHandler.Instance.inputActions.right.IsPressed
            && PlayerData.instance.GetInt(nameof(PlayerData.instance.healthBlue)) > 0 && _activeBomb == null)
                fsm.SendEvent("POWERBOMB");
            else if (InputHandler.Instance.inputActions.down.IsPressed)
                fsm.SendEvent("QUAKE");
            else
                fsm.SendEvent("FIREBALL");
        })
        {
            Name = "Check for Left"
        }, 3);

        if (_bombPrefab == null)
        {
            _bombPrefab = new("Bomb");
            _bombPrefab.SetActive(false);
            GameObject scuttle = LoreMaster.Instance.PreloadedObjects["Lifeblood Scuttler"];
            _bombPrefab.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite("LifebloodBomb");
            Rigidbody2D rigidbody = _bombPrefab.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 1f;
            rigidbody.mass = 200f;
            _bombPrefab.AddComponent<BoxCollider2D>();
            _bombPrefab.AddComponent<Bomb>();
            GameObject.DontDestroyOnLoad(_bombPrefab);
        }
    }

    protected override void Enable()
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
    }

    protected override void Disable()
    {
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
    }

    private void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (string.Equals(self.FsmName, "quake_floor"))
        {
            self.AddState(new(self.Fsm)
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
            self.AddState(new(self.Fsm)
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
        else if(string.Equals(self.FsmName, "breakable_wall_v2"))
            self.GetState("Idle").AddTransition("POWERBOMBED", "PD Bool?");
        orig(self);
    }

    #endregion
}