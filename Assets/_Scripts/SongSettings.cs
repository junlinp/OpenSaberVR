using UnityEngine;

public class SongSettings : MonoBehaviour
{

    public static SongSettings instance_;

    public static SongSettings Instance {
        get {
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
            return instance_;}
        private set{ instance_ = value;}
         }
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }
    }
    public Song CurrentSong;
    public int CurrentSongIndex = 0;
}
