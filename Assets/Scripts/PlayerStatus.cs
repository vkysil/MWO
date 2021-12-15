using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// script managing player status interactions
public class PlayerStatus : MonoBehaviour
{

    public int currentHealth; // current player health
    public int maxHealth = 100; // max player health
    public int killScore = 0; // current kill score
    public int experience = 0; // current experience points
    public bool isPlayerDead = false; // player death flag
    public bool isInvulnerable = false; // invulnerability period after damage

    private PlayerController _playerController; // default player controller
    private CapsuleCollider2D _capsuleCollider; // default player collider
    private Scene scene; // current scene, used for restarting the level


    // start is called before the first frame update
    void Start()
    {
        // initialize the necessary values
        _playerController = gameObject.GetComponent<PlayerController>();
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
        scene = SceneManager.GetActiveScene();
        currentHealth = maxHealth;
    }

    // player health adjustment logic
    public void AdjustPlayerHealth(int value)
    {
        // player takes damage
        if (value < 0) 
        {
            // no damage during invulnerability period
            if (!isInvulnerable) 
            {
                currentHealth += value;
            }
            // health reaches zero, player dies
            if (currentHealth <= 0) 
            {
                if (!isPlayerDead)
                {
                    isPlayerDead = true;
                    PlayerDeath();
                }
            }
            // player still has health remaining
            else
            {
                // start invulnerability period
                StartCoroutine("Invulnerable");
            }
        }
        // player gets healed
        else if (value > 0)
        {
            currentHealth += value;
        }
        // health can't go below zero
        if(currentHealth < 0)
        {
            currentHealth = 0;
        }
        // health can't go above maximum
        else if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    // player kill score adjustment logic
    public void AdjustKillScore()
    {
        // To-Do
    }

    // player experience adjustment logic
    public void AdjustExperience()
    {
        // To-Do
    }

    // player death
    public void PlayerDeath()
    {
        // disable player sprites
        SkinnedMeshRenderer[] sprites = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach(SkinnedMeshRenderer sprite in sprites){
            sprite.enabled = false;
        }
        // disable player controls
        _playerController.enabled = false;
        _capsuleCollider.enabled = false;

        // restart the level
        StartCoroutine("RestartCurrentLevel");
    }

    // invulnerability period timer
    private IEnumerator Invulnerable()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(0.5f);
        isInvulnerable = false;
    }

    // restarting the current level
    private IEnumerator RestartCurrentLevel()
    {
        // To-Do: display death window pop-up
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(scene.name);
        isPlayerDead = false;
        currentHealth = maxHealth;
    }
}
