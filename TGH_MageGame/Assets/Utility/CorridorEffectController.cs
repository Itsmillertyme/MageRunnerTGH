using UnityEngine;

public class CorridorEffectController : MonoBehaviour {

    //    //**PROPERTIES**
    //    [SerializeField] float corridorSpeed;
    //    [SerializeField] Direction direction;
    //    [SerializeField] Vector3 startPosition;
    //    [SerializeField] Vector3 flippedPosition;
    //    [SerializeField] Vector3 roomMidPoint;
    //    [SerializeField] bool isFlipped;
    //    [SerializeField] ParticleSystem ps;

    //    PathNode corridorPathNode;
    //    Coroutine transportPlayer;
    //    PlayerController playerController;
    //    bool isActive;

    //    //**FIELDS**
    //    public bool IsActive { get => isActive; set => SetCorridorState(value); }
    //    public PathNode CorridorPathNode { get => corridorPathNode; set => corridorPathNode = value; }

    //    //**UNITY METHODS**
    //    private void Awake() {
    //        //Cache references
    //        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    //        ps = GetComponent<ParticleSystem>();

    //        //Initialize
    //        isActive = false;
    //        //float roomSize = direction == Direction.VERTICAL ? startPosition.z - flippedPosition.z : startPosition.x - flippedPosition.x;
    //        roomMidPoint = (startPosition + flippedPosition) / 2;

    //    }
    //    //
    //    void Update() {
    //        if (direction == Direction.VERTICAL) {
    //            //face player
    //            if (playerController.transform.position.x > startPosition.z) {
    //                transform.localRotation = Quaternion.Euler(0, 90, 0);
    //                transform.localPosition = startPosition;
    //                isFlipped = false;
    //            }
    //            else if (playerController.transform.position.x < flippedPosition.z) {
    //                transform.localRotation = Quaternion.Euler(0, -90, 0);
    //                transform.localPosition = flippedPosition;
    //                isFlipped = true;
    //            }
    //        }
    //        else if (direction == Direction.HORTIZONTAL) {
    //            //face player
    //            if (playerController.transform.position.y + 2 < startPosition.x) {
    //                transform.localRotation = Quaternion.Euler(0, 0, 0);
    //                transform.localPosition = startPosition;
    //                isFlipped = false;
    //            }
    //            else if (playerController.transform.position.y > flippedPosition.x) {
    //                transform.localRotation = Quaternion.Euler(0, 180, 0);
    //                transform.localPosition = flippedPosition;
    //                isFlipped = true;
    //            }
    //        }


    //    }
    //    //
    //    private void OnTriggerEnter(Collider other) {
    //        if (other.gameObject.CompareTag("Player")) {
    //            if (transportPlayer == null) {
    //                transportPlayer = StartCoroutine(MovePlayer(other.gameObject));
    //            }
    //        }
    //    }

    //    //**UTILITY METHODS**
    //    public void SetupEffect(Vector3 StartPosIn, Direction directionIn, float corridorSizeIn) {

    //        startPosition = StartPosIn;
    //        direction = directionIn;
    //        isFlipped = false;

    //        transform.localPosition = startPosition;

    //        //Debug.Log("startPos: " + startPosition);
    //        //Debug.Log("direction: " + direction);
    //        //Debug.Log("corridorSize: " + corridorSize);

    //        flippedPosition = direction == Direction.VERTICAL ? new Vector3(startPosition.x, startPosition.y, startPosition.z - corridorSizeIn) : new Vector3(startPosition.x + corridorSizeIn, startPosition.y, startPosition.z);

    //        roomMidPoint = (startPosition + flippedPosition) / 2;

    //        ParticleSystem ps = GetComponent<ParticleSystem>();
    //#pragma warning disable CS0618 // Type or member is obsolete
    //        ps.startLifetime = 2.5f * (corridorSizeIn + 2) / 13f;
    //#pragma warning restore CS0618 // Type or member is obsolete

    //    }

    //    public void SetCorridorState(bool isActiveIn) {
    //        ParticleSystem.TrailModule trails = ps.trails;
    //        if (isActiveIn) {
    //            //Handle particles
    //#pragma warning disable CS0618 // Type or member is obsolete
    //            ps.startColor = Color.white;
    //            ps.maxParticles = 200;
    //            ps.emissionRate = 50;
    //            trails.colorOverTrail = Color.white;
    //            trails.lifetime = 1f;
    //#pragma warning restore CS0618 // Type or member is obsolete

