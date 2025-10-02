using System.Collections;
using UnityEngine;

public class ShatterstoneBarrageProjectileMovement : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public IEnumerator ShatterstoneMoveProjectile(ShatterstoneBarrage sb, Vector3 spawnOffset)
    {
        Vector3 startPosition = transform.position;
        Vector3 riseEndPosition = startPosition + Vector3.up * GetRandomRiseHeight(sb);

        // PHASE 1: MOVE UPWARD
        float riseTime = 0f;
        while (riseTime < sb.RiseMaxTime)
        {
            // BREAK IF DESTROYED
            if (this == null)
            {
                yield break;
            }

            riseTime += Time.deltaTime * sb.RiseSpeed;
            transform.position = Vector3.Lerp(startPosition, riseEndPosition, riseTime);
            yield return null;
        }

        // BRIEF PAUSE BETWEEN PHASES
        yield return new WaitForSeconds(sb.PauseBetweenPhases);

        // ADD DAMAGER COMPONENT AND SET ATTRIBUTES. THERE WERE ISSUES HAVING IT ON THE PREFAB BY DEFAULT.
        EnemyDamager damager = gameObject.AddComponent<EnemyDamager>();
        damager.SetAttributes(sb);

        // PHASE 2: MOVE TOWARDS RETICLE DIRECTION
        Vector3 targetPosition = gameManager.CrosshairPositionIn3DSpace + spawnOffset; // PROJECTILE'S TARGET CONSIDERING OFFSET
        targetPosition.z = gameManager.CrosshairPositionIn3DSpace.z; // CLAMP INITIAL DIRECTION

        startPosition = transform.position;
        Vector3 direction = (targetPosition - startPosition).normalized; // PROJECTILE PATH

        while (true) // RUN UNTIL DESTROY IS CALLED FROM DAMAGER SCRIPT
        {
            // BREAK IF DESTROYED
            if (this == null)
            {
                yield break;
            }

            Vector3 velocity = sb.MoveSpeed * Time.deltaTime * direction;
            transform.position += velocity; // MOVE ALONG PATH

            // CONTINUOUSLY CLAMP Z ALONG DIRECTION
            Vector3 currentPos = transform.position;
            currentPos.z = gameManager.CrosshairPositionIn3DSpace.z;
            transform.position = currentPos;

            yield return null;
        }
    }

    private float GetRandomRiseHeight(ShatterstoneBarrage sb)
    {
        return (sb.RiseHeight + Random.Range(-sb.RiseHeightVariation, sb.RiseHeightVariation));
    }

    public void SetProjectileSize(float size)
    {
        float x = transform.localScale.x;
        float y = transform.localScale.y;
        float z = transform.localScale.z;

        x = x * size;
        y = y * size;
        z = z * size;

        transform.localScale = new (x, y, z);
    }
}