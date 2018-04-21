using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Formation : MonoBehaviour {

    public GameObject spotPrefab;

    public Unit leader;
    public float formationSpeed;
    public float defaultRelativeDistance;
    public int spotUpdateLagThreshold;
    public float formationUpdateRate;

    private float lastFormationUpdate;

    private float relativeDistance;
    private int unitLagCount;

    private float rotationOffset;

    private List<Unit> units;
    private List<Spot> spots;

    void Awake () {
        spots = new List<Spot>();
        units = new List<Unit>();
        relativeDistance = defaultRelativeDistance;
        lastFormationUpdate = -formationUpdateRate;
        rotationOffset = 0;
    }

    void Update () {
        if (Time.time - lastFormationUpdate >= formationUpdateRate) {
            lastFormationUpdate = Time.time;
            FixFormation();
            FixUnitLag();
        }
    }

    void FixFormation () {
        if (!CheckValidSpots()) {
            bool success = ContractFormation();
            /*if (!success) {
                success = RotateFormation();   
            }*/
            Debug.Log("could fix formation: " + success);
        } else {
            TryRevertingFormation();   
        }
    }

    void FixUnitLag () {
        bool isLagging = false;
        foreach (Unit unit in units) {
            if (unit.spot != null) {
                float distance = Vector3.Distance(unit.GetComponent<FollowNavAgent> ().agent.transform.position, unit.spot.transform.position);
                if (distance > relativeDistance) {
                    isLagging = true;
                    break;
                }
            }
        }

        if (!isLagging) {
            unitLagCount = 0;

            foreach (Unit unit in units) {
                unit.GetComponent<FollowNavAgent> ().agent.speed = unit == leader ? formationSpeed : formationSpeed * 2.2f;
            }

            return;
        }

        Debug.Log("formation lag count " + unitLagCount);

        if (unitLagCount++ > spotUpdateLagThreshold) {
            unitLagCount = 0;
            ReassignSpots ();
        }

        foreach (Unit unit in units) {
            if (unit.spot != null) {
                float distance = Vector3.Distance(unit.GetComponent<FollowNavAgent>().agent.transform.position, unit.spot.transform.position);
                float speed = unit == leader ? (distance <= relativeDistance ? formationSpeed / 2 : formationSpeed) : formationSpeed * 2.2f;
                unit.GetComponent<FollowNavAgent> ().agent.speed = speed;
            }
        }
    }

    bool ContractFormation () {
        relativeDistance -= 0.1f;
        if (relativeDistance < 1.1f) {
            relativeDistance = 1.1f;
            return false;
        }

        bool canUpdate = CanUpdateFormation();
        if (canUpdate) UpdateSpots(false);
        return canUpdate;
    }

    bool RotateFormation () {
        rotationOffset += 10f;
        if (rotationOffset > 360f) {
            rotationOffset = 0;
            return false;
        }

        bool canUpdate = CanUpdateFormation();
        if (canUpdate) UpdateSpots(false); // TODO: maybe needs to reassign
        return canUpdate;
    }

    void TryRevertingFormation () {
        if (relativeDistance < defaultRelativeDistance) {
            float originalRelativeDistance = relativeDistance;
            relativeDistance += 0.1f;
            relativeDistance = Mathf.Clamp(relativeDistance, 1.1f, defaultRelativeDistance);
            if (CanUpdateFormation ()) {
                UpdateSpots (false);
            } else {
                relativeDistance = originalRelativeDistance;
            }
        }

        if (Mathf.Abs(rotationOffset - 0.0f) > Mathf.Epsilon) {
            float originalRotation = rotationOffset;
            rotationOffset += 10.0f * (Mathf.Abs(rotationOffset) / rotationOffset);
            rotationOffset = Mathf.Abs(rotationOffset) < 10.0f ? 0.0f : rotationOffset;
            if (CanUpdateFormation ()) {
                UpdateSpots (false);
            } else {
                rotationOffset = originalRotation;
            }
        }
    }

    bool CheckValidSpots () {
        foreach (Spot spot in spots) {
            if (!IsValidPosition(spot.transform.position)) return false;
        }

        return true;
    }

    bool CanUpdateFormation () {
        List<Vector3> spotPositions = CalculateSpotPositions();

        foreach (Vector3 position in spotPositions) {
            if (!IsValidPosition(position)) return false;
        }

        return true;
    }

    bool IsValidPosition (Vector3 position) {
        if (float.IsNaN(position.x) ||
            float.IsNaN(position.y) ||
            float.IsNaN(position.z)) return false;

        NavMeshPath path = new NavMeshPath();
        leader.GetComponent<FollowNavAgent>().agent.CalculatePath(position, path);

        return path.status == NavMeshPathStatus.PathComplete;
    }

    void AddSpots () {
        if (units.Count < 2) return;

        foreach (Spot spot in spots) {
            Destroy (spot.gameObject);
        }
        spots.Clear();

        List<Vector3> spotPositions = CalculateSpotPositions();
        foreach (Vector3 position in spotPositions) AddSpotAtLocation(position);

        AssignSpot (leader, GetClosestSpot (leader.GetComponent<FollowNavAgent>().agent.transform.position));

        ReassignSpots ();
    }

    void AddSpotAtLocation (Vector3 position) {
        Spot newSpot = Instantiate(spotPrefab, position, Quaternion.identity).GetComponent<Spot> ();
        newSpot.formation = this;
        newSpot.occupier = null;
        newSpot.transform.SetParent(this.transform);
        spots.Add(newSpot);
    }

    void UpdateSpots (bool doReassign) {
        List<Vector3> spotPositions = CalculateSpotPositions();
        for (int i = 0; i < spotPositions.Count; i++) {
            spots[i].transform.position = spotPositions[i];
        }
        if (doReassign) ReassignSpots ();
    }

    List<Vector3> CalculateSpotPositions () {
        List<Vector3> spotPositions = new List<Vector3>();

        // assuming box formation
        float sqrtSpotCount = Mathf.Sqrt(units.Count);
        int formationWidth = Mathf.FloorToInt(sqrtSpotCount);
        int formationHeight = Mathf.CeilToInt(sqrtSpotCount);

        if (formationWidth * formationHeight > units.Count) {
            if (formationHeight > 1) {
                formationHeight -= 1;
            } else {
                formationWidth -= 1;
            }
        }

        Vector3 leaderPos = leader.GetComponent<FollowNavAgent>().agent.transform.position;

        for (int x = 0; x < formationWidth; x++) {
            for (int y = 0; y < formationHeight; y++) {
                Vector3 spotPosition = leaderPos;

                spotPosition.x -= x * relativeDistance;
                spotPosition.z -= y * relativeDistance;

                spotPositions.Add(RotateAroundPivot (spotPosition, leaderPos, Vector3.up * rotationOffset));
            }
        }

        int spotsToAdd = units.Count - (formationWidth * formationHeight);
        if (spotsToAdd > 0) {
            for (int x = 0; x < spotsToAdd; x++) {
                Vector3 spotPosition = leaderPos;

                spotPosition.x -= x * relativeDistance;
                spotPosition.z -= formationHeight * relativeDistance;

                spotPositions.Add(RotateAroundPivot(spotPosition, leaderPos, Vector3.up * rotationOffset));
            }
        }

        Debug.Assert(spotPositions.Count == units.Count);

        return spotPositions;
    }

    Vector3 RotateAroundPivot (Vector3 point, Vector3 pivot, Vector3 angles) {
        if (Mathf.Abs(rotationOffset) - 0.0f < Mathf.Epsilon) return point;
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    Spot GetClosestSpot(Vector3 position) {
        float minDistance = Mathf.Infinity;
        Spot closestSpot = spots[0];
        foreach (Spot spot in spots) {
            float distance = Vector3.Distance(position, spot.transform.position);
            if (distance < minDistance) {
                minDistance = distance;
                closestSpot = spot;
            }
        }

        return closestSpot;
    }

    Unit GetClosestUnit(Vector3 position) {
        float minDistance = Mathf.Infinity;
        Unit closestUnit = units[0];
        foreach (Unit unit in units) {
            if (unit.spot != null) continue;

            float distance = Vector3.Distance(position, unit.GetComponent<FollowNavAgent>().agent.transform.position);
            if (distance < minDistance) {
                minDistance = distance;
                closestUnit = unit;
            }
        }

        return closestUnit;
    }

    void AssignSpot(Unit occupier, Spot spot) {
        spot.occupier = occupier;
        occupier.spot = spot;
        if (occupier != leader) {
            occupier.GetComponent<FollowNavAgent> ().targetToFollow = spot.transform;
        }
    }

    void ReassignSpots () {
        foreach (Spot spot in spots) {
            if (spot.occupier && spot.occupier != leader) {
                spot.occupier.spot = null;
                spot.occupier = null;
            }
        }

        foreach (Spot spot in spots) {
            if (!spot.occupier) AssignSpot(GetClosestUnit(spot.transform.position), spot);
        }
    }

    public void BreakFormation () {
        foreach (Unit unit in units) {
            unit.GetComponent<FollowNavAgent> ().targetToFollow = null;
            unit.spot = null;
            unit.formation = null;
        }

        Destroy(this.gameObject);
    }

    public void RemoveUnit (Unit rogueUnit) {
        rogueUnit.spot = null;
        rogueUnit.formation = null;
        units.Remove(rogueUnit);

        foreach (Unit unit in units) {
            unit.GetComponent<FollowNavAgent> ().targetToFollow = null;
            unit.spot = null;
        }

        if (units.Count == 1) {
            BreakFormation ();
        } else {
            AddSpots();
        }
    }

    public void SetUnitsAndTarget (Unit leader, List<Unit> units, Vector3 target) {
        this.leader = leader;
        this.units = new List<Unit>(units);

        foreach (Unit unit in units) {
            unit.GetComponent<FollowNavAgent>().targetToFollow = null;
            unit.spot = null;
            unit.formation = this;
        }

        leader.GetComponent<FollowNavAgent>().SetDestination(target);
        AddSpots();
    }
}
