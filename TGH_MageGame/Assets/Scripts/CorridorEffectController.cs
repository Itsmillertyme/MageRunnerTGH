using UnityEngine;

public class CorridorEffectController : MonoBehaviour {

    PlayerController playerController;

    [SerializeField] Direction direction;
    [SerializeField] Vector3 startPosition;
    [SerializeField] Vector3 flippedPosition;
    [SerializeField] Vector3 roomMidPoint;


    private void Awake() {
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

        float roomSize = direction == Direction.VERTICAL ? startPosition.z - flippedPosition.z : startPosition.x - flippedPosition.x;

        roomMidPoint = (startPosition + flippedPosition) / 2;

    }


    void Update() {
        if (direction == Direction.VERTICAL) {
            //face player
            if (playerController.transform.position.x > startPosition.z) {
                transform.localRotation = Quaternion.Euler(0, 90, 0);
                transform.localPosition = startPosition;
            }
            else if (playerController.transform.position.x < flippedPosition.z) {
                transform.localRotation = Quaternion.Euler(0, -90, 0);
                transform.localPosition = flippedPosition;
            }
        }
        else if (direction == Direction.HORTIZONTAL) {
            //face player
            if (playerController.transform.position.y < startPosition.x) {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                transform.localPosition = startPosition;
            }
            else if (playerController.transform.position.y > flippedPosition.x) {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
                transform.localPosition = flippedPosition;
            }
        }


    }

    public void SetupEffect(Vector3 StartPosIn, Direction directionIn, float corridorSizeIn) {

        startPosition = StartPosIn;
        direction = directionIn;

        transform.localPosition = startPosition;

        //Debug.Log("startPos: " + startPosition);
        //Debug.Log("direction: " + direction);
        //Debug.Log("corridorSize: " + corridorSize);

        flippedPosition = direction == Direction.VERTICAL ? new Vector3(startPosition.x, startPosition.y, startPosition.z - corridorSizeIn) : new Vector3(startPosition.x + corridorSizeIn, startPosition.y, startPosition.z);

        roomMidPoint = (startPosition + flippedPosition) / 2;

        ParticleSystem ps = GetComponent<ParticleSystem>();
        ps.startLifetime = 2.5f * (corridorSizeIn + 2) / 13f;

    }

}

public enum Direction {
    VERTICAL = 0,
    HORTIZONTAL = 1
}

