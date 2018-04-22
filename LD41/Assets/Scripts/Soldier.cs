using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Commands {
    None,
    Goto,
    Attack
}

public class Soldier : MonoBehaviour {

    public Transform canvas;
    public GameObject healthBarPrefab;
    public Transform healthBarPosition;

    public UnitSelection unitSelection;

    public Commands order;
    public Animator animator;

    public float maxHealth;
    public float health;
    public float attackRadius;
    public float attackPower;

    public bool isAlive;

    private HealthBar healthBar;
    private Unit unit;

    private Transform currentTowerTarget;

    void Start () {
        unit = GetComponent<Unit>();
        unitSelection = FindObjectOfType<UnitSelection>();
        canvas = unitSelection.hudCanvas.transform;
        healthBar = Instantiate(healthBarPrefab).GetComponent<HealthBar> ();
        healthBar.canvasRect = canvas.GetComponent<RectTransform>();
        healthBar.transform.SetParent(canvas);
        healthBar.maxHealth = maxHealth;
        healthBar.entity = healthBarPosition;
        healthBar.gameObject.SetActive(unit.isSelected);
        health = maxHealth;
        isAlive = true;
    }

    void Update() {
        if (!isAlive) return;
        healthBar.gameObject.SetActive(unit.isSelected);
        healthBar.health = health;

        if (unit.formation) {
            currentTowerTarget = unit.formation.currentTowerTarget;
        }

        if (currentTowerTarget && isNearTower()) {
            order = Commands.Attack;
            currentTowerTarget.GetComponent<Tower> ().Attack(attackPower * Time.deltaTime);
        } else if (order == Commands.Attack) {
            order = Commands.None;
        }

        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);
        switch (order) {
            case Commands.None:                
                break;
            case Commands.Goto:
                animator.SetBool("isWalking", true);
                break;
            case Commands.Attack:
                animator.SetBool("isAttacking", true);
                break;
        }

        if (health <= 0) {
            // TODO play death animation
            unitSelection.unitsDeadCount++;
            isAlive = false;
            Destroy(healthBar.gameObject);
            Destroy(GetComponent<FollowNavAgent>().agent.gameObject);
            if (unit.formation) {
                unit.formation.RemoveUnit(unit);
            }
            unitSelection.DeselectUnit(unit);
            Destroy(this.gameObject);
        }
    }

    void FixedUpdate () {
        Vector3 targetPos = GetComponent<FollowNavAgent> ().agent.destination;
        if (unit.formation) {
            targetPos = unit.formation.targetPoint;
        }
        Vector3 moveDirection = targetPos - transform.position;
        if (moveDirection != Vector3.zero) {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            Debug.Log(angle);
            //animator.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            //Vector3 angles = animator.transform.eulerAngles;
            //angles.x = 45;
            //animator.transform.eulerAngles = angles;
            animator.GetComponent<SpriteRenderer>().flipX = angle > 0;
        }
    }

    bool isNearTower () {
        Collider[] colliders = Physics.OverlapSphere(transform.position, attackRadius);
        foreach (Collider col in colliders) {
            if (col.CompareTag("tower")) return true;
        }
        return false;
    }

    public void Attack(float dmg) {
        health -= dmg;
    }

    public void SetTowerTarget(Transform tower) {
        currentTowerTarget = tower;
    }
}
