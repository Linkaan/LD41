using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeMenu : MonoBehaviour {

    public UnitSelection selector;

    public Text timeLeft;
    public Text timeElapsed;

    void FixedUpdate () {
        timeElapsed.text = "TIME ELAPSED: " + (int)(Time.time - selector.startTime);
        float theTimeLeft = selector.MinTowerTime();
        if (theTimeLeft > 1000) {
            timeLeft.text = "NO TOWER";
        } else {
            timeLeft.text = "TIME LEFT: " + (int)(theTimeLeft);
        }
    }
}
