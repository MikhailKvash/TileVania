using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float runningSpeed = 10f;
    [SerializeField] float jumpSpeed = 20f;
    [SerializeField] float climbingSpeed = 3f;
    [SerializeField] GameObject arrow;
    [SerializeField] Transform bow;

    [SerializeField] AudioClip bowShootSFX;
    [SerializeField] [Range(0f, 1f)] float bowShootVolume = 1f;
    [SerializeField] AudioClip jumpSFX;
    [SerializeField] [Range(0f, 1f)] float jumpVolume = 1f;
    [SerializeField] AudioClip deathSFX;
    [SerializeField] [Range(0f, 1f)] float deathVolume = 1f;
    
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeetCollider;
    Vector2 moveInput;
    Rigidbody2D myRigidbody;    
    Animator myAnimator;
    float startingGravity;

    bool isAlive = true;
    public PhysicsMaterial2D noMaterial;
    private CinemachineImpulseSource myImpulseSource;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
        myAnimator = GetComponent<Animator>();
        myImpulseSource = GetComponent<CinemachineImpulseSource>();
        startingGravity = myRigidbody.gravityScale;
    }

    void Update()
    {
        if (!isAlive) {return;}
        Run();
        FlipSprite();
        ClimbLadder();
        Die();
    }

    void OnFire(InputValue value)
    {
        if (!isAlive) {return;}
        Instantiate(arrow, bow.position, transform.rotation);
        myAnimator.SetTrigger("Shooting");
        AudioSource.PlayClipAtPoint(bowShootSFX, transform.position, bowShootVolume);
    }

    void OnMove(InputValue value)
    {
        if (!isAlive) {return;}
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (!isAlive) {return;}
        if(!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"))) {return;}

        if (value.isPressed)
        {   
            myRigidbody.velocity += new Vector2 (0f, jumpSpeed);
            AudioSource.PlayClipAtPoint(jumpSFX, transform.position, jumpVolume);
        }
    }

    void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * runningSpeed, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon; 
        myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2 (Mathf.Sign(myRigidbody.velocity.x), 1);
        }   
    }

    void ClimbLadder()
    {
        if(!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing"))) 
        {
            myRigidbody.gravityScale = startingGravity;
            myAnimator.SetBool("isClimbing", false);
            myAnimator.SetBool("isIdleClimbing", false);
            return;
        }

        Vector2 climbVelocity = new Vector2(myRigidbody.velocity.x, moveInput.y * climbingSpeed);
        myRigidbody.velocity = climbVelocity;
        myRigidbody.gravityScale = 0f;

        bool playerHasVerticalSpeed = Mathf.Abs(myRigidbody.velocity.y) > Mathf.Epsilon;

        if(playerHasVerticalSpeed)
        {
            myAnimator.SetBool("isClimbing", true);
            myAnimator.SetBool("isIdleClimbing", false);
        }   
        else
        {
            myAnimator.SetBool("isClimbing", false);
            myAnimator.SetBool("isIdleClimbing", true);
        }
    }

    void Die()
    {
        if(isAlive && myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazards", "DeepWater")) || myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazards", "DeepWater")) )
        {
            float randomNumber = Random.Range(-10.0f, 10.0f);
            if(randomNumber == 0f)
            {
                randomNumber = -2f;
            }
            
            myRigidbody.velocity += new Vector2(randomNumber,jumpSpeed+5);

            isAlive = false;
            myAnimator.SetTrigger("Dying");
            myImpulseSource.GenerateImpulse(1);
            AudioSource.PlayClipAtPoint(deathSFX, transform.position, deathVolume);

            GetComponent<CapsuleCollider2D>().sharedMaterial = noMaterial;
            Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemies"), true);

            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }

    public void ResetPlayerCollision()
    {
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemies"), false);
    }
}