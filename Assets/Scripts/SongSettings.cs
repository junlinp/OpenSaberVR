using UnityEngine;

public class SongSettings : MonoBehaviour
{

    public static SongSettings instance_;

    public static SongSettings Instance
    {
        get
        {
            if (instance_ == null)
            {
                // If the instance is null, find it in the scene or create a new GameObject
                instance_ = FindObjectOfType<SongSettings>();
                if (instance_ == null)
                {
                    // If still null, create a new GameObject and attach the singleton script
                    GameObject singletonObject = new GameObject("SongSettings");
                    instance_ = singletonObject.AddComponent<SongSettings>();
                }
            }
            return instance_;
        }
        private set { instance_ = value; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else {
            Instance = this;
        }
    }
    
    private SongSettings()
    {
        current_song = new Song();
        current_song_index = 0;
    }

    private Song current_song;

    private int current_song_index = 0;


    public Song CurrentSong {
        get => current_song;
        set => current_song = value;
    }

    public int CurrentSongIndex {
        get => current_song_index;
        set => current_song_index = value;
    }
}
