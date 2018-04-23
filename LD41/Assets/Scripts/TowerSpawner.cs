using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TowerSpawner : MonoBehaviour {

    public GameObject towerPrefab;
    public Transform raycastPoint;
    public LayerMask terrainMask;

    public DifficultyIncrease difficulty;

    public float cooldownTime;
    public float incrementChanceRate;
    public float spawnUpdateRate;
    public float defaultSpawnChanceRate;

    private float lastSpawnUpdate;
    private float lastSpawnTime;

    private float spawnChanceRate;

    void Start () {
        SpawnTower();
    }
		
	void Update () {
        if (Time.time - lastSpawnUpdate > spawnUpdateRate / difficulty.towerSpawnMultiplier) {
            lastSpawnUpdate = Time.time;

            if (Time.time - lastSpawnTime > cooldownTime) {

                if (Random.value < spawnChanceRate) {
                    spawnChanceRate = defaultSpawnChanceRate;
                    lastSpawnTime = Time.time;

                    SpawnTower();

                } else {
                    spawnChanceRate += incrementChanceRate;
                }

            }
        }	
	}

    void SpawnTower () {        
        RaycastHit hit;
        for (int i = 0; i < 100; i++) {
            Vector3 randomPlacement = transform.position + (Random.insideUnitSphere * 50f);
            NavMeshHit hitNav;
            NavMesh.SamplePosition(randomPlacement, out hitNav, 50f, NavMesh.AllAreas);
            Vector3 finalPos = hitNav.position;
            finalPos.y += 100f;
            raycastPoint.transform.position = finalPos;
            Vector3 ray = raycastPoint.TransformDirection(Vector3.down) * 100;
            if (Physics.Raycast(raycastPoint.position, ray, out hit, 100, terrainMask)) {
                Vector3 terrainPoint = hit.point;

                Instantiate(towerPrefab, terrainPoint, Quaternion.identity);

                return;
            }
        }
    }
}
