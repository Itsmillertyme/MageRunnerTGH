using UnityEngine;
using UnityEngine.AI;

public class DEVEnemySpawner : MonoBehaviour {
    [SerializeField] GameObject testPrefab;
    [SerializeField] Transform[] waypoints;
    private void Awake() {

        GameObject mob = Instantiate(testPrefab, new Vector3(0, .2f, 0), Quaternion.Euler(0, 180f, 0));
        NavMeshAgent agent = mob.GetComponent<NavMeshAgent>();
        agent.SetDestination(waypoints[0].position);

    }
}
