using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Tower : MonoBehaviour {

    public Transform canvas;
    public GameObject towerBarsPrefab;
    public Transform towerBarsPosition;

    public GameObject unitPrefab;
    public GameObject navMeshUnitPrefab;

    public LineRenderer laser;
    public Transform laserStart;

    public UnitSelection unitSelection;

    public SpriteRenderer icon;

    public int numUnitsOnDeath;

    public float maxHealth;
    public float health;

    public float maxTime;
    public float time;
    public float attackRadius;
    public float attackPower;
    public float blinkSpeed;

    private TowerBars towerBars;
    private float startTime;

    private Soldier currentTarget;

    private bool isAlive;

    private bool hasPlayedSFX;

    void Start () {        
        unitSelection = FindObjectOfType<UnitSelection>();
        maxTime = maxTime / unitSelection.difficulty.towerTimeMultiplier;
        canvas = unitSelection.hudCanvas.transform;
        towerBars = Instantiate(towerBarsPrefab).GetComponent<TowerBars> ();
        towerBars.canvasRect = canvas.GetComponent<RectTransform>();
        towerBars.transform.SetParent(canvas);
        towerBars.maxHealth = maxHealth;
        towerBars.maxTime = maxTime;
        towerBars.entity = towerBarsPosition;
        health = maxHealth;
        time = maxTime;
        startTime = Time.time;
        isAlive = true;
        unitSelection.AddTower(this);
        hasPlayedSFX = false;
    }

    void Update() {
        if (!isAlive) return;

        time = maxTime - (Time.time - startTime);
        towerBars.time = time;
        towerBars.health = health;

        if (time < maxTime / 2.0f) {
            bool show = (int)((Time.time - startTime) * blinkSpeed) % 2 == 0;
            icon.enabled = show;

            if (!hasPlayedSFX) {
                hasPlayedSFX = true;
                unitSelection.sfxManager.PlaySound(unitSelection.sfxManager.halfwaySFX);
            }

        } else {
            icon.enabled = true;
        }

        if (time <= 0) {
            unitSelection.GameOver();
            isAlive = false;
        }

        AquireTarget();

        if (currentTarget) {
            laser.enabled = true;
            laser.positionCount = 2;
            laser.SetPositions(new Vector3[] {laserStart.position, currentTarget.transform.position});
            currentTarget.Attack(attackPower * Time.deltaTime);
        } else {
            laser.enabled = false;
        }

        if (health <= 0) {
            isAlive = false;
            icon.enabled = true;
            OnDeath();
            Destroy(towerBars.gameObject);
            Destroy(this.gameObject);
        }
    }

    void OnDeath () {
        unitSelection.RemoveTower(this);
        unitSelection.towersDestroyedCount++;
        for (int i = 0; i < numUnitsOnDeath; i++) {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * 4f;
            randomPos.y = transform.position.y;
            Vector3 navMeshUnitPosition = randomPos;
            navMeshUnitPosition.y = 0.75f;
            NavMeshAgent agent = Instantiate(navMeshUnitPrefab, navMeshUnitPosition, Quaternion.identity).GetComponent<NavMeshAgent>();
            FollowNavAgent follower = Instantiate(unitPrefab, randomPos, Quaternion.identity).GetComponent<FollowNavAgent>();
            follower.agent = agent;
            follower.raycastPoint = agent.transform.GetChild(0);
        }
    }

    void AquireTarget() {
        if (currentTarget) {
            if (Vector3.Distance(currentTarget.transform.position, transform.position) > attackRadius) currentTarget = null;
        }

        if (!currentTarget) {
            Collider[] colliders = Physics.OverlapSphere(transform.position, attackRadius);
            foreach (Collider col in colliders) {
                if (col.CompareTag("unit")) {
                    currentTarget = col.GetComponent<Soldier>();
                    return;
                }
            }
        }
    }

    public void Attack (float dmg) {
        health -= dmg;
    }
}
