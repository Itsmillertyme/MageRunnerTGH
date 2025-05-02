using System.Collections;
using UnityEngine;

// THIS SCRIPT MANAGES MUSIC ACROSS ALL LEVELS

public class MusicManager : MonoBehaviour {
    [SerializeField] private float backgroundVolume = 0.3f;
    [SerializeField] private MusicPlaylist[] soundTrack; // THE GAME'S OST
    [SerializeField] private float songTransitionTime = 0.15f;

    private AudioClip[] currentPlaylist;
    private int currentSongIndex = 0;
    private int currentPlaylistIndex = 0;
    private AudioSource music;
    private Coroutine playingCoroutine;

    private void Awake() {
        music = GetComponent<AudioSource>();
        music.volume = backgroundVolume;
    }

    private void Start() {
        SwitchPlaylist(0);
        PlayTrack();
    }

    private IEnumerator PlayTrackRoutine() {
        yield return new WaitForSeconds(music.clip.length + songTransitionTime);

        SwitchToNextTrack();
        PlayTrack();
    }

    private void PlayTrack() {
        music.clip = currentPlaylist[currentSongIndex];
        music.Play();

        // RESET COROUTINE
        if (playingCoroutine != null) {
            StopCoroutine(playingCoroutine);
        }
        playingCoroutine = StartCoroutine(PlayTrackRoutine());
    }

    private void SwitchToNextTrack() {
        if (currentSongIndex < currentPlaylist.Length - 1) // IF NOT AT END OF PLAYLIST
        {
            currentSongIndex++; // NEXT TRACK
        }
        else // IF AT END OF PLAYLIST
        {
            currentSongIndex = 0; // JUMP TO FIRST TRACK OF PLAYLIST
        }
    }

    public void PlayNextTrack() {
        SwitchToNextTrack();
        PlayTrack();
    }

    public void PauseMusic() {
        if (music.isPlaying) {
            music.Pause();
        }
    }

    public void ResumeMusic() {
        if (!music.isPlaying) {
            music.UnPause();
        }
    }

    public void SwitchPlaylist(int sceneIndex)  // RUNS OFF OF SCENE BUILD INDEX VALUES
    {
        if (music.isPlaying) {
            music.Stop();
        }

        if (playingCoroutine != null) {
            StopCoroutine(playingCoroutine);
        }

        currentPlaylistIndex = sceneIndex;
        currentPlaylist = soundTrack[currentPlaylistIndex].Playlist;
        currentSongIndex = 0;
        PlayTrack();
    }
}