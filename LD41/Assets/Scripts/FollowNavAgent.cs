using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowNavAgent : MonoBehaviour {

    public NavMeshAgent agent;
    public Transform targetToFollow;
    public Transform raycastPoint;
    public LayerMask terrainMask;

    private Soldier soldier;

    void Start () {
        soldier = GetComponent<Soldier>();
        agent.updateRotation = false;
    }

    public void SetDestination (Vector3 position) {
        targetToFollow = null;
        position.y = agent.transform.position.y;
        agent.SetDestination (position);
    }

    void Update () {
        if (targetToFollow != null) {
            Vector3 pos = targetToFollow.position;
            //pos.y = 0.75f;
            targetToFollow.position = pos;
            agent.SetDestination(targetToFollow.position);
        }

        if (soldier.order != Commands.Attack) {
            Vector3 agentPos = agent.transform.position;
            agentPos.y = 0;
            Vector3 destPos = agent.destination;
            destPos.y = 0;
            float distToTarget = Vector3.Distance(agentPos, destPos);
            if (distToTarget > 0.1f || agent.velocity.x > 0.1f || agent.velocity.z > 0.1f) {
                soldier.order = Commands.Goto;
            } else {
                soldier.order = Commands.None;
            }
        }
    }
		
	void FixedUpdate () {
        RaycastHit hit;
        Vector3 ray = raycastPoint.TransformDirection(Vector3.down) * 100;
        if (Physics.Raycast(raycastPoint.position, ray, out hit, 100, terrainMask)) {
            Vector3 terrainPoint = hit.point;
            Debug.DrawLine(raycastPoint.position, hit.point, Color.red);
            transform.position = Vector3.MoveTowards(transform.position, terrainPoint, agent.speed);
        } else {
            Debug.DrawRay(raycastPoint.position, ray, Color.red);
        }
	}
}
