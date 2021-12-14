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

        }
        else // no ground collision detected
        {
            _moveDirection.y -= gravity * Time.deltaTime;
        }
        

        _characterController.Move(_moveDirection * Time.deltaTime);

    }

    // Method is called on each input event
    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }
}
