using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowNavAgent : MonoBehaviour {

    public NavMeshAgent agent;
    public Transform targetToFollow;
    public Transform raycastPoint;
    public LayerMask terrainMask;

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
