using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour {

    public UnitSelection selector;
    public GameObject spawner;

    public Text gameOverText;

    public void onGameOver() {
        float timeAlive = Time.time - selector.startTime;
        gameOverText.text = timeAlive + " seconds alive\n" + selector.towersDestroyedCount + " towers destroyed\n" + selector.unitsDeadCount + " units dead";
        FindObjectOfType<ScoreManager>().LoadLeaderboard();
    }

    public void Retry() {
        SceneManager.LoadScene(1);
    }

    public void Menu() {
        SceneManager.LoadScene(0);
    }

}
