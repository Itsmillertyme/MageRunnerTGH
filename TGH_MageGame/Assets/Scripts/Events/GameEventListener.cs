using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// UNUSED. THIS JUST MIGHT BE NICE TO HAVE LATER
// THIS SCRIPT LISTENS FOR GAME EVENT SCRIPTABLE OBJECTS.
// IT'S ATTACHED TO A LISTENER IN THE SCENE.

public class GameEventListener : MonoBehaviour
{
    public GameEvent Event;

    public UnityEvent Response;

    private void OnEnable()
    {
        Event.AddToListener(this);
    }

    private void OnDisable()
    {
        Event.RemoveFromListener(this);
    }

    public void OnEventRaised()
    {
        Response.Invoke();
    }
}