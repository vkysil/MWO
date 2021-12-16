using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GlobalTypes;
using System;

// Controller for player-specific movement
public class PlayerController : MonoBehaviour
{

    #region public properties
    [Header("Player Properties")]
    public float walkSpeed = 10f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;
    public float doubleJumpSpeed = 15f;
    public float xWallJumpSpeed = 15f;
    public float yWallJumpSpeed = 15f;

    [Header("Player Abilities")]
    // Player ability toggles
    public bool canDoubleJump;
    public bool canWallJump;
    public bool canDoubleJumpAfterWallJump;

    // Player state
    [Header("Player State")]
    public bool isJumping; // true when the player is jumping
    public bool isDoubleJumping; // true when the player is double-jumping
    public bool isWallJumping; // true when the player is wall-jumping
    public bool isCrouching; // true when the player is crouching
    public bool isMovingCrouched; // true when the player is moving while crouched
    #endregion

    #region private properties
    private bool _startJump; // true when jump button started
    private bool _releaseJump; // true when jump button released
    private bool _holdJump; // true when jump button is held down

    private Vector2 _input;
    private Vector2 _moveDirection;
    private CharacterController2D _characterController;

    private CapsuleCollider2D _capsuleCollider;
    private Vector2 _originalColliderSize;
    // TODO: remove later when we add separate sprites for animations
    private SpriteRenderer _spriteRenderer;

