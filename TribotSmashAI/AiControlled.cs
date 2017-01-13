using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AbilityInputManager;
using Tribot;
using Tribot.Logger;
using UnityEngine;
using Tribot.Container;
using ExtensionMethods;

/*
This script is the implementation of AI for tribot smash
The goal of AI is to make the game feel less empty when played with less then 4 player coop
The ai is fully tailored with tribot smash in mind. It is cheap, fast and makes the AI behaviour feel alive
The AI can be seen in the following video the first 40 seconds
https://www.youtube.com/watch?v=DBWMmZ5GOdQ


    -Pathfinding summary-
The pathfinding of this ai is simple in essence but the best solution for the game tribot smash
it checks if the ai can move forwards if not it will look around for an opening and determine which opening is the optimal way to get unstuck
This works great for tribot smash since the arena's aren't complex so  the ai can easily find a way out

It also creates scenario's where the AI walks against another player which makes him execute an attack. 
But because he also can't move forward he will dodge towards the optimal rotation directly. This makes the AI feel much more like a real player

Another great advantage of this pathfinding method is that it is super cheap, now other parts of the game can be much more visually detailed and complex since the AI doesn't use up any cpu

The disadvantage of the ai is that it won't be able to find a way out of small mazes that consist of small corridors, but this scenario does not exist in tribot smash

    -Attacking summary-
Attack is based on relative distances between the AI and other players and line of sight.
Abilities all basicly ranked in priority based on which case is currently presented and which abilities are available
This can be seen in action in the determine attack ability
*/
public class AiControlled : Player
{
    enum AiState
    {
        Regular,
        Targeting,
    }

    AiState _state = AiState.Regular; // These states are only used to switch targeting mode for ranged abilities.

    public Range RefreshRate = new Range() { Min = .25f, Max = .75f }; //The AI will make decisions in intervals of number within this range
    
    public float CloseRangeDistance = 1.5f; // This is the distance from the player center to the player front used to determine if the ai is stuck
    public float MoveSpeed = 6f;
    public float RotationSpeed = 500f;
    public Vector3 BoxSize = new Vector3(.5f, .5f, 1f); //Used to raycast(boxcast) in front of the ai.
    public Vector3 HeightOffset = new Vector3(0f, 1f, 0f); // this is the center of the character

    //Player always rotates towards this euler angle
    private Vector3 _targetEulerAngle = Vector3.zero;
    private GameObject _targetObject = null;

    //All the lists of objects the AI needs to know
    private List<GameObject> Players;
    private List<GameObject> Pickups;
    private List<GameObject> ControlPoints;
    private List<GameObject> Projectiles;
    private List<GameObject> Meteors;

    private bool _refreshed = true;
    private float _timestamp = 0f;

    private float _rangedAttackTimestamp = 0f;
    private bool OnTarget = false;

    float TargetRefreshRate
    {
        get { return Random.Range(RefreshRate.Min, RefreshRate.Max); } //Returns random refresh rate
    }

    Quaternion _angle
    {
        get
        {
            /*
            Depending on the targeting state this will return an target angle which the AI rotates towards
            Normally the ai rotates towards a target that is determined every refersh
            But if the AI is targeting an object the AI will continuesly rotate towards that gameobject realtime
            */
            var angle = Vector3.zero;
            switch (_state)
            {
                case AiState.Regular:

                    angle = _targetEulerAngle;
                    break;
                case AiState.Targeting:
                    if (!_targetObject)
                        angle = _targetEulerAngle;
                    else
                        angle = transform.LookRotation(
                            _targetObject.transform.position.x,
                            transform.position.y,
                            _targetObject.transform.position.z).eulerAngles;
                    break;
                default:
                    angle = _targetEulerAngle;
                    break;
            }

            return Quaternion.Euler(0f, angle.y, 0f);
        }
    }

    Vector3 _newPosition
    {
        get
        {
            /*
            Returns a new position for the AI based on rotation and speed
            */
            var speed = MoveSpeed;
            switch (_state)
            {
                case AiState.Regular:
                    //DONT DO SHIT
                    break;
                case AiState.Targeting:
                    speed = MoveSpeed * .3f;
                    break;
            }
            _animState.Speed = speed / MoveSpeed;

            return transform.position + (transform.forward * speed * Time.deltaTime);
        }
    }

    protected override void ExtendedStart()
    {
        //Obtain all objects from a singleton which the AI has to know
        Players = TargetContainer.Instance.Players;
        Pickups = TargetContainer.Instance.Pickups;
        ControlPoints = TargetContainer.Instance.ControlPoints;
        Projectiles = TargetContainer.Instance.Projectiles;
        Meteors = TargetContainer.Instance.Meteors;
    }

