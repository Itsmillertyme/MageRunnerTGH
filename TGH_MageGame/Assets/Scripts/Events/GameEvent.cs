using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]

// GAME EVENT SCRIPTABLE OBJECTS CAN BE USED TO TRIGGER EVENTS
// THE OBJECTS ARE MADE AND ASSIGNED IN THE INSPECTOR

public class GameEvent : ScriptableObject {
    // LIST OF THOSE TO BE NOTIFIED
    private List<GameEventListener> listeners = new List<GameEventListener>();

    public void AddToListener(GameEventListener listener) {
        listeners.Add(listener);
    }

    public void RemoveFromListener(GameEventListener listener) {
        listeners.Remove(listener);
    }

    public void Raise() {
        for (int i = listeners.Count - 1; i >= 0; i--) {
            listeners[i].OnEventRaised();
        }
    }
}
