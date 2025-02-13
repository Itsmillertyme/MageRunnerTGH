using UnityEngine;

public class AnimatorForward : MonoBehaviour {
    //Components
    Animator animator;


    private void Awake() {
        //Cache components references
        animator = GetComponent<Animator>();

    }

    private void OnAnimatorMove() {
        //output Vector
        Vector3 forwardedMotion;

        //Test if y movement is less than a threshold (cancels out camera bobbing from animation)
        if (System.Math.Abs(animator.deltaPosition.y) < .1) {
            //remove y component from vector
            forwardedMotion = new Vector3(animator.deltaPosition.x, 0, animator.deltaPosition.z);
        }
        else {
            //pass vector through
            forwardedMotion = animator.deltaPosition;
        }

        //update parent object's position
        GameObject.FindWithTag("Player").transform.position += forwardedMotion;

    }
}

