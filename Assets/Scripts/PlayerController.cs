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

    // Player state
    public bool isJumping; // true when the player is jumping

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

        // Update y dimension
        if (_characterController.below) // ground collision detected
        {
            isJumping = false;
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
            _moveDirection.y -= gravity * Time.deltaTime;
        }
        _characterController.Move(_moveDirection * Time.deltaTime);
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
        }
        else if (context.canceled) // button released
        {
            _releaseJump = true;
        }
    }

}
