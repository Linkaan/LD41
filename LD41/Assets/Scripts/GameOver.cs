using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour {

    public UnitSelection selector;
    public GameObject spawner;

    public Text gameOverText;

    private float startTime;

    void Start () {
        startTime = Time.time;
    }

    public void onGameOver() {
        float timeAlive = Time.time - startTime;
        gameOverText.text = timeAlive + " seconds alive\n" + selector.towersDestroyedCount + " towers destroyed\n" + selector.unitsDeadCount + " units dead";
    }

    public void Retry() {
        SceneManager.LoadScene(1);
    }

    public void Menu() {
        SceneManager.LoadScene(0);
    }

}