    protected override void ExtendedFixedUpdate()
    {
        //Update refresh boolean
        if (Time.time < _timestamp)
        {
            _refreshed = false;
        }
        else
        {
            _refreshed = true;
            _timestamp = Time.time + TargetRefreshRate;
        }

        //OnTarget is used to keep the AI standing still on an objective, 
        //an example is the control point gamemodes where the palyer that stands on a certain position for the longest time will win
        OnTarget = false;
        var obj = GetClosestTargetFromList(ControlPoints);
        if (obj && GetDistance(obj) < 1.5f)
            OnTarget = true;

        //Check if the ai can do something trhought the playerstatemachine
        if (_animState.CanInteract)
        {
            DetermineAttack();
            if (_refreshed)
            {
                if (!DetermineIfStuck())
                {
                    DetermineRotation();
                }
            }
        }

        //Check if the ai can move through the playerstatemachine
        if (_animState.CanMove)
        {
            //Rotate and move
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _angle, Time.deltaTime * RotationSpeed);
            //Only move if the ai is not standing on an objective
            if (!OnTarget)
                _rb.MovePosition(_newPosition);
        }
    }

    void DetermineRotation()
    {
        Quaternion rotation;

        var meteor = GetClosestTargetFromList(Meteors);
        if (meteor && GetDistance(meteor) < 2.2f)
        {
            Log("Run away from meteor");
            var dir = (transform.position - meteor.transform.PositionXZ(transform.position)).normalized;
            rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            var obj = GetOptimalTarget();
            if (obj == null)
                return;
            Log("new target: " + obj.name);
            rotation = transform.LookRotation(obj.transform.position);
        }

        
        var euler = rotation.eulerAngles;
        _targetEulerAngle = euler;
    }


    /*
    Determineifstuck is basicly the pathfinding for the AI
    */
    bool DetermineIfStuck()
    {
        var obj = CheckFrontSide(CloseRangeDistance);
        if (obj != null)
        {
            Log("Stuck, looking for a way out");
            _targetEulerAngle = GetOptimalRotation();
            return true;
        }

        return false;
    }

    void DetermineAttack()
    {
        Ability dash = null;
        Ability shield = null;
        Ability punch = null;
        Ability grab = null;
        Ability shockwave = null;
        Ability shockblast = null;
        Ability wall = null;
        Ability rangedgrab = null;
        Ability bomb = null;

        //Check which abilities are available for the AI
        foreach (var ability in Abilities)
        {
            if (ability.Value.IsReady)
            {
                if (ability.Value.GetType() == typeof (AbilityDash))
                {
                    dash = ability.Value;
                }
                else if (ability.Value.GetType() == typeof(AbilityShield))
                {
                    shield = ability.Value;
                }
                else if (ability.Value.GetType() == typeof(AbilityPunch))
                {
                    punch = ability.Value;
                }
                else if (ability.Value.GetType() == typeof(AbilityGrab))
                {
                    grab = ability.Value;
                }
                else if (ability.Value.GetType() == typeof(AbilityShockwave))
                {
                    shockwave = ability.Value;
                }
                else if (ability.Value.GetType() == typeof(AbilityChargedShot))
                {
                    shockblast = ability.Value;
                }
                else if (ability.Value.GetType() == typeof(AbilityWall))
                {
                    wall = ability.Value;
                }
                else if (ability.Value.GetType() == typeof(AbilityRangedGrab))
                {
                    rangedgrab = ability.Value;
                } else if (ability.Value.GetType() == typeof (AbilityBomb))
                {
                    bomb = ability.Value;
                }
            }
        }


        //The highest priority for the AI is dodging projectiles, if a projectile is aiming towards the ai and is pretty close the AI will use the shield ability
        //This is the only ability that checks if it has to be used realtime because of the fast nature of projectiles
        var obj = GetClosestTargetFromList(Projectiles);
        if (obj
            && Vector3.Distance(transform.position, obj.transform.position) < 3f
            && shield)
        {
            RaycastHit hit;
            if (Physics.BoxCast(obj.transform.position, BoxSize, obj.transform.forward, out hit, obj.transform.rotation))
            {
                if (hit.transform == transform)
                {
                    StartCoroutine(DelayCastAbility(shield, null, .2f));
                    return;
                }
            }
        }

        //Check offensive options only after refresh
        if (!_refreshed)
            return;

        //If multiple players are nearby always use AOE ability like bomb of shockwave
        if (Players.Count(x => Vector3.Distance(transform.position, x.transform.position) < 1.5f) > 1 && (shockwave || bomb))
        {

            if (bomb && shockwave)
            {
                var random = Random.Range(0f, 1f);
                if (random > .5f)
                {
                    CastAbility(shockwave);
                    return;
                }
                else
                {
                    CastAbility(bomb);
                    return;
                }
            }
            else
            {
                if (shockwave)
                {
                    CastAbility(shockwave);
                    return;
                }
                if (bomb)
                {
                    CastAbility(bomb);
                    return;
                }
            }
        }

        //If a player is right in front of the ai Punch him
        obj = CheckFrontSide(2f);
        if (obj && Players.Contains(obj) && punch)
        {
            CastAbility(punch);
            return;
        }
        
        //if player is in front of the ai but futher away use the wall ability
        obj = CheckFrontSide(5f);
        if (obj && Players.Contains(obj) && wall)
        {
            CastAbility(wall);
            return;
        }

        //Even futher away use shockblast projectile
        obj = CheckFrontSide(15f);
        if (obj && Players.Contains(obj) && shockblast)
        {
            StartCoroutine(DelayCastAbility(shockblast, obj, 1f));
            return;
        }

        //If nothing to do use dash to get closer towards the target faster
        obj = CheckFrontSide(3f);
        if (dash && obj == null && !OnTarget)
        {
            CastAbility(dash);
            return;
        }
    }

    Vector3 GetOptimalRotation()
    {
        List<Vector3> results = new List<Vector3>();

        //Raycast in a full circle and check which options are open
        for (int i = 0; i < 360; i++)
        {
            var rot = transform.rotation * Quaternion.Euler(0, (i * 2) + 180f, 0);
            var dir = rot*transform.forward;
            var origin = transform.position + HeightOffset;
            if (!Physics.BoxCast(
                origin,
                BoxSize, 
                dir,
                transform.rotation,
                1f))
            {
                results.Add((transform.rotation * rot).eulerAngles);
            }
        }

        if (results.Count == 0)
        {
            Log("Failed to find new rotation");
            return transform.rotation.eulerAngles;
        }

        //Return a random opening
        return results[UnityEngine.Random.Range(0, results.Count)];
    }

    /*
    Check which target the AI should move towards
    */
    GameObject GetOptimalTarget()
    {
        GameObject player = GetClosestTargetFromList(Players);
        GameObject pickup = GetClosestTargetFromList(Pickups);
        GameObject objective = GetClosestTargetFromList(ControlPoints);

        var pDis = GetDistance(player);
        var puDis = GetDistance(pickup);
        var oDis = GetDistance(objective);
        
        //Any objective is the highest priority, so check if an objective is within range and move towards it
        if (objective != null && oDis < 10f)
        {

            return objective;
        }

        //Pickups give the AI new options so it is also important
        if (pickup != null && puDis < 6f)
        {
            return pickup;
        }

        //If an alive player is nearby target him
        if (player != null && pDis < 3f && player.GetComponent<Hitpoints>() != null)
        {
            return player;
        }

        //If the ai didn't aquire a target yet move towards any objective no matter what distance
        if (objective != null)
        {
            return objective;
        }

        //Still no target just move towards the nearest player
        if (player != null)
        {
            return player;
        }

        return null;
    }

    GameObject GetClosestTargetFromList(List<GameObject> list)
    {
        GameObject target = null;
        foreach (var item in list)
        {
            if (item == gameObject)
                continue;

            if (target == null)
            {
                target = item;
            }
            else
            {
                if (Vector3.Distance(transform.position, item.transform.position)
                    < Vector3.Distance(transform.position, target.transform.position))
                {
                    target = item;
                }
            }
        }

        return target;
    }

    GameObject CheckFrontSide(float distance, bool filterPlayer = false)
    {
        GameObject result = null;
        var origin = transform.position + HeightOffset;
        foreach (var hit in Physics.BoxCastAll(origin, BoxSize, transform.forward, transform.rotation, distance))
        {
            if (hit.transform.gameObject == gameObject)
                continue;

            if (filterPlayer && Players.Contains(hit.transform.gameObject))
                continue;

            result = hit.transform.gameObject;
            break;
        }

        return result;
    }

    IEnumerator DelayCastAbility(Ability ability, GameObject obj, float delay)
    {
        if (!ability)
            yield break;
        CastAbility(ability);
        _targetObject = obj;
        _state = AiState.Targeting;

        yield return new WaitForSeconds(delay);
        Log(ability.AbilityName);
        ability.Release = true;
        _state = AiState.Regular;
    }

    float GetDistance(GameObject obj)
    {
        if (obj == null)
            return 0f;

        return Vector3.Distance(transform.position, obj.transform.position);
    }

    void Log(string message)
    {
        TriLog.Log("Ai", Index, message);
    }

    [System.Serializable]
    public class Range
    {
        public float Min;
        public float Max;
    }
}

