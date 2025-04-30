using UnityEngine;

/* THIS SCRIPT CREATES A SCRIPTABLE OBJECT TYPE THAT IS USED AS A 'PLAYLIST' IN
   THE MUSIC MANAGER. */

[CreateAssetMenu()]

public class MusicPlaylist : ScriptableObject
{
    [SerializeField] private AudioClip[] playlist;

    public AudioClip[] Playlist => playlist;
}