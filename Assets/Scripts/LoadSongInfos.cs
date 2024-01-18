using Boomlagoon.JSON;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LoadSongInfos : MonoBehaviour
{
    public List<Song> AllSongs = new List<Song>();
    public int CurrentSong
    {
        get
        {
            return Songsettings.CurrentSongIndex;
        }
        set
        {
            Songsettings.CurrentSongIndex = value;
        }
    }

    public RawImage Cover;
    public TextMeshProUGUI SongName;
    public TextMeshProUGUI Artist;
    public TextMeshProUGUI BPM;
    public TextMeshProUGUI Levels;
    private SongSettings Songsettings;

    private void Awake()
    {
        Songsettings = SongSettings.Instance;
    }

    public void OnEnable()
    {
        AddSong();
    }
#if UNITY_ANDROID
    string ReadTextFromFile(string path)
    {
        string fileText = "";
        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(path);
        www.SendWebRequest();

        while (!www.isDone) { }
        if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            fileText = www.downloadHandler.text;
        } else
        {
            Debug.LogError("Failed to read file: " + path);
        }
        return fileText;
    }
#endif
    public void AddSong() {
#if UNITY_ANDROID
        // hard code for android
        var song = new Song();
        string base_path = Path.Combine(Application.streamingAssetsPath, "Playlists");
        song.Path = Path.Combine(base_path, "3833c (Viva La Vida - MadChase, Joshabi)");


        JSONObject infoFile = JSONObject.Parse(ReadTextFromFile(Path.Combine(song.Path, "Info.dat")));

        song.Name = infoFile.GetString("_songName");
        song.AuthorName = infoFile.GetString("_songAuthorName");
        song.BPM = infoFile.GetNumber("_beatsPerMinute").ToString();

        song.CoverImagePath = Path.Combine(song.Path, infoFile.GetString("_coverImageFilename"));

        song.AudioFilePath = Path.Combine(song.Path, infoFile.GetString("_songFilename"));
        song.Difficulties = new List<string>();

        var difficultyBeatmapSets = infoFile.GetArray("_difficultyBeatmapSets");
        foreach (var beatmapSets in difficultyBeatmapSets)
        {
            foreach (var difficultyBeatmaps in beatmapSets.Obj.GetArray("_difficultyBeatmaps"))
            {
                song.Difficulties.Add(difficultyBeatmaps.Obj.GetString("_difficulty"));
            }
        }

        AllSongs.Add(song);

#else
        string path = Path.Combine(Application.streamingAssetsPath + "/Playlists");
        if (Directory.Exists(path))
        {
            foreach (var dir in Directory.GetDirectories(path))
            {
                if (Directory.Exists(dir) && Directory.GetFiles(dir, "Info.dat").Length > 0)
                {
                    JSONObject infoFile = JSONObject.Parse(File.ReadAllText(Path.Combine(dir, "Info.dat")));

                    var song = new Song();
                    song.Path = dir;
                    song.Name = infoFile.GetString("_songName");
                    song.AuthorName = infoFile.GetString("_songAuthorName");
                    song.BPM = infoFile.GetNumber("_beatsPerMinute").ToString();
                    song.CoverImagePath = Path.Combine(dir, infoFile.GetString("_coverImageFilename"));
                    song.AudioFilePath = "file://"+Path.Combine(dir, infoFile.GetString("_songFilename"));
                    song.Difficulties = new List<string>();

                    var difficultyBeatmapSets = infoFile.GetArray("_difficultyBeatmapSets");
                    foreach (var beatmapSets in difficultyBeatmapSets)
                    {
                        foreach (var difficultyBeatmaps in beatmapSets.Obj.GetArray("_difficultyBeatmaps"))
                        {
                            song.Difficulties.Add(difficultyBeatmaps.Obj.GetString("_difficulty"));
                        }
                    }

                    AllSongs.Add(song);
                }
            }
        } else {
            Debug.LogFormat("{0} don't exists", path);
        }
#endif
    }

    public Song NextSong()
    {
        CurrentSong++;
        if(CurrentSong > AllSongs.Count - 1)
        {
            CurrentSong = 0;
        }

        Songsettings.CurrentSong = AllSongs[CurrentSong];

        return Songsettings.CurrentSong;
    }

    public Song PreviousSong()
    {
        CurrentSong--;
        if (CurrentSong < 0)
        {
            CurrentSong = AllSongs.Count - 1;
        }

        Songsettings.CurrentSong = AllSongs[CurrentSong];

        return Songsettings.CurrentSong;
    }

    public Song GetCurrentSong()
    {
        return Songsettings.CurrentSong;
    }
}

public class Song
{
    public string Path { get; set; }
    public string AudioFilePath { get; set; }
    public string Name { get; set; }
    public string AuthorName { get; set; }
    public string BPM { get; set; }
    public string CoverImagePath { get; set; }
    public List<string> Difficulties { get; set; }
    public string SelectedDifficulty { get; set; }
}