    // jump pad properties
    private float _jumpPadAmount = 15f;
    private float _jumpPadAdjustment = 0f;
    private Vector2 _tempVelocity; // velocity before hitting the ground
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController2D>();
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _originalColliderSize = _capsuleCollider.size;
    }

    // Update is called once per frame
    void Update()
    {

        ProcessHorizontalMovement();

        // Update y dimension
        if (_characterController.below) // ground collision detected
        {
            OnGround();
        }
        else // no ground collision detected
        {
            InAir();
        }
        _characterController.Move(_moveDirection * Time.deltaTime);
    }

    void ProcessHorizontalMovement()
    {
        // Update x dimension
        // unless during a wall-jump
        if (!isWallJumping)
        {
            _moveDirection.x = _input.x;
            _moveDirection.x *= walkSpeed;
            if (_moveDirection.x < 0)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else if (_moveDirection.x > 0)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
    }

    // method responsible for interactions while collision below character is detected
    void OnGround()
    {
        if (_characterController.hitGroundThisFrame)
        {
            _tempVelocity = _moveDirection;
        }

        // clear any downward motion while on ground
        _moveDirection.y = 0f;

        ClearAirAbilityFlags();

        Jump();
        Crouch();
        JumpPad();

    }

    // jump pad
    private void JumpPad()
    {
        if(_characterController.groundType == GroundType.JumpPad)
        {
            _jumpPadAmount = _characterController.jumpPadAmount;

            // if velocity while touching the jump pad
            // is greater than jump pad amount
            if(-_tempVelocity.y > _jumpPadAmount)
            {
                _moveDirection.y = -_tempVelocity.y * 0.92f;
            }
            else
            {
                _moveDirection.y = _jumpPadAmount;
            }

            // if holding jump button and a little each time we bounce
            if (_holdJump)
            {
                _jumpPadAdjustment += _moveDirection.y * 0.1f;
                _moveDirection.y += _jumpPadAdjustment;
            }
            else
            {
                _jumpPadAdjustment = 0f;
            }
            // enforce upper limit for the jump
            if(_moveDirection.y > _characterController.jumpPadUpperLimit)
            {
                _moveDirection.y = _characterController.jumpPadUpperLimit;
            }
        }
    }

    // clear flags for all in-air abilities
    void ClearAirAbilityFlags()
    {
        isJumping = false;
        isDoubleJumping = false;
        isWallJumping = false;
    }

    // method for jumping-related interactions
    void Jump()
    {
        if (_startJump) // jumping event initiated
        {
            _startJump = false;
            // check if we're on a one-way platform
            if (isCrouching && _characterController.groundType == GroundType.OneWayPlatform)
            {
                StartCoroutine(DisableOneWayPlatform(true));
            }
            else
            {
                _moveDirection.y = jumpSpeed;
            }
            isJumping = true;
            _characterController.DisableGroundCheck();
        }
    }

    // method for crouching and moving while crouched
    void Crouch()
    {
        if (_input.y < 0f)
        {
            if (!isCrouching && !isMovingCrouched)
            {
                _capsuleCollider.size = new Vector2(_capsuleCollider.size.x, _capsuleCollider.size.y / 2);
                transform.position = new Vector2(transform.position.x, transform.position.y - (_originalColliderSize.y / 4));
                isCrouching = true;
                _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp_crouching");
            }

        }
        else
        {
            if (isCrouching || isMovingCrouched)
            {
                RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center,
                    transform.localScale, CapsuleDirection2D.Vertical, 0f, Vector2.up, _originalColliderSize.y / 2,
                    _characterController.layerMask); // checking if anything is above the player
                if (!hitCeiling.collider)
                {
                    _capsuleCollider.size = _originalColliderSize;
                    transform.position = new Vector2(transform.position.x, transform.position.y + (_originalColliderSize.y / 4));
                    _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp");
                    isCrouching = false;
                    isMovingCrouched = false;
                }
            }
        }
        if (isCrouching && _moveDirection.x != 0)
        {
            isMovingCrouched = true;
        }
        else
        {
            isMovingCrouched = false;
        }
    }

    // method responsible for interactions while collision below character is NOT detected
    void InAir()
    {

        AirCrouch();
        AirJump();

        GravityCalculations();
    }

    void AirCrouch()
    {
        if((isCrouching || isMovingCrouched) && _moveDirection.y > 0)
        {
            StartCoroutine("ClearCrouchingState");
        }
        
    }

    // method for jump-related interactions while in-air
    void AirJump()
    {
        // smaller jump when jump button was released early
        if (_releaseJump)
        {
            _releaseJump = false;
            if (_moveDirection.y > 0)
            {
                _moveDirection.y *= 0.5f;
            }
        }

        // pressed jump while in the air
        if (_startJump)
        {
            // double-jump
            if (canDoubleJump && (!_characterController.left && !_characterController.right))
            {
                if (!isDoubleJumping)
                {
                    _moveDirection.y = doubleJumpSpeed;
                    isDoubleJumping = true;
                }
            }
            // wall-jump
            else if (canWallJump && (_characterController.left || _characterController.right))
            {
                if (_moveDirection.x <= 0 && _characterController.left)
                {
                    _moveDirection.x = xWallJumpSpeed;
                    _moveDirection.y = yWallJumpSpeed;
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }
                else if (_moveDirection.x >= 0 && _characterController.right)
                {
                    _moveDirection.x = -xWallJumpSpeed;
                    _moveDirection.y = yWallJumpSpeed;
                    transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }

                StartCoroutine("WallJumpWaiter"); // how soon after wall-jump player regains control over x-movement
                if (canDoubleJumpAfterWallJump)
                {
                    isDoubleJumping = false;
                }
                else
                {
                    isDoubleJumping = true;
                }
            }
            _startJump = false;
        }
    }

    // gravity behaviour while jumping
    void GravityCalculations()
    {
        if(_moveDirection.y > 0f && _characterController.above)
        {
            if(_characterController.ceilingType == GroundType.OneWayPlatform)
            {
                StartCoroutine(DisableOneWayPlatform(false));
            }
            else
            {
                _moveDirection.y = 0f; // stop vertical movement when colliding with an object above
            }
        }
        _moveDirection.y -= gravity * Time.deltaTime;
    }

    #region input methods
    // Method is called on each input event
    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context) 
    {
        if (context.started) // button pressed
        {
            _startJump = true;
            _releaseJump = false;
            _holdJump = true;
        }
        else if (context.canceled) // button released
        {
            _releaseJump = true;
            _startJump = false;
            _holdJump = false;
        }
    }
    #endregion

    #region coroutines
    IEnumerator WallJumpWaiter()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(0.4f);
        isWallJumping = false;
    }

    IEnumerator ClearCrouchingState()
    {
        yield return new WaitForSeconds(0.05f);
        RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, transform.localScale,
            CapsuleDirection2D.Vertical, 0f, Vector2.up, _originalColliderSize.y / 2, _characterController.layerMask);
        if (!hitCeiling.collider)
        {
            _capsuleCollider.size = _originalColliderSize;
            _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp");
            isCrouching = false;
            isMovingCrouched = false;
        }
    }

    IEnumerator DisableOneWayPlatform(bool checkBelow)
    {
        GameObject tempOneWayPlatform = null;
        if (checkBelow) // player is ontop of the platform
        {
            Vector2 raycastBelow = transform.position - new Vector3(0, _capsuleCollider.size.y * 0.5f, 0);
            RaycastHit2D hit = Physics2D.Raycast(raycastBelow, Vector2.down, _characterController.raycastDistance, 
                _characterController.layerMask);
            if (hit.collider)
            {
                tempOneWayPlatform = hit.collider.gameObject;
            }
        }
        else // player is below the platform
        {
            Vector2 raycastAbove = transform.position + new Vector3(0, _capsuleCollider.size.y * 0.5f, 0);
            RaycastHit2D hit = Physics2D.Raycast(raycastAbove, Vector2.up, _characterController.raycastDistance,
                _characterController.layerMask);
            if (hit.collider)
            {
                tempOneWayPlatform = hit.collider.gameObject;
            }
        }
        if (tempOneWayPlatform)
        {
            // switching off platform collider
            tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = false; 
        }
        yield return new WaitForSeconds(0.75f);
        if (tempOneWayPlatform)
        {
            // switching platform collider back on
            tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = true; 
        }
    }
    #endregion
}
