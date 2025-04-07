using UnityEngine;
using System.Collections;

public class MagicAttack : MonoBehaviour
{
    /* 
    works perfectly. just need to work to implement to spellbook. not far off from that goal.
     */
    [SerializeField] private GameObject projectilePrefab;    // Assign your projectile prefab in inspector
    [SerializeField] private Vector3 spawnPoint;            // Set this in inspector for spawn location
    [SerializeField] private int projectileCount = 10;       // Number of projectiles
    [SerializeField] private float spawnRadius = 2f;         // Radius around spawn point
    [SerializeField] private float riseHeight = 3f;          // How high projectiles rise
    [SerializeField] private float riseSpeed = 2f;           // Speed of rising phase
    [SerializeField] private float attackSpeed = 5f;         // Speed toward target
    [SerializeField] private float delayBetweenSpawns = 0.1f;// Stagger spawn timing
    [SerializeField] private float riseMaxTime = 1.5f;       // How long projectiles rise
    [SerializeField] private float closeToTarget = 0.2f;     // Close enough to target distance


    private Vector3 targetPoint;
    private bool isCasting = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isCasting) // Left click to cast
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                targetPoint = hit.point;
                StartCasting();
            }

            /* use instead once i've got it to spellbook
            // Convert mouse position to world space, assuming Z=0 plane
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.nearClipPlane; // Set to near plane for proper conversion
            targetPoint = Camera.main.ScreenToWorldPoint(mousePosition);
            targetPoint.z = 0; // Force Z to 0 for 2.5D
            StartCasting();
            */
        }
    }

    void StartCasting()
    {
        isCasting = true;
        StartCoroutine(SpawnProjectiles());
    }

    IEnumerator SpawnProjectiles()
    {
        for (int i = 0; i < projectileCount; i++)
        {
            // Random position around designated spawn point
            Vector3 spawnOffset = Random.insideUnitSphere * spawnRadius;
            spawnOffset.z = 0;
            Vector3 spawnPos = spawnPoint + spawnOffset;
            spawnPos.y = spawnPoint.y; // Keep at spawn point's Y level initially

            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            StartCoroutine(ProjectileSequence(projectile, spawnOffset));

            yield return new WaitForSeconds(delayBetweenSpawns);
        }

        // Reset casting state after all projectiles are spawned
        yield return new WaitForSeconds(delayBetweenSpawns * projectileCount);
        isCasting = false;
    }

    IEnumerator ProjectileSequence(GameObject projectile, Vector3 spawnOffset)
    {
        Vector3 startPos = projectile.transform.position;
        Vector3 risePos = startPos + Vector3.up * riseHeight;

        // Phase 1: Rise upward
        float riseTime = 0f;
        while (riseTime < riseMaxTime)
        {
            riseTime += Time.deltaTime * riseSpeed;
            projectile.transform.position = Vector3.Lerp(startPos, risePos, riseTime);
            yield return null;
        }

        // Phase 2: Move toward target while maintaining offset
        Vector3 groupCenterStart = risePos - spawnOffset; // Center of group after rising
        Vector3 targetCenter = targetPoint;              // Where the center of the group should end
        targetCenter.z = 0;
        Vector3 finalPosition = targetCenter + spawnOffset; // Individual projectile's target
        finalPosition.z = 0;

        Vector3 attackStartPos = projectile.transform.position;
        Vector3 direction = (finalPosition - attackStartPos).normalized;

        while (Vector3.Distance(projectile.transform.position, finalPosition) > closeToTarget)
        {
            // Move at constant speed
            Vector3 velocity = direction * attackSpeed * Time.deltaTime;
            projectile.transform.position += velocity;

            // Gradually adjust Z toward 0
            Vector3 currentPos = projectile.transform.position;
            currentPos.z = Mathf.Lerp(currentPos.z, 0, Time.deltaTime * attackSpeed);
            projectile.transform.position = currentPos;

            yield return null;
        }

        // Set final position and destroy
        finalPosition.z = 0; // Ensure exact Z=0 at end
        projectile.transform.position = finalPosition;
        Destroy(projectile);
    }
}