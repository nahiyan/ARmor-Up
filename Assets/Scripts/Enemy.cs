using Niantic.Lightship.AR.NavigationMesh;
using UnityEngine;
using UnityEngine.Assertions;
using static Niantic.Lightship.AR.NavigationMesh.LightshipNavMeshAgent;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private GameObject weapon;
    [SerializeField]
    private GameObject statusText;
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
    float meleeDist = 0.5f; //Distance from which the enemy will attack the player

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
        Assert.IsNotNull(statusText, "Status text null");

        // Patrol by default
        if (navmesh != null && navmesh.IsOnNavMesh(transform.position, 1))
        {
            Debug.Log("Patrolling!");
            ChangeState(STATE.PATROL);
        }
    }

    void Update()
    {
        if (statusText != null && Camera.main != null)
        {
            statusText.transform.LookAt(Camera.main.transform);
            statusText.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }

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
                if (CanAttackPlayer())
                {
                    ChangeState(STATE.MELEEATTACK);
                }
                else if (CanStopChase())
                {
                    ChangeState(STATE.PATROL);
                }
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
        Vector3 direction = player.transform.position - transform.position;
        float angle = Vector3.Angle(direction, transform.forward);

        if (direction.magnitude < visDist && angle < visAngle)
        {
            return true;
        }
        return false;
    }

    public bool CanAttackPlayer()
    {
        Vector3 direction = player.transform.position - transform.position;
        if (direction.magnitude < meleeDist)
        {
            return true;
        }
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
        // Previous state
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
                weapon.GetComponent<Collider>().enabled = false;
                break;
            case STATE.HIT:
                anim.ResetTrigger("isHited");
                break;
        }

        // New state
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
                // Chase
                // agent.speed = 3;
                // agent.isStopped = false;
                anim.SetTrigger("isChasing");
                break;
            case STATE.MELEEATTACK:
                // Attack
                waitTimer = 0;
                weapon.GetComponent<Collider>().enabled = true;
                agent.StopMoving();
                // animEv.isAttacking = true;
                anim.SetTrigger("isMeleeAttacking");
                break;
            case STATE.HIT:
                // Hit
                agent.StopMoving();
                waitTimer = 0;
                anim.SetTrigger("isHited");
                break;
        }

        statusText.GetComponent<TextMesh>().text = newState.ToString() + " " + (IsAttacking() ? "attacking" : "");

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
}
