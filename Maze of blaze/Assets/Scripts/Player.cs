using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player class handles everything relatedto the player
/// </summary>
public class Player : MonoBehaviour
{
    [HideInInspector]
    public Animator animator;

    [HideInInspector]
    public CharacterController playerController;

    [HideInInspector]
    public Health myHealth;
   // public PlayerData playerData;

    public float speed = 5f;
    public float damage = 1f;

    bool powerPill = false;
    void Start()
    {
        if (playerController == null)
            playerController = this.GetComponent<CharacterController>();
        if (myHealth == null)
            myHealth = this.GetComponent<Health>();
        if (animator == null)
            animator = this.GetComponent<Animator>();
        InputManager.instance.movementCallback += Move;
    }

    public delegate void MoveEvent(Vector3 position);

    public event MoveEvent positionChangeCallback;

    public delegate void HealthChangeEvent();

    public event HealthChangeEvent healthChangeCallback;
    private void Move(Vector2 movementDirection)
    {
        playerController.Move(new Vector3(movementDirection.x, 0, movementDirection.y) * Time.deltaTime * speed);
        if (positionChangeCallback != null)
            positionChangeCallback(transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            if (powerPill)
            {
                other.gameObject.GetComponent<Health>().DealDamage(damage);
            }
            else
            {
                if (enemy.dealsDamage)
                {
                    myHealth.DealDamage(enemy.damage);
                    enemy.Retreat();
                    if (healthChangeCallback != null)
                        healthChangeCallback();
                } else
                {
                    other.gameObject.GetComponent<Health>().DealDamage(damage);
                }
            }
        } else
            if (other.TryGetComponent<Bonus>(out Bonus bonus))
            {
                switch (bonus.type)
                {
                    case Bonus.BonusType.POWERPILL:
                        StopCoroutine("PowerPill");
                        StartCoroutine(PowerPill(bonus.powerPillDuration));
                        break;
                }
                Destroy(other.gameObject);
            }
    }

    IEnumerator PowerPill(float wait)
    {
        powerPill = true;
        animator.SetTrigger("PowerPillStart");
        GameManager.instance.enemyManager.AllRetreat(wait);
        yield return new WaitForSeconds(wait - 1f);//hardcoded numbers are a bad practise
        animator.SetTrigger("PowerPillFadeStart");
        yield return new WaitForSeconds(1f);
        powerPill = false;
    }
}
