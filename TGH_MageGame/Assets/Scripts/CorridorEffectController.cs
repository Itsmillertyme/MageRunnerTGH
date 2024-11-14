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

        //CHAOS - idk why
        startPosition = transform.position;
        flippedPosition = direction == Direction.VERTICAL ? new Vector3(startPosition.x - roomSize, startPosition.y, startPosition.z) : new Vector3(startPosition.x, startPosition.y - roomSize, startPosition.z);

        roomMidPoint = (startPosition + flippedPosition) / 2;

        ////set direction of corridor
        //if (transform.localScale.x > transform.localScale.y) {
        //    direction = Direction.VERTICAL;
        //}
        //else {
        //    //horizontal
        //    direction = Direction.HORTIZONTAL;
        //}

    }


    void Update() {
        if (direction == Direction.VERTICAL) {
            //face player
            if (playerController.transform.position.x > roomMidPoint.x) {
                transform.localRotation = Quaternion.Euler(0, 90, 0);
                transform.position = startPosition;
            }
            else {
                transform.localRotation = Quaternion.Euler(0, -90, 0);
                transform.position = flippedPosition;
            }
        }
        else if (direction == Direction.HORTIZONTAL) {
            //face player
            if (playerController.transform.position.y < roomMidPoint.y) {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                transform.position = startPosition;
            }
            else {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
                transform.position = flippedPosition;
            }
        }


    }

    public void SetupEffect(Vector3 StartPosIn, Direction directionIn, int corridorSize) {

        startPosition = StartPosIn;
        direction = directionIn;

        //Debug.Log("startPos: " + startPosition);
        //Debug.Log("direction: " + direction);
        //Debug.Log("corridorSize: " + corridorSize);

        if (direction == Direction.HORTIZONTAL) {
            flippedPosition = new Vector3(startPosition.x + corridorSize, startPosition.y, startPosition.z);
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (direction == Direction.VERTICAL) {
            flippedPosition = new Vector3(startPosition.x, startPosition.y, startPosition.z - corridorSize);
            transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        //Debug.Log("flippedPosition: " + flippedPosition);
    }

}

public enum Direction {
    VERTICAL = 0,
    HORTIZONTAL = 1
}

