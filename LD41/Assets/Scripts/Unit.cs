using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    public Spot spot;
    public Formation formation;
    public bool isSelected;

    public GameObject selectionRing;

    void Update () {
        selectionRing.SetActive(isSelected);
    }
}
