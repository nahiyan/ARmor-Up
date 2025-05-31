using Niantic.Lightship.AR.NavigationMesh;
using UnityEngine;
using UnityEngine.Assertions;
using static Niantic.Lightship.AR.NavigationMesh.LightshipNavMeshAgent;

public class Enemy : MonoBehaviour
{

    public LightshipNavMesh navmesh;
    private LightshipNavMeshAgent agent;
    Animator anim;
    // SoundManager soundMan;
    // AnimatorEventsEn animEv;
    public Player player;
    public enum STATE
    {
        IDLE, PATROL, CHASE, MELEEATTACK, HIT
    }
    public STATE currState = STATE.IDLE;

    private float waitTimer = 0;
    public float attackTime = 1.0f; //Time between attacks

    private bool isInvincible = false;

    float visDist = 10.0f; //Distance of vision
    float visAngle = 90.0f; //Angle of the cone vision
    float meleeDist = 1.5f; //Distance from which the enemy will attack the player

    public GameObject damageTextPrefab;
    public Transform damageTextPos;
    private CapsuleCollider coll;
    private int hp = 100;

    void Start()
    {
        agent = GetComponent<LightshipNavMeshAgent>();
        anim = GetComponent<Animator>();
        // animEv = this.GetComponentInChildren<AnimatorEventsEn>();
        // soundMan = GetComponent<SoundManager>();
        coll = GetComponent<CapsuleCollider>();

        Assert.IsNotNull(agent, "Navmesh Agent null");
        Assert.IsNotNull(anim, "Animator null");
        Assert.IsNotNull(coll, "Collision null");

        // Patrol by default
        if (navmesh != null && navmesh.IsOnNavMesh(transform.position, 1))
        {
            Debug.Log("Patrolling!");
            ChangeState(STATE.PATROL);
        }
    }

    void Update()
    {
        switch (currState)
        {
            case STATE.IDLE:
                if (CanSeePlayer())
                {
                    ChangeState(STATE.CHASE);
                }
                else if (Random.Range(0, 100) < 10)
                {
                    ChangeState(STATE.PATROL);
                }
                break;
            case STATE.PATROL:
                if (navmesh == null)
                    break;

                if (agent.State == AgentNavigationState.Idle)
                {
                    navmesh.FindRandomPosition(transform.position, 30f, out Vector3 randomPos);
                    agent.SetDestination(randomPos);
                }


                if (CanSeePlayer())
                {
                    ChangeState(STATE.CHASE);
                }
                break;
            case STATE.CHASE:
                agent.SetDestination(player.transform.position);
                // TODO: Has path?
                // if (agent.hasPath)
                // {
                if (CanAttackPlayer())
                {
                    ChangeState(STATE.MELEEATTACK);
                }
                else if (CanStopChase())
                {
                    ChangeState(STATE.PATROL);
                }
                // }
                break;
            case STATE.MELEEATTACK:
                LookPlayer(2.0f);

                waitTimer += Time.deltaTime;
                if (waitTimer >= attackTime)
                {
                    if (CanAttackPlayer())
                        ChangeState(STATE.CHASE);
                    else
                        ChangeState(STATE.PATROL);
                }

                break;
            case STATE.HIT:
                waitTimer += Time.deltaTime;
                if (waitTimer < 0.5f)
                {
                    LookPlayer(5.0f);
                }
                else if (waitTimer >= 0.5f && isInvincible)
                {
                    isInvincible = false;
                }
                else if (waitTimer >= 1.25f)
                {
                    if (CanAttackPlayer())
                        ChangeState(STATE.CHASE);
                    else
                        ChangeState(STATE.PATROL);
                }
                break;
        }
    }

    public bool CanSeePlayer()
    {
        // Vector3 direction = player.transform.position - transform.position;
        // float angle = Vector3.Angle(direction, transform.forward);

        // if (direction.magnitude < visDist && angle < visAngle)
        // {
        //     return true;
        // }
        return false;
    }

    public bool CanAttackPlayer()
    {
        // Vector3 direction = player.transform.position - transform.position;
        // if (direction.magnitude < meleeDist)
        // {
        //     return true;
        // }
        return false;
    }

    public bool CanStopChase() //Stop follow the player
    {
        Vector3 direction = player.transform.position - transform.position;
        if (direction.magnitude > visDist)
        {
            return true;
        }
        return false;
    }

    public void ChangeState(STATE newState)
    {
        switch (currState)
        {
            case STATE.IDLE:
                anim.ResetTrigger("isIdle");
                break;
            case STATE.PATROL:
                anim.ResetTrigger("isPatrolling");
                break;
            case STATE.CHASE:
                anim.ResetTrigger("isChasing");
                break;
            case STATE.MELEEATTACK:
                anim.ResetTrigger("isMeleeAttacking");
                // animEv.isAttacking = false;
                // anim.GetComponent<AnimatorEventsEn>().DisableWeaponColl();
                break;
            case STATE.HIT:
                anim.ResetTrigger("isHited");
                break;
        }
        switch (newState)
        {
            case STATE.IDLE:
                anim.SetTrigger("isIdle");
                break;
            case STATE.PATROL:
                if (navmesh == null)
                    break;
                navmesh.FindRandomPosition(transform.position, 30f, out Vector3 randomPos);
                agent.SetDestination(randomPos);
                anim.SetTrigger("isPatrolling");
                break;
            case STATE.CHASE:
                // TODO: Chase
                // agent.speed = 3;
                // agent.isStopped = false;
                anim.SetTrigger("isChasing");
                break;
            case STATE.MELEEATTACK:
                // TODO: Attack
                // agent.isStopped = true;
                waitTimer = 0;
                // animEv.isAttacking = true;
                anim.SetTrigger("isMeleeAttacking");
                break;
            case STATE.HIT:
                // TODO: Hit
                // agent.isStopped = true;
                waitTimer = 0;
                anim.SetTrigger("isHited");
                break;
        }

        currState = newState;
    }

    public void ApplyDmg(DmgInfo dmgInfo) // Apply damage to the enemy
    {
        if (!isInvincible && player.IsAttacking())
        {
            isInvincible = true;
            ChangeState(STATE.HIT);
            // soundMan.PlaySound("Hit");
            GameObject dmgText = Instantiate(damageTextPrefab, damageTextPos.position, Quaternion.identity);
            dmgText.GetComponent<DamagePopup>().SetUp(dmgInfo.dmgValue + Random.Range(-10, 10), dmgInfo.textColor);

            hp -= dmgInfo.dmgValue;
            if (hp <= 0)
            {
                // TODO: Agent stopped
                // agent.isStopped = true;
                agent.StopMoving();
                anim.Play("Die");

                Destroy(gameObject, 2f);
            }
        }
    }

    private void LookPlayer(float speedRot)
    {
        Vector3 direction = player.transform.position - transform.position;
        float angle = Vector3.Angle(direction, transform.forward);
        direction.y = 0;

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * speedRot);
    }

    public bool IsAttacking()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Attack01");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player" && IsAttacking())
        {
            other.GetComponent<Player>().ApplyDMG(other.transform.position - transform.position, 250f);
        }
    }
}
