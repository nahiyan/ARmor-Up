﻿using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [HideInInspector] public CharacterController charController;
    // private LightshipNavMeshAgent agent;
    // private LightshipNavMesh navMesh;
    [HideInInspector] public Animator animator;
    public GameObject childPlayer;
    public Camera cam;
    public GameObject movIndicator; //Where is the character moving to

    // [HideInInspector] public SoundManager soundMan;

    public float speed = 0.3f; //Speed of the player
    public float jumpSpeed = 1.0f; //How high does the player jump
    public float gravity = 20.0f; //Gravity applied to the player
    private bool canJump = true;
    private bool wasGrounded = false; //Dettects the first time the player is falling

    private float mass = 60.0f; // Defines the character mass
    private Vector3 impact = Vector3.zero;

    public bool airControl = true; //If player can control the direction of the movement on falling
    private float fallTime = 0f; //Time the player is falling

    public float maxDashTime = 0.25f; //Time that the player is dashing
    private float currentDashTime;
    public float dashSpeed = 20; //Dash speed of the player
    private Vector3 dashDir;
    // private bool canDash = true;

    private Vector3 moveDirection = Vector3.zero;

    private float distToGround; //Distance to the ground for check if there is ground under the character
    private Vector3 groundNormal;

    [HideInInspector] public bool canMove = true; //If player can move or not because is attaking or hitted
    private bool hit = false; //If player is hitted it will be true

    // Input controls
    InputAction attackAction, moveAction, jumpAction;

    void Start()
    {
        charController = GetComponent<CharacterController>();
        Assert.IsNotNull(charController);
        // soundMan = GetComponent<SoundManager>();
        animator = GetComponentInChildren<Animator>();
        Assert.IsNotNull(animator);
        currentDashTime = maxDashTime;
        distToGround = charController.bounds.extents.y;

        attackAction = InputSystem.actions.FindAction("Attack");
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        Assert.IsNotNull(attackAction);
        Assert.IsNotNull(moveAction);
        Assert.IsNotNull(jumpAction);
    }

    void Update()
    {
        if (charController.isGrounded)
        {
            if (!wasGrounded) //If it is the frame when player touches the ground
            {
                canJump = true;
                animator.SetBool("Jump", false);
                if (fallTime > 0.2f)
                {
                    // soundMan.PlaySound("Land");
                    if (!hit)
                        animator.CrossFade("FallingEnd", 0.1f);
                }
                fallTime = 0f;
            }
        }
        else
        {
            animator.SetFloat("SpeedY", charController.velocity.y);
            if (wasGrounded && canJump) //If it is the first frame of falling
            {
                if (DistToGround() > 0.3f) //If the ground is far enough
                {
                    moveDirection.y = 0f;
                    wasGrounded = false;
                    animator.SetBool("Jump", true); //Needed for activate the jump state if character falls
                    animator.CrossFade("Falling", 0.2f);
                }
            }
            if (charController.velocity.y < 0) //If player is falling down
                fallTime += Time.deltaTime;
        }
        wasGrounded = charController.isGrounded;

        if (!canMove)
        {
            if (hit)
            {
                moveDirection.y -= gravity * Time.deltaTime;
                Vector3 impactGrav = new Vector3(impact.x, impact.y + moveDirection.y, impact.z); //Adds gravity to the impact force
                                                                                                  // apply the impact force:
                if (impact.magnitude > 0.2f || !charController.isGrounded) charController.Move(impactGrav * Time.deltaTime);
                // consumes the impact energy each cycle:
                impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);

                if (charController.isGrounded && impact.magnitude <= 0.2f)
                {
                    hit = false;
                    canMove = true;
                    animator.Play("Idle"); //To unlock the animation in some weird cases
                }
            }
            return;
        }

        if (charController.isGrounded)
        {
            Vector2 moveVector = moveAction.ReadValue<Vector2>();
            moveDirection = new Vector3(moveVector.x, 0, moveVector.y);
            if (moveDirection.magnitude < 0.1f) //Because if velocity is minor than 0.1 the animator dont play the correct animation
                moveDirection = Vector3.zero;
            moveDirection = Vector3.ClampMagnitude(moveDirection, 1); //Limits the vector magnitude to 1
            animator.SetFloat("Speed", moveDirection.magnitude);

            movIndicator.transform.localPosition = moveDirection; //Positionate the reference that indicates the direction of the movement

            // if (Input.GetButtonDown("Dash") && canDash) //Dash
            // {
            //     currentDashTime = 0;
            //     canDash = false;
            //     anim.Play("Slide");
            //     // soundMan.PlaySound("Dash");
            //     if (moveDirection != Vector3.zero)
            //     {
            //         dashDir = transform.TransformDirection(moveDirection).normalized;

            //         //Guide the player to where they are going to move
            //         Vector3 targetActPosition = new Vector3(movIndicator.transform.position.x, childPlayer.transform.position.y, movIndicator.transform.position.z);
            //         childPlayer.transform.rotation = Quaternion.LookRotation(targetActPosition - childPlayer.transform.position);
            //     }
            //     else
            //         dashDir = childPlayer.transform.forward;
            // }
            if (currentDashTime < maxDashTime)
            {
                dashDir.y = -10f;
                currentDashTime += Time.deltaTime;

                charController.Move(dashDir * Time.deltaTime * dashSpeed);
                return;
            }
            // canDash = true;

            if (moveDirection.magnitude > 0) //Fixes the problem when there is no movement
            {
                //To rotate the controller when moving and position it correctly relative to the camera
                if (cam != null)
                    charController.transform.rotation = new Quaternion(charController.transform.rotation.x, cam.transform.rotation.y, charController.transform.rotation.z, cam.transform.rotation.w);

                //Smoothly rotate the character in the xz plane towards the direction of movement
                Vector3 targetActPosition = new Vector3(movIndicator.transform.position.x, childPlayer.transform.position.y, movIndicator.transform.position.z);
                Quaternion rotation = Quaternion.LookRotation(targetActPosition - childPlayer.transform.position);
                childPlayer.transform.rotation = Quaternion.Slerp(childPlayer.transform.rotation, rotation, Time.deltaTime * 10);
            }

            //Rotate it to the player orientation
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed; // apply the horizontal speed


            moveDirection.y = -10f; //To prevent the controller from taking off when going down ramps
            if (!IsGrounded()) //If the character controller says is grounded but the raycast to the ground no
            {
                moveDirection.x += (1f - groundNormal.y) * groundNormal.x;
                moveDirection.z += (1f - groundNormal.y) * groundNormal.z;
            }

            if (jumpAction.WasPerformedThisFrame() && canJump)
            {
                moveDirection.y = jumpSpeed;
                canJump = false;
                animator.SetFloat("SpeedY", moveDirection.y);
                animator.Play("Falling");
                animator.SetBool("Jump", true);
                // soundMan.PlaySound("Jump");
            }

        }
        else
        {
            if (currentDashTime < maxDashTime) //If player was dashing
            {
                currentDashTime = maxDashTime;
                // canDash = true;
            }
            if (airControl)
            {
                Vector2 moveVector = moveAction.ReadValue<Vector2>();
                Vector3 moveDirectionTemp = new Vector3(moveVector.x, 0, moveVector.y);
                moveDirectionTemp = Vector3.ClampMagnitude(moveDirectionTemp, 1); //Limits the vector magnitude to 1
                moveDirection = new Vector3(moveDirectionTemp.x, moveDirection.y, moveDirectionTemp.z);

                movIndicator.transform.localPosition = new Vector3(moveDirection.x, 0, moveDirection.z); //Positionate the reference that indicates the direction of the movement

                if (moveDirectionTemp.magnitude > 0) //Fixes the problem when there is no movement
                {
                    //To rotate the controller when moving and position it correctly relative to the camera
                    if (cam != null)
                        charController.transform.rotation = new Quaternion(charController.transform.rotation.x, cam.transform.rotation.y, charController.transform.rotation.z, cam.transform.rotation.w);

                    //Smoothly rotate the character in the xz plane towards the direction of movement
                    Vector3 targetActPosition = new Vector3(movIndicator.transform.position.x, childPlayer.transform.position.y, movIndicator.transform.position.z);
                    Quaternion rotation = Quaternion.LookRotation(targetActPosition - childPlayer.transform.position);
                    childPlayer.transform.rotation = Quaternion.Slerp(childPlayer.transform.rotation, rotation, Time.deltaTime * 10);
                }
                // rotate it to the player orientation
                moveDirection = transform.TransformDirection(moveDirection);
                moveDirection = new Vector3(moveDirection.x * speed * 0.8f, moveDirection.y, moveDirection.z * speed * 0.8f);
            }
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        charController.Move(moveDirection * Time.deltaTime);

        // Attack
        if (attackAction.WasPerformedThisFrame())
        {
            if (canMove)
            {
                if (charController.isGrounded && !IsDashing())
                {
                    animator.Play("Attack01_SwordAndShiled");
                    // playerCont.soundMan.PlaySound("Attack");
                }
            }
        }
    }

    public void AddImpact(Vector3 dir, float force) //Apply a force to the player
    {
        moveDirection = Vector3.zero;
        animator.Play("Hit");

        dir.Normalize();
        if (dir.y < 0) dir.y = -dir.y; //Reflect down force on the ground
        impact += dir.normalized * force / mass;
    }

    public void ApplyDMG(Vector3 dir, float force) //Apply damage to the player
    {
        if (!hit)
        {
            hit = true;
            canMove = false;
            // soundMan.PlaySound("Hit");
            currentDashTime = maxDashTime; //Cancels dash if was pressed
            animator.SetFloat("Speed", 0);
            // anim.GetComponent<AnimatorEvents>().DisableWeaponColl();
            AddImpact(dir, force);
        }
    }

    float DistToGround() //Calculates the distance to the ground when starts to fall
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, distToGround + 999))
            return hit.distance - distToGround;
        else return 999;
    }

    bool IsGrounded() //Check if ground is under the character
    {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        groundNormal = hit.normal;
        //groundAngle = Vector3.Angle(groundNormal, Vector3.up);
    }

    public bool IsDashing() //Checks if player if dashing
    {
        return currentDashTime < maxDashTime;
    }

    public void EnableMove(bool camMoveT) //Enables or disables the character movement
    {
        if (!hit)
            canMove = camMoveT;
    }

    public bool IsAttacking()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Attack01_SwordAndShiled");
    }
}
