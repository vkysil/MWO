using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Controller for player-specific movement
public class PlayerController : MonoBehaviour
{
    // Player-specific properties
    public float walkSpeed = 10f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;
    public float doubleJumpSpeed = 15f;

    // Player ability toggles
    public bool canDoubleJump;

    // Player state
    public bool isJumping; // true when the player is jumping
    public bool isDoubleJumping; // true when the player is double-jumping

    // Input properties
    private bool _startJump; // true when jump button started
    private bool _releaseJump; // true when jump button released

    private Vector2 _input;
    private Vector2 _moveDirection;
    private CharacterController2D _characterController;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController2D>();
    }

    // Update is called once per frame
    void Update()
    {

        // Update x dimension
        _moveDirection.x = _input.x;
        _moveDirection.x *= walkSpeed;

        if(_moveDirection.x < 0)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        } else if(_moveDirection.x > 0)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        // Update y dimension
        if (_characterController.below) // ground collision detected
        {
            _moveDirection.y = 0f;

            isJumping = false;
            isDoubleJumping = false;

            if(_startJump) // jumping event initiated
            {
                _startJump = false;
                _moveDirection.y = jumpSpeed;
                isJumping = true;
                _characterController.DisableGroundCheck();
            }
        }
        else // no ground collision detected
        {
            // smaller jump when jump button was released early
            if(_releaseJump)
            {
                _releaseJump = false;
                if(_moveDirection.y > 0)
                {
                    _moveDirection.y *= 0.5f;
                }
            }

            // double-jumping
            if (_startJump)
            {
                if(canDoubleJump && (!_characterController.left && !_characterController.right))
                {
                    if (!isDoubleJumping)
                    {
                        _moveDirection.y = doubleJumpSpeed;
                        isDoubleJumping = true;
                    }
                }
                _startJump = false;
            }

            GravityCalculations();
        }
        _characterController.Move(_moveDirection * Time.deltaTime);
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

    // Input methods
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

}
