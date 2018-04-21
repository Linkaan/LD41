using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitSelection : MonoBehaviour {

    public GameObject formationPrefab;
    public GameObject unitPrefab;
    public GameObject navMeshUnitPrefab;

    private List<Unit> units;
    private List<Unit> unitsToDeselect;

    private Vector3 selectionStart;
    private Vector3 selectionEnd;

    private bool isSelectionActive;
    private bool isSelectionBoxActive;

    void Start () {
        units = new List<Unit>();
    }

    void Update () {
        CancelSelection ();

        CreateSelectionBox ();

        // consider drawing selection box
        if (isSelectionBoxActive) {
            foreach (Unit unit in GetUnitsInSelectionBox(selectionStart, Input.mousePosition)) unit.isSelected = !units.Contains(unit);
        }

        if (Input.GetMouseButtonUp (0)) {
            CreateSelection ();
        } else if (Input.GetMouseButtonDown (1)) {
            CreateUnit ();
        }
    }

    void CreateSelection () {
        selectionEnd = Input.mousePosition;
        isSelectionBoxActive = false;

        if (Vector3.Distance (selectionStart, selectionEnd) < 1.0f) {
            SelectUnitOrTarget();
        } else {
            List<Unit> selectedUnits = GetUnitsInSelectionBox(selectionStart, selectionEnd);

            foreach (Unit unit in selectedUnits) {
                SelectUnit(unit);
            }
        }
    }

    void SelectUnitOrTarget () {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100)) {
            if (hit.collider.CompareTag("unit")) {
                SelectUnit(hit.collider.GetComponent<Unit> ());
            } else if (isSelectionActive) {
                SelectTarget(hit.point);
            }
        }
    }

    void SelectUnit (Unit unit) {
        if (!isSelectionActive) {
            units.Clear();
            isSelectionActive = true;
        }

        if (units.Contains (unit)) {
            units.Remove (unit);
        } else {
            units.Add(unit);
        }
    }

    void SelectTarget (Vector3 point) {
        isSelectionActive = true;

        if (units.Count > 1) {
            Formation formation = units[0].formation;
            bool isSame = false;

            if (formation != null) {
                isSame = true;
                foreach (Unit unit in units) {
                    if (formation != unit.formation) {
                        isSame = false;
                        break;
                    }
                }
            }
            if (isSame) {
                formation.leader.GetComponent<FollowNavAgent>().SetDestination(point);
            } else {
                foreach (Unit unit in units) {
                    unit.GetComponent<FollowNavAgent> ().agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

                    if (unit.formation && unit.formation.leader == unit) {
                        unit.formation.BreakFormation();
                    } else if (unit.formation) {
                        unit.formation.RemoveUnit(unit);
                    }
                }
                CreateFormation(point);
            }
        } else if (units.Count == 1) {
            Unit unit = units[0];

            if (unit.formation/* && unit.formation.leader != unit*/) {
                unit.formation.RemoveUnit(unit);
            }

            unit.GetComponent<FollowNavAgent> ().SetDestination (point);
        }
    }

    Unit GetClosestUnit(Vector3 position) {
        float minDistance = Mathf.Infinity;
        Unit closestUnit = units[0];
        foreach (Unit unit in units) {
            float distance = Vector3.Distance(position, unit.transform.position);
            if (distance < minDistance) {
                minDistance = distance;
                closestUnit = unit;
            }
        }

        return closestUnit;
    }

    void CreateFormation (Vector3 target) {
        Unit closestUnit = GetClosestUnit(target);

        Debug.Assert(closestUnit.formation == null);

        // easy way to make AI not look like a complete idiot
        closestUnit.GetComponent<FollowNavAgent> ().agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        Formation formation = Instantiate(formationPrefab, closestUnit.GetComponent<FollowNavAgent>().agent.transform.position, Quaternion.identity).GetComponent<Formation> ();
        formation.transform.SetParent(closestUnit.GetComponent<FollowNavAgent> ().agent.transform);
        formation.SetUnitsAndTarget(closestUnit, units, target);
    }

    void CancelSelection () {
        if (isSelectionActive && Input.GetKeyDown(KeyCode.Escape)) {
            isSelectionActive = false;


            foreach (Unit unit in units) unit.isSelected = false;
            units.Clear ();
        }
    }

    void CreateSelectionBox () {
        if (Input.GetMouseButtonDown (0)) {
            selectionStart = Input.mousePosition;
            isSelectionBoxActive = true;
        }

        if (unitsToDeselect != null) {
            foreach (Unit unit in unitsToDeselect) unit.isSelected = false;
            unitsToDeselect = null;
        }
    }

    List<Unit> GetUnitsInSelectionBox (Vector3 selectionStart, Vector3 selectionEnd) {
        List<Unit> selectedUnits = new List<Unit>();

        float xMin = Mathf.Min(selectionStart.x, selectionEnd.x);
        float yMin = Mathf.Min(selectionStart.y, selectionEnd.y);
        float width = Mathf.Max(selectionStart.x, selectionEnd.x) - xMin;
        float height = Mathf.Max(selectionStart.y, selectionEnd.y) - yMin;
        Rect selectionRect = new Rect(xMin, yMin, width, height);

        GameObject[] allUnits = GameObject.FindGameObjectsWithTag("unit");
        foreach (GameObject unit in allUnits) {
            if (selectionRect.Contains(Camera.main.WorldToScreenPoint(unit.transform.position))) {
                selectedUnits.Add(unit.GetComponent<Unit> ());
            }
        }

        return selectedUnits;
    }

    void CreateUnit () {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100)) {
            Vector3 navMeshUnitPosition = hit.point;
            navMeshUnitPosition.y = 0.75f;
            NavMeshAgent agent = Instantiate (navMeshUnitPrefab, navMeshUnitPosition, Quaternion.identity).GetComponent<NavMeshAgent> ();
            FollowNavAgent follower = Instantiate(unitPrefab, hit.point, Quaternion.identity).GetComponent<FollowNavAgent> ();
            follower.agent = agent;
            follower.raycastPoint = agent.transform.GetChild(0);
        }
    }

}
