using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

// controller for all character movements
public class CharacterController2D : MonoBehaviour
{
    // collision detection and movement
    public float raycastDistance = 0.2f; // distance for raycasts used to detect ground collision
    public float downForceAdjustment = 1.2f; // additional down force for more fluid slope movement
    public float slopeAngleLimit = 45; // maximum slope angle characters can move over
    public LayerMask layerMask; // filtering out unnecessary layers

    // collision flags
    public bool below; // true if something is below the player
    public bool left; // true if something is to the left side of the player
    public bool right; // true if something is to the right side of the player
    public bool above; // true if something is above the player
    public bool hitGroundThisFrame; // TRUE if character collided with the ground in the current frame

    // ground and wall types
    public GroundType groundType;
    public GroundType ceilingType;
    public WallType rightWallType;
    public WallType leftWallType;

    // movement variables
    private Vector2 _moveAmount;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;

    // jump pad variables
    public float jumpPadAmount; 
    public float jumpPadUpperLimit;

    private Rigidbody2D _rigidbody; // default rigidbody
    private CapsuleCollider2D _capsuleCollider; // default collider
    private Vector2[] _raycastPosition = new Vector2[3]; // raycast origin
    private RaycastHit2D[] _raycastHits = new RaycastHit2D[3]; // information about raycast colliders
    private bool _disabledGroundCheck; // disable ground check while beginning a jump
    private Vector2 _slopeNormal; // normal perpendicular to the slope
    private float _slopeAngle; // angle of the slope
    private bool _inAirLastFrame; // true if character wasn't colliding with the ground last frame
    private Transform _tempMovingPlatform; // used to keep a character standing on the moving platform
    private Vector2 _movingPlatformVelocity; // velocity of the moving platform

    // start method
    void Start()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
    }

    // update for the physics engine
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
        // check left collision
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

        // check right collision
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

        // check above collision
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

    // movement update method adjusted to framerate
    public void Move(Vector2 movement)
    {
        _moveAmount += movement;
    }

    // check if player character collides with the ground
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

            // jump pad logic
            if(groundType == GroundType.JumpPad)
            {
                JumpPad jumpPad = hit.collider.GetComponent<JumpPad>();
                jumpPadAmount = jumpPad.jumpPadAmount;
                jumpPadUpperLimit = jumpPad.jumpPadUpperLimit;
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

    // visible raycasts for debugging purposes
    private void DrawDebugRays(Vector2 direction, Color color)
    {
        for (int i = 0; i < _raycastPosition.Length; i++ )
        {
            Debug.DrawRay(_raycastPosition[i], direction * raycastDistance, color);
        }
    }

    // ground check disable
    public void DisableGroundCheck()
    {
        below = false;
        _disabledGroundCheck = true;
        StartCoroutine("EnableGroundCheck");
    }

    // ground check enable
    IEnumerator EnableGroundCheck()
    {
        yield return new WaitForSeconds(0.1f);
        _disabledGroundCheck = false;
    }

    // determining the below collider ground type
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

    // determining the side collider wall type
    private WallType DetermineWallType(Collider2D collider)
    {
        if (collider.GetComponent<WallEffector>())
        {
            WallEffector wallEffector = collider.GetComponent<WallEffector>();
            return wallEffector.wallType;
        }
        else
        {
            return WallType.LevelGeometry;
        }
    }

    // moving platform character movement adjustment
    private Vector2 MovingPlatformAdjust()
    {
        if(_tempMovingPlatform && groundType == GroundType.MovingPlatform)
        {
            _movingPlatformVelocity = _tempMovingPlatform.GetComponent<MovingPlatform>().frameDifference;
            return _movingPlatformVelocity;
        }
        else
        {
            return Vector2.zero;
        }
    }

}