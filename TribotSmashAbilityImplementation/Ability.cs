using UnityEngine;
using AbilityInputManager;
using System.Collections;
using Tribot;

//This piece of code showcases the implementation of abilities for Tribot Smash!
//Since it is required to switch abilities on the fly ingame i designed the abilities to be fully modular
//The ability can be mapped to any button and swapped out at any time on the fly using this implementation

public abstract class Ability : MonoBehaviour
{
    //Variables
    [Header("Generic Ability Settings")]
    public AbilityInput.InputButtons MappedButton = AbilityInput.InputButtons.A;
    public string AbilityName = "Ability";
    public PlayerStateMachine PlayerState;
    public float CastMSMultiplier = 0.2f; //Is used by Player to determine how much slowdown in movement there is based on the ability
    public float Cooldown = 10f;
    
    public bool IsCasting = false; //IsCasting is used by player.cs to see if the ability is done casting


    public float PreCastTime = 0f;
    public float PostCastTime = 0f;
    protected virtual bool CanInterrupt { get { return true; } }

    protected Coroutine _cast = null;
    protected float Timestamp = 0f;

    //Properties
    //Abilities that have hold key functionality use this bool to determine if it needs to release
    public bool Release
    {
        protected get;
        set;
    }

    //Check if ability is off cooldown, can be overwritten by child if necesary
    public virtual bool IsReady
    {
        get { return Timestamp <= Time.time; }
    }

    //Determine current cooldown based on percentage, usefull for UI
    public float GetCooldown
    {
        get
        {
            if (IsReady)
                return 0;

            return (Timestamp - Time.time) / Cooldown;
        }
    }

    //Prepares the ability for casts
    public virtual bool Cast()
    {
        if (!IsReady || IsCasting)
            return false;
        IsCasting = true;
        Prepare();
        _cast = null;
        _cast = StartCoroutine(PreCast());
        return true;
    }

    //Interupts the ability if it is casting
    public virtual bool Cancel()
    {
        if (!IsCasting || !CanInterrupt)
            return false;
        if (_cast != null)
            StopCoroutine(_cast);
        InterruptCast();
        return true;
    }

    //Private methods
    //The actuall ability cast
    private IEnumerator PreCast()
    {
        yield return new WaitForSeconds(PreCastTime);
        Execute();
        _cast = null;
        _cast = StartCoroutine(PostCast());

    }

    //Waits for the postcasttime to run out so any ability animations can properly finish
    private IEnumerator PostCast()
    {
        yield return new WaitForSeconds(PostCastTime);
        _cast = null;
        IsCasting = false;
        Timestamp = Time.time + Cooldown;
        PlayerState.SwitchState(PlayerStateMachine.PlayerStates.Idle);
        Finish();
    }

    private void LateUpdate()
    {
        ExtendedLateUpdate();
        Release = false;
    }

    //Virtual and Abstract methods
    protected virtual void Prepare() { }
    protected virtual void Execute() { }
    protected virtual void Finish() { }
    protected virtual void InterruptCast() { }
    protected virtual void ExtendedLateUpdate() { }
}

//Now the implementation of an example ability called AbilityBomb
//In this ability the player jumps backwards while leaving a bomb behind
//The following code is now all that it takes to fully implement this ability
//Normally in a different file, but it's in here for convenience of showcasing
public class AbilityBomb : Ability
{
    [Header("Bomb Settings")]
    public Vector3 JumpForce = new Vector3(0f, 10f, 5f);
    public Vector3 BombPositionalOffset = new Vector3(0f, 1f, 0f);
    public GameObject BombPrefab;

    [Header("Audio Settings")]
    public CustomClip ClipJump;
    public CustomClip ClipLand;

    private Rigidbody _rb;
    private AudioSource _audio;
    private Bomb _bomb = null;

    //The default of caninterupt is true, we don't want that for this ability so override it to false
    protected override bool CanInterrupt
    {
        get { return false; }
    }

    // Use this for initialization
    void Start()
    {
        _rb = transform.parent.GetComponent<Rigidbody>();
        _audio = gameObject.AddComponent<AudioSource>();
    }

    //In the prepare the bomb gets placed on the player position and the player jump animation and sound gets played
    protected override void Prepare()
    {
        var obj = Instantiate(BombPrefab);
        obj.transform.position = transform.parent.position + BombPositionalOffset;
        _bomb = obj.GetComponent<Bomb>();

        ClipJump.Play(_audio);

        _rb.velocity += (transform.rotation * JumpForce);

        PlayerState.SwitchState(PlayerStateMachine.PlayerStates.Bomb);
    }

    //In the execute we trigger the bomb to explode
    protected override void Execute()
    {
        if (_bomb)
            _bomb.TriggerExplosion(transform.parent.gameObject);
    }
}