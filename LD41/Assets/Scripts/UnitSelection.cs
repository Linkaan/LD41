using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitSelection : MonoBehaviour {

    public GameObject formationPrefab;
    public GameObject unitPrefab;
    public GameObject navMeshUnitPrefab;

    public GameObject hudCanvas;
    public GameObject gameOverCanvas;

    public Camera minimapCamera;

    public DifficultyIncrease difficulty;

    public SoundManager sfxManager;

    public Transform[] spawnSpots;
    public LayerMask terrainMask;
    public LayerMask minimapMask;
    public LayerMask unitMask;

    public int numInitialUnits;

    public int towersDestroyedCount;
    public int unitsDeadCount;

    public float startTime;
    public bool isSelectionBoxActive;

    private List<Unit> units;
    private List<Unit> unitsToDeselect;

    private List<Tower> allTowers;

    private Vector3 selectionStart;
    private Vector3 selectionEnd;

    private bool isSelectionActive;    

    private Transform currentTowerTarget;

    public bool gameOver;
    public float timeAlive;
    public int unitsAlive;

    void Start () {
        startTime = Time.time;
        units = new List<Unit>();
        allTowers = new List<Tower>();
        for (int i = 0; i < numInitialUnits; i++) {
            CreateUnitAtSpawnSpot();
        }
    }

    void Update () {
        if (gameOver) return;
        timeAlive = Time.time - startTime;
        CancelSelection ();
        RaycastHit hit;

        /*
        if (isSelectionActive && Input.GetMouseButtonDown(0)) {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100)) {
                if (hit.collider.CompareTag("tower")) {
                    SelectUnitOrTarget();
                    return;
                }
            }
        }
        */

        CreateSelectionBox ();

        // consider drawing selection box
        if (isSelectionBoxActive) {
            if (units.Count > 0/* && Vector3.Distance(selectionStart, Input.mousePosition) >= 1.0f*/) {
                foreach (Unit unit in units) unit.isSelected = false;
                units.Clear();
            }

            List<Unit> selectedUnits = GetUnitsInSelectionBox(selectionStart, Input.mousePosition);

            foreach (Unit unit in selectedUnits) unit.isSelected = true;

            unitsToDeselect = selectedUnits;
        }

        if (Input.GetMouseButtonUp (0)) {            
            CreateSelection();
        } else if (Input.GetMouseButtonDown (1)) {
            unitsToDeselect = null;
            SelectUnitOrTarget();
        }
    }

    void FixedUpdate () {
        if (gameOver) return;
        Unit[] unitsAlive = FindObjectsOfType<Unit>();
        if (unitsAlive.Length == 0) {
            GameOver();
        }
    }

    void CreateSelection () {    
        RaycastHit hit;
        if (!isSelectionBoxActive && Physics.Raycast(minimapCamera.ScreenPointToRay(Input.mousePosition), out hit, 100, minimapMask)) {
            return;
        }

        selectionEnd = Input.mousePosition;
        isSelectionBoxActive = false;
        unitsToDeselect = null;

        List<Unit> selectedUnits = GetUnitsInSelectionBox(selectionStart, selectionEnd);

        foreach (Unit unit in selectedUnits) {
            SelectUnit(unit);
        }
    }

    void SelectUnitOrTarget () {
        RaycastHit hit;

        if (units != null && units.Count == 0) return;

        if (Physics.Raycast(minimapCamera.ScreenPointToRay(Input.mousePosition), out hit, 100, minimapMask) ||
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, unitMask)) {           

            if (isSelectionActive) {
                Vector3 targetPoint = hit.point;
                if (hit.collider.CompareTag("tower")) {
                    Tower tower = hit.collider.GetComponentInParent<Tower>();
                    targetPoint = tower.transform.position;
                    targetPoint.x += 4f;
                    targetPoint.z -= 1.5f;
                    currentTowerTarget = tower.transform;
                    sfxManager.PlaySound(sfxManager.attackSFX);
                } else {
                    sfxManager.PlaySound(sfxManager.gotoSFX);
                }
                SelectTarget(targetPoint);
                currentTowerTarget = null;
            }
        }
    }

    void SelectUnit (Unit unit) {
        if (!isSelectionActive) {
            foreach (Unit toDeselect in units) {
                toDeselect.isSelected = false;
            }
            units.Clear();
            isSelectionActive = true;
        }

        unit.isSelected = true;
        if (!units.Contains (unit)) {            
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
            if (isSame && formation.GetUnitCount () == units.Count) {
                formation.SetTargetPoint(point);
                formation.SetTowerTarget(currentTowerTarget);
            } else {
                foreach (Unit unit in units) {
                    unit.GetComponent<FollowNavAgent> ().agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

                    if (unit.formation) {
                        unit.formation.RemoveUnit(unit);
                    }
                }
                CreateFormation(point);
                units[0].formation.SetTargetPoint(point);
                units[0].formation.SetTowerTarget(currentTowerTarget);
            }
        } else if (units.Count == 1) {
            Unit unit = units[0];

            if (unit.formation/* && unit.formation.leader != unit*/) {
                unit.formation.RemoveUnit(unit);
            }

            unit.GetComponent<FollowNavAgent> ().SetDestination (point);
            unit.GetComponent<Soldier> ().SetTowerTarget(currentTowerTarget);
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
        formation.unitSelection = this;
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
        RaycastHit hit;
        if (Physics.Raycast(minimapCamera.ScreenPointToRay(Input.mousePosition), out hit, 100, minimapMask)) {
            return;
        }

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

        Unit unitStart = GetUnitMouse(selectionStart);
        if (unitStart) {
            selectedUnits.Add(unitStart);
        }

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

        Unit unitEnd = GetUnitMouse(selectionEnd);
        if (unitEnd) {
            selectedUnits.Add(unitEnd);
        }

        return selectedUnits;
    }

    Unit GetUnitMouse(Vector3 selection) {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(selection);
        if (Physics.Raycast(ray, out hit, 100, unitMask)) {
            if (hit.collider.CompareTag("unit")) {
                return hit.collider.GetComponent<Unit> ();
            }
        }
        return null;
    }

    void CreateUnit () {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, unitMask)) {
            Vector3 navMeshUnitPosition = hit.point;
            navMeshUnitPosition.y = 0.75f;
            NavMeshAgent agent = Instantiate (navMeshUnitPrefab, navMeshUnitPosition, Quaternion.identity).GetComponent<NavMeshAgent> ();
            FollowNavAgent follower = Instantiate(unitPrefab, hit.point, Quaternion.identity).GetComponent<FollowNavAgent> ();
            follower.agent = agent;
            follower.raycastPoint = agent.transform.GetChild(0);
        }
    }

    void CreateUnitAtSpawnSpot () {
        RaycastHit hit;
        Transform raycastPoint = spawnSpots[Random.Range(0, spawnSpots.Length)];
        Vector3 ray = raycastPoint.TransformDirection(Vector3.down) * 100;
        if (Physics.Raycast(raycastPoint.position, ray, out hit, 100, terrainMask)) {
            Vector3 randomPos = hit.point + Random.insideUnitSphere * 4f;
            randomPos.y = hit.point.y;
            Vector3 navMeshUnitPosition = randomPos;
            navMeshUnitPosition.y = 0.75f;
            NavMeshAgent agent = Instantiate(navMeshUnitPrefab, navMeshUnitPosition, Quaternion.identity).GetComponent<NavMeshAgent>();
            FollowNavAgent follower = Instantiate(unitPrefab, randomPos, Quaternion.identity).GetComponent<FollowNavAgent>();
            follower.agent = agent;
            follower.raycastPoint = agent.transform.GetChild(0);
        }
    }

    public void DeselectUnit(Unit unit) {
        if (units.Contains(unit)) {
            units.Remove(unit);
        }

        if (unitsToDeselect != null && unitsToDeselect.Contains(unit)) {
            unitsToDeselect.Remove(unit);
        }
    }

    public void GameOver () {
        if (gameOver) return;
        gameOver = true;

        sfxManager.PlaySound(sfxManager.loseSFX);

        FindObjectOfType<TowerSpawner>().enabled = false;

        //hudCanvas.SetActive(false);
        Tower[] towers = FindObjectsOfType<Tower>();

        if (unitsToDeselect != null) {
            foreach (Unit unit in unitsToDeselect) unit.isSelected = false;
        }

        if (units != null) {
            foreach (Unit unit in units) unit.isSelected = false;
        }

        foreach (Tower tower in towers) {
            Destroy(tower.GetComponent<LineRenderer>());
            Destroy(tower);
        }

        foreach (Unit unit in FindObjectsOfType<Unit>()) {
            unitsAlive++;
            Destroy(unit.GetComponent<FollowNavAgent> ());
            Destroy(unit.GetComponent<Soldier>());
            if (unit.formation) {
                unit.formation.BreakFormation();
            }
            Destroy(unit);
        }

        //Camera.main.GetComponent<TopdownCamera>().enabled = false;
        gameOverCanvas.SetActive(true);
        gameOverCanvas.GetComponent<GameOver>().onGameOver();
    }

    public void AddTower (Tower tower) {
        sfxManager.PlaySound(sfxManager.towerUpSFX);
        allTowers.Add(tower);
    }

    public void RemoveTower (Tower tower) {
        sfxManager.PlaySound(sfxManager.towerDownSFX);
        allTowers.Remove(tower);
    }

    public float MinTowerTime () {
        float minTime = float.PositiveInfinity;
        foreach (Tower tower in allTowers) {
            if (tower.time < minTime) {
                minTime = tower.time;
            }
        }
        return minTime;
    }

    void ToggleFullscreen() {
        Screen.fullScreen = !Screen.fullScreen;
    }

}
