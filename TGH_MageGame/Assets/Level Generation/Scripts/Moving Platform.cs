using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

    //**PROPERTIES**
    [SerializeField] Transform initialPosition;
    [SerializeField] Transform destinationPosition;
    [SerializeField] GameObject platformObject;
    [SerializeField] float travelTime = 5f;
    [SerializeField] float waitTime = 1f;
    //
    Transform currentTarget;

    //**UNITY METHODS**
    private void Start() {

        //Initialize
        currentTarget = destinationPosition;

        StartCoroutine(LerpToPosition());
    }

    IEnumerator LerpToPosition() {
        //Helpers
        float timeElapsed = 0f;
        Vector3 startingPos = platformObject.transform.position;
        Vector3 targetPos = currentTarget.position;

        //Lerp
        while (timeElapsed < travelTime) {
            platformObject.transform.position = Vector3.Lerp(startingPos, targetPos, timeElapsed / travelTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }


        //Wait for cooldown
        yield return new WaitForSeconds(waitTime);

        //Switch target
        currentTarget = currentTarget == initialPosition ? destinationPosition : initialPosition;

        //Start moving again
        StartCoroutine(LerpToPosition());
    }
}
