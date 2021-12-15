using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script responsible for dealing damage to the player
public class PlayerDamageDealer : MonoBehaviour
{

    public int damageOnCollision = -10; // how much damage an object deals
    public float raycastDistance = 0.2f; // raycast collision detection range
    public LayerMask layerMask; // filtering out unnecessary layers

    // basic object collision types detected
    public bool aboveCollisionDetected;
    public bool belowCollisionDetected;
    public bool leftCollisionDetected;
    public bool rightCollisionDetected;

    // damaging object's default collider type
    private BoxCollider2D _boxCollider;

    // start is called before the first frame update
    void Start()
    {
        _boxCollider = gameObject.GetComponent<BoxCollider2D>();
    }

    // deal damage on each collision with the player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // resetting the flags
        aboveCollisionDetected = false;
        belowCollisionDetected = false;
        leftCollisionDetected = false;
        rightCollisionDetected = false;

        // raycast from above detection
        RaycastHit2D aboveHit = Physics2D.CapsuleCast(_boxCollider.bounds.center, _boxCollider.size,
            CapsuleDirection2D.Vertical, 0f, Vector2.up, raycastDistance, layerMask);

        // raycast from below detection
        RaycastHit2D belowHit = Physics2D.CapsuleCast(_boxCollider.bounds.center, _boxCollider.size,
            CapsuleDirection2D.Vertical, 0f, Vector2.down, raycastDistance, layerMask);

        // raycast on the left detection
        RaycastHit2D leftHit = Physics2D.BoxCast(_boxCollider.bounds.center, _boxCollider.size * 0.75f, 0f, Vector2.left, 
            raycastDistance * 2, layerMask);

        // raycast on the right detection
        RaycastHit2D rightHit = Physics2D.BoxCast(_boxCollider.bounds.center, _boxCollider.size * 0.75f, 0f, Vector2.right, 
            raycastDistance * 2, layerMask);

        // above collision detected
        if (aboveHit.collider)
        {
            aboveCollisionDetected = true;
            PlayerStatus playerStatus = collision.gameObject.GetComponent<PlayerStatus>();
            playerStatus.AdjustPlayerHealth(damageOnCollision); // deal damage to the player
        }
        // below collision detected
        if (belowHit.collider)
        {
            belowCollisionDetected = true;
            PlayerStatus playerStatus = collision.gameObject.GetComponent<PlayerStatus>();
            playerStatus.AdjustPlayerHealth(damageOnCollision); // deal damage to the player
        }
        // left collision detected
        if (leftHit.collider && leftHit.collider.gameObject.CompareTag("Player"))
        {
            leftCollisionDetected = true;
            PlayerStatus playerStatus = collision.gameObject.GetComponent<PlayerStatus>();
            playerStatus.AdjustPlayerHealth(damageOnCollision); // deal damage to the player
        }
        // right collision detected
        if (rightHit.collider)
        {
            rightCollisionDetected = true;
            PlayerStatus playerStatus = collision.gameObject.GetComponent<PlayerStatus>();
            playerStatus.AdjustPlayerHealth(damageOnCollision); // deal damage to the player
        }
    }
}
