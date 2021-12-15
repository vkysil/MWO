using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controller for all characters movement
public class CharacterController2D : MonoBehaviour
{
    public float raycastDistance = 0.2f; // distance for raycasts used to detect ground collision
    public LayerMask layerMask; // filtering out certain geometries

    // flags
    public bool below; // TRUE if something is below the player, FALSE otherwise

    private Vector2 _moveAmount;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _capsuleCollider;

    private Vector2[] _raycastPosition = new Vector2[3];
    private RaycastHit2D[] _raycastHits = new RaycastHit2D[3]; // information about object colliding with raycast

    private bool _disabledGroundCheck;

    // Start method
    void Start()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
    }

    // FixedUpdate for the physics engine
    void FixedUpdate()
    {
        _lastPosition = _rigidbody.position;
        _currentPosition = _lastPosition + _moveAmount;
        _rigidbody.MovePosition(_currentPosition);
        _moveAmount = Vector2.zero;
        if (!_disabledGroundCheck)
        {
            CheckGrounded();
        }
    }

    // Movement update method adjusted to framerate
    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }

    // Check if player character collides with the ground
    private void CheckGrounded()
    {
        Vector2 raycastOrigin = _rigidbody.position - new Vector2(0, _capsuleCollider.size.y * 0.5f);

        _raycastPosition[0] = raycastOrigin + (Vector2.left * _capsuleCollider.size.x * 0.25f + Vector2.up * 0.1f);
        _raycastPosition[1] = raycastOrigin;
        _raycastPosition[2] = raycastOrigin + (Vector2.right * _capsuleCollider.size.x * 0.25f + Vector2.up * 0.1f);

        DrawDebugRays(Vector2.down, Color.green);

        // how many ray collisions with the ground happened
        int numberOfGroundHits = 0;

        for (int i = 0; i < _raycastPosition.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(_raycastPosition[i], Vector2.down, raycastDistance, layerMask);
            if (hit.collider)
            {
                _raycastHits[i] = hit;
                numberOfGroundHits++;
            }
        }

        if (numberOfGroundHits > 0)
        {
            below = true;
        }
        else
        {
            below = false;
        }
    }

    // Debug method making raycasts visible
    private void DrawDebugRays(Vector2 direction, Color color)
    {
        for (int i = 0; i < _raycastPosition.Length; i++ )
        {
            Debug.DrawRay(_raycastPosition[i], direction * raycastDistance, color);
        }
    }

    public void DisableGroundCheck()
    {
        below = false;
        _disabledGroundCheck = true;
        StartCoroutine("EnableGroundCheck");
    }

    IEnumerator EnableGroundCheck()
    {
        yield return new WaitForSeconds(0.1f);
        _disabledGroundCheck = false;
    }

}