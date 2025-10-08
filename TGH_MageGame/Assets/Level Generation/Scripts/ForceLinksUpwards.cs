using Unity.AI.Navigation;
using UnityEngine;

[ExecuteInEditMode]
public class ForceLinksUpwards : MonoBehaviour {
    void Start() {
        NavMeshLink[] links = FindObjectsByType<NavMeshLink>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (NavMeshLink link in links) {
            Vector3 start = link.startPoint;
            Vector3 end = link.endPoint;

            // Only nudge if the link is going upwards
            if (start.y < end.y) {
                link.startPoint = new Vector3(start.x, start.y + 0.1f, start.z);
                Debug.Log($"Forced link upwards from {start} to {end}");
            }
        }
    }
}
