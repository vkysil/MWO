using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

// Controller for all characters movement
public class CharacterController2D : MonoBehaviour
{
    public float raycastDistance = 0.2f; // distance for raycasts used to detect ground collision
    public LayerMask layerMask; // filtering out certain layers we're not interested in
    public float slopeAngleLimit = 45; // maximum slope angle characters can move over
    public float downForceAdjustment = 1.2f; // additional down force for more fluid slope movement

    // collision flags
    public bool below; // TRUE if something is below the player, FALSE otherwise
    public bool left; // TRUE if something is to the left side of the player, FALSE otherwise
    public bool right; // TRUE if something is to the right side of the player, FALSE otherwise
    public bool above; // TRUE if something is above the player, FALSE otherwise
    
    public GroundType groundType; // type of ground the player is standing on
    public GroundType ceilingType;
    public WallType leftWallType; // temp
    public WallType rightWallType; // temp

    public bool hitGroundThisFrame; // TRUE if character collided with the ground in the current frame

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

    private bool _inAirLastFrame; // TRUE if character wasn't colliding with the ground last frame

    private Transform _tempMovingPlatform; // used to keep a character standing on the moving platform
    private Vector2 _movingPlatformVelocity; // velocity of the moving platform

    // Start method
    void Start()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
    }

    // Update for the physics engine
    void Update()
    {
        _inAirLastFrame = !below;
        _lastPosition = _rigidbody.position;

        // character is on a slope
        if (_slopeAngle != 0 && below == true) 
        {
            if((_moveAmount.x > 0f && _slopeAngle > 0f) || (_moveAmount.x < 0f && _slopeAngle < 0f))
            {
                _moveAmount.y = -Mathf.Abs(Mathf.Tan(_slopeAngle * Mathf.Deg2Rad) *_moveAmount.x);
                _moveAmount.y *= downForceAdjustment;
            }
        }

        // character is on a moving platform
        if(groundType == GroundType.MovingPlatform)
        {
            // offset the player's movement on the X with the platform velocity
            _moveAmount.x += MovingPlatformAdjust().x;

            // if platform is moving down
            // offset the player's movement on the Y with the platform velocity
            if (MovingPlatformAdjust().y < 0f)
            {
                _moveAmount.y += MovingPlatformAdjust().y;
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
        CheckOtherCollisions();

        if(below && _inAirLastFrame)
        {
            hitGroundThisFrame = true;
        }
        else
        {
            hitGroundThisFrame = false;
        }
    }

    private void CheckOtherCollisions()
    {
        // check left
        RaycastHit2D leftHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size * 0.75f, 0f, Vector2.left,
            raycastDistance * 2, layerMask);
        if (leftHit.collider)
        {
            leftWallType = DetermineWallType(leftHit.collider);
            left = true;
        }
        else
        {
            leftWallType = WallType.None;
            left = false;
        }

        // check right
        RaycastHit2D rightHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size * 0.75f, 0f, Vector2.right,
            raycastDistance * 2, layerMask);
        if (rightHit.collider)
        {
            rightWallType = DetermineWallType(rightHit.collider);
            right = true;
        }
        else
        {
            rightWallType = WallType.None;
            right = false;
        }

        // check above
        RaycastHit2D aboveHit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, CapsuleDirection2D.Vertical,
            0f, Vector2.up, raycastDistance, layerMask);
        if (aboveHit.collider)
        {
            ceilingType = DetermineGroundType(aboveHit.collider);
            above = true;
        }
        else
        {
            ceilingType = GroundType.None;
            above = false;
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
        RaycastHit2D hit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, CapsuleDirection2D.Vertical,
            0f, Vector2.down, raycastDistance, layerMask);
        if (hit.collider)
        {
            groundType = DetermineGroundType(hit.collider);
            _slopeNormal = hit.normal;
            _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);

            if (_slopeAngle > slopeAngleLimit || _slopeAngle < -slopeAngleLimit)
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
            if (_tempMovingPlatform)
            {
                _tempMovingPlatform = null;
            }
        }
    }


    // Check if player character collides with the ground
    // Previous version with 3 manual raycasts - deprecated
    /* private void CheckGrounded()
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
    } */

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
            if(groundType == GroundType.MovingPlatform)
            {
                if (!_tempMovingPlatform)
                {
                    _tempMovingPlatform = collider.transform;
                }
            }
            return groundEffector.groundType;
        }
        else
        {
            return GroundType.LevelGeometry;
        }
    }

    private WallType DetermineWallType(Collider2D collider)
    {
        if (collider.GetComponent<WallEffector>())
        {
            WallEffector wallEffector = collider.GetComponent<WallEffector>();
            return wallEffector.wallType;
        }
        else
        {
            return WallType.Normal;
        }
    }

    private Vector2 MovingPlatformAdjust()
    {
        if(_tempMovingPlatform && groundType == GroundType.MovingPlatform)
        {
            _movingPlatformVelocity = _tempMovingPlatform.GetComponent<MovingPlatform>().difference;
            return _movingPlatformVelocity
        }
        else
        {
            return Vector2.zero;
        }
    }

}