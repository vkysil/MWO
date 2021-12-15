using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    #endregion

    #region private properties
    private bool _startJump; // true when jump button started
    private bool _releaseJump; // true when jump button released

    private Vector2 _input;
    private Vector2 _moveDirection;
    private CharacterController2D _characterController;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController2D>();
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
        // clear any downward motion while on ground
        _moveDirection.y = 0f;

        ClearAirAbilityFlags();

        Jump();

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
            _moveDirection.y = jumpSpeed;
            isJumping = true;
            _characterController.DisableGroundCheck();
        }
    }

    // method responsible for interactions while collision below character is NOT detected
    void InAir()
    {
        AirJump();

        GravityCalculations();
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
            }
            _startJump = false;
        }
    }

    // gravity behaviour while jumping
    void GravityCalculations()
    {
        if(_moveDirection.y > 0f && _characterController.above)
        {
            _moveDirection.y = 0f; // stop vertical movement when colliding with an object above
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
        }
        else if (context.canceled) // button released
        {
            _releaseJump = true;
            _startJump = false;
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
    #endregion
}
