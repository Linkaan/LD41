using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyIncrease : MonoBehaviour {

    public UnitSelection selection;

    public float spawnMultiplierIncrease;
    public float towerMultiplierIncrease;

    public float maxSpawnMultiplier;
    public float maxTimeMultiplier;

    public float towerSpawnMultiplier;
    public float towerTimeMultiplier;

    private float startTime;

    void Start () {
        startTime = Time.time;
    }

    void FixedUpdate () {
        if (selection.towersDestroyedCount == 0 && Time.time - startTime < 30) return;

        if (towerSpawnMultiplier < maxSpawnMultiplier) {
            towerSpawnMultiplier += spawnMultiplierIncrease;
        }

        if (towerTimeMultiplier < maxTimeMultiplier) {
            towerTimeMultiplier += towerMultiplierIncrease;
        }
    }
}