    //            //take down invisible plane
    //            transform.GetChild(0).gameObject.SetActive(false);
    //        }
    //        else {
    //            //Handle particles
    //#pragma warning disable CS0618 // Type or member is obsolete
    //            ps.startColor = Color.red;
    //            ps.maxParticles = 100;
    //            ps.emissionRate = 25;
    //            trails.colorOverTrail = Color.red;
    //            trails.lifetime = 0.05f;
    //#pragma warning restore CS0618 // Type or member is obsolete

    //            //put up invisible plane
    //            transform.GetChild(0).gameObject.SetActive(true);
    //        }

    //        //Set property
    //        isActive = isActiveIn;
    //    }

    //    //**COROUTINES**
    //    IEnumerator MovePlayer(GameObject playerObject) {

    //        //Helpers
    //        Vector3 outputPos;
    //        PlayerController playerController = playerObject.GetComponent<PlayerController>();
    //        bool sidePush = direction == Direction.HORTIZONTAL && !isFlipped;
    //        GameManager gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
    //        //
    //        PathNode neighborA = corridorPathNode.neighbors[0].GetComponent<PathNode>();
    //        PathNode neighborB = corridorPathNode.neighbors[1].GetComponent<PathNode>();
    //        PathNode nextRoom = neighborA == gm.CurrentPathNode ? neighborB : neighborA;

    //        //set GM current pathnode to corridor
    //        gm.CurrentPathNode = corridorPathNode;

    //        //Freze player physics
    //        playerController.FreezePhysics = true;

    //        //setup destination
    //        if (!isFlipped) {
    //            outputPos = direction == Direction.VERTICAL ? new Vector3(flippedPosition.z - 2, flippedPosition.x, 2.5f) : new Vector3(flippedPosition.z, flippedPosition.x + 2, 2.5f);
    //        }
    //        else {
    //            outputPos = direction == Direction.VERTICAL ? new Vector3(startPosition.z + 2, startPosition.x, 2.5f) : new Vector3(startPosition.z, startPosition.x - 2, 2.5f);
    //        }

    //        bool notArrived = direction == Direction.VERTICAL ? Mathf.Abs(playerObject.transform.position.x - outputPos.x) > 0.1 : Mathf.Abs(playerObject.transform.position.y - outputPos.y) > 0.1;

    //        while (notArrived) {

    //            //setup direction
    //            Vector3 dir = (outputPos - playerObject.transform.position).normalized;

    //            //move player
    //            playerObject.GetComponent<CharacterController>().Move(dir * corridorSpeed * Time.deltaTime);

    //            notArrived = direction == Direction.VERTICAL ? Mathf.Abs(playerObject.transform.position.y - outputPos.y) > 0.1 : Mathf.Abs(playerObject.transform.position.x - outputPos.x) > 0.1;

    //            //wait until end of frame
    //            yield return new WaitForEndOfFrame();
    //        }


    //        if (sidePush) {

    //            outputPos = new Vector3(outputPos.x - 3, outputPos.y + 2, outputPos.z);

    //            while (Mathf.Abs(playerObject.transform.position.x - outputPos.x) > 0.1) {

    //                //setup direction
    //                Vector3 dir = (outputPos - playerObject.transform.position).normalized;

    //                //move player
    //                playerObject.GetComponent<CharacterController>().Move(dir * corridorSpeed * Time.deltaTime);

    //                //wait until end of frame
    //                yield return new WaitForEndOfFrame();
    //            }

    //        }

    //        //unfreeze player
    //        playerController.FreezePhysics = false;

    //        //set GM current pathnode to next room
    //        gm.CurrentPathNode = nextRoom;

    //        //Set corridor inactive
    //        SetCorridorState(false);

    //        //cooldown
    //        yield return new WaitForSeconds(3);

    //        //Set corridor active
    //        SetCorridorState(true);

    //        //reset coroutine
    //        transportPlayer = null;
    //    }

    //}

    //public enum Direction {
    //    VERTICAL = 0,
    //    HORTIZONTAL = 1
}

