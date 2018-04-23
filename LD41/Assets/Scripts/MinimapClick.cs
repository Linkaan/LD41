using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapClick : MonoBehaviour {

    public Camera minimapCamera;
    public LayerMask terrainMask;

    public UnitSelection selection;

    void Update () {
        if (Input.GetMouseButton(0) && !selection.isSelectionBoxActive) {
            RaycastHit hit;
            Ray ray = minimapCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100, terrainMask)) {
                Vector3 terrainPoint = hit.point;
                terrainPoint.y = Camera.main.transform.position.y;
                Camera.main.transform.position = terrainPoint;
            }
        }
    }

}
