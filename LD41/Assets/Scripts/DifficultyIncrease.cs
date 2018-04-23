using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyIncrease : MonoBehaviour {

    public float spawnMultiplierIncrease;
    public float towerMultiplierIncrease;

    public float maxSpawnMultiplier;
    public float maxTimeMultiplier;

    public float towerSpawnMultiplier;
    public float towerTimeMultiplier;

    void FixedUpdate () {
        if (towerSpawnMultiplier < maxSpawnMultiplier) {
            towerSpawnMultiplier += spawnMultiplierIncrease;
        }

        if (towerTimeMultiplier < maxTimeMultiplier) {
            towerTimeMultiplier += towerMultiplierIncrease;
        }
    }
}
