using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

// Controller for all characters movement
public class CharacterController2D : MonoBehaviour
{
    public float raycastDistance = 0.2f; // distance for raycasts used to detect ground collision
    public LayerMask layerMask; // filtering out certain geometries
    public float slopeAngleLimit = 45; // maximum slope angle characters can move over
    public float downForceAdjustment = 1.2f; // additional down force for more fluid slope movement

    // flags
    public bool below; // TRUE if something is below the player, FALSE otherwise
    public GroundType groundType; // type of ground the player is standing on

    private Vector2 _moveAmount;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _capsuleCollider;

    private Vector2[] _raycastPosition = new Vector2[3];
    private RaycastHit2D[] _raycastHits = new RaycastHit2D[3]; // information about object colliding with raycast

    private bool _disabledGroundCheck; // disable ground check while beginning a jump

    private Vector2 _slopeNormal; // normal perpendicular to the slope
    private float _slopeAngle; // angle of the slope

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
        if(_slopeAngle != 0 && below == true) // character is on a slope
        {
            if((_moveAmount.x > 0f && _slopeAngle > 0f) || (_moveAmount.x < 0f && _slopeAngle < 0f))
            {
                _moveAmount.y = -Mathf.Abs(Mathf.Tan(_slopeAngle * Mathf.Deg2Rad) *_moveAmount.x);
                _moveAmount.y *= downForceAdjustment;
            }
        }
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
            if(_raycastHits[1].collider) // if the middle raycast collides with something
            {
                groundType = DetermineGroundType(_raycastHits[1].collider);
                _slopeNormal = _raycastHits[1].normal;
                _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up); // calculate angle between up direction and direction of the slope
            }
            else
            {
                for(int i = 0; i < _raycastHits.Length; i++) // check other raycasts
                {
                    if (_raycastHits[i].collider)
                    {
                        groundType = DetermineGroundType(_raycastHits[i].collider);
                        _slopeNormal = _raycastHits[i].normal;
                        _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up); // calculate angle between up direction and direction of the slope
                    }
                }
            }
            if(_slopeAngle > slopeAngleLimit || _slopeAngle < -slopeAngleLimit)
            {
                below = false;
            }
            else
            {
                below = true;
            }
        }
        else
        {
            groundType = GroundType.None;
            below = false;
        }
        System.Array.Clear(_raycastHits, 0, _raycastHits.Length);
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

    private GroundType DetermineGroundType(Collider2D collider)
    {
        if (collider.GetComponent<GroundEffector>())
        {
            GroundEffector groundEffector = collider.GetComponent<GroundEffector>();
            return groundEffector.groundType;
        }
        else
        {
            return GroundType.LevelGeometry;
        }
    }

}