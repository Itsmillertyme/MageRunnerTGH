using System.Collections.Generic;
using UnityEngine;

public class Notes : MonoBehaviour
{
    [SerializeField] private bool displayNotes;
    [SerializeField] private List<string> notes = new List<string>();

    private void Awake()
    {
        if (displayNotes)
        {
            foreach (var note in notes)
            {
                Debug.Log(note);
            }
        }
    }
}