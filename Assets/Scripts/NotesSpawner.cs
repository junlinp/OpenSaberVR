/*
 * The spawner code and also the correct timing stuff was taken from the project:
 * BeatSaver Viewer (https://github.com/supermedium/beatsaver-viewer) and ported to C#.
 * 
 * To be more precisly most of the code in the Update() method was ported to C# by me 
 * from their project.
 * 
 * Without that project this project won't exist, so thank you very much for releasing 
 * the source code under MIT license!
 */

using Boomlagoon.JSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using dm;

public class NotesSpawner : MonoBehaviour
{
    public GameObject[] Cubes;
    public GameObject Wall;
    public Transform[] SpawnPoints;

    private string jsonString;
    private string audioFilePath;
    private List<ColorNote> NotesToSpawn = new List<ColorNote>();
    private PriorityQueue<double, ColorNote> notes_to_spawn = new PriorityQueue<double, ColorNote>();
    private List<Obstacle> ObstaclesToSpawn = new List<Obstacle>();
    private double BeatsPerMinute;

    private double BeatsTime = 0;
    private double? BeatsPreloadTime = 0;
    private double BeatsPreloadTimeTotal = 0;
    private double current_time_in_seconds = 0;

    private readonly double beatAnticipationTime = 1.1;
    private readonly double beatSpeed = 19.0;
    private readonly double beatWarmupTime = BeatsConstants.BEAT_WARMUP_TIME / 1000;
    private readonly double beatWarmupSpeed = BeatsConstants.BEAT_WARMUP_SPEED;

    private AudioSource audioSource;

    private SongSettings songSettings;
    private SceneHandling SceneHandling;
    private bool menuLoadInProgress = false;
    private bool audioLoaded = false;
    private GameState gameState = GameState.ChooseSongs;

    void Start()
    {
         //songSettings= SongSettings.Instance;
        //SceneHandling = SceneHandling.Instance;

    }
    private void Awake()
    {
        songSettings = SongSettings.Instance;
        SceneHandling = SceneHandling.Instance;
    }
    public void OnEnable() {
        Debug.LogFormat("Songsettings is null {0}",  songSettings== null);
        Debug.LogFormat("Songsettings.CurrentSong is null {0}", songSettings.CurrentSong == null);
        if (songSettings.CurrentSong == null)
        {
            return;
        }
        string path = songSettings.CurrentSong.Path;
        string version = ""; 
        string difficulty_path = "";
        bool LoadPathSuccess = false;
        if (Directory.Exists(path))
        {
            if (Directory.GetFiles(path, "Info.dat").Length > 0)
            {
                JSONObject infoFile = JSONObject.Parse(File.ReadAllText(Path.Combine(path, "Info.dat")));
                version = infoFile.GetString("_version");
                var difficultyBeatmapSets = infoFile.GetArray("_difficultyBeatmapSets");
                foreach (var beatmapSets in difficultyBeatmapSets)
                {
                    foreach (var difficultyBeatmaps in beatmapSets.Obj.GetArray("_difficultyBeatmaps"))
                    {
                        if (difficultyBeatmaps.Obj.GetString("_difficulty") == songSettings.CurrentSong.SelectedDifficulty)
                        {
                            audioFilePath = Path.Combine(path, infoFile.GetString("_songFilename"));
                            difficulty_path = Path.Combine(path, difficultyBeatmaps.Obj.GetString("_beatmapFilename"));
                            jsonString = File.ReadAllText(Path.Combine(path, difficultyBeatmaps.Obj.GetString("_beatmapFilename")));
                            LoadPathSuccess = true;
                            break;
                        }
                    }
                }
            }
        }

        audioSource = GetComponent<AudioSource>();
        Debug.LogFormat("Load Path Success {0}", LoadPathSuccess);
        gameState = GameState.LoadingSongs;
        StartCoroutine("LoadAudio");

        JSONObject json = JSONObject.Parse(jsonString);

        var bpm = Convert.ToDouble( songSettings.CurrentSong.BPM);

        //Notes
        /*
        var notes = json.GetArray("_notes");
        foreach (var note in notes)
        {
            var n = new Note
            {
                Hand = (NoteType)note.Obj.GetNumber("_type"),
                CutDirection = (CutDirection)note.Obj.GetNumber("_cutDirection"),
                LineIndex = (int)note.Obj.GetNumber("_lineIndex"),
                LineLayer = (int)note.Obj.GetNumber("_lineLayer"),
                TimeInSeconds = (note.Obj.GetNumber("_time") / bpm) * 60,
                Time = (note.Obj.GetNumber("_time"))
            };

            NotesToSpawn.Add(n);
        }
        */
        NotesToSpawn = Difficulty.ParseJson(difficulty_path);
        Debug.LogFormat("Load {0} Notes", NotesToSpawn.Count);
        //Obstacles
        //var obstacles = json.GetArray("_obstacles");
        //foreach (var obstacle in obstacles)
        //{
        //    var o = new Obstacle
        //    {
        //        Type = (ObstacleType)obstacle.Obj.GetNumber("_type"),
        //        Duration = obstacle.Obj.GetNumber("_duration"),
        //        LineIndex = (int)obstacle.Obj.GetNumber("_lineIndex"),
        //        TimeInSeconds = (obstacle.Obj.GetNumber("_time") / bpm) * 60,
        //        Time = (obstacle.Obj.GetNumber("_time")),
        //        Width = (obstacle.Obj.GetNumber("_width"))
        //    };

        //    ObstaclesToSpawn.Add(o);
        //}

        BeatsPerMinute = bpm;
        BeatsPreloadTimeTotal = (beatAnticipationTime + beatWarmupTime);
        foreach (ColorNote note in NotesToSpawn)
        {
            notes_to_spawn.Enqueue(note.TimeInBeat, note);
            
        }
    }

    private IEnumerator LoadAudio()
    {
        if (gameState != GameState.LoadingSongs)
        {
            Debug.Log("LoadAudio but not in LoadingSongs state");
            yield return null;
        }
        var downloadHandler = new DownloadHandlerAudioClip( songSettings.CurrentSong.AudioFilePath, AudioType.OGGVORBIS);
        downloadHandler.compressed = false;
        downloadHandler.streamAudio = true;
        var uwr = new UnityWebRequest(
                 songSettings.CurrentSong.AudioFilePath,
                UnityWebRequest.kHttpVerbGET,
                downloadHandler,
                null);

        var request = uwr.SendWebRequest();
        while (!request.isDone)
            yield return null;

        audioSource.clip = DownloadHandlerAudioClip.GetContent(uwr);
        audioLoaded = true;
        gameState = GameState.GameReady;
    }
    [ContextMenu("loadmenu")]
    public void LoadBackMenu(){
            StartCoroutine(LoadMenu());
    }

    void Update()
    {
        switch (gameState)
        {
            case GameState.GameReady:
                {
                    audioSource.Play();
                    current_time_in_seconds = audioSource.time;
                    gameState = GameState.GameInPrograss;
                    break;
                }
            case GameState.GameInPrograss:
                {
                    current_time_in_seconds += Time.deltaTime;
                    double beats_need_to_preload = (BeatsConstants.ANTICIPATION_DURATION_SECONDS + BeatsConstants.WARM_UP_DURATION_SECONDS) / 60.0 * BeatsPerMinute;
                    double current_beats = current_time_in_seconds * (BeatsPerMinute / 60.0);

                    while(!notes_to_spawn.IsEmpty())
                    {
                        ColorNote note = notes_to_spawn.Front();
                        //Debug.LogFormat("Current Beats : {0}, note Beat {1}", current_beats, note.TimeInBeat);
                        if (note.TimeInBeat <= current_beats + beats_need_to_preload)
                        {
                            note = notes_to_spawn.Dequeue();
                            GenerateNote(note);
                        } else
                        {
                            break;
                        }
                    }
                    
                    break;
                }
            default:
                break;
        }
        /*
        var prevBeatsTime = BeatsTime;

        if (BeatsPreloadTime == null)
        {
            if (!audioSource.isPlaying)
            {
                if (!menuLoadInProgress)
                {
                    menuLoadInProgress = true;
                    StartCoroutine(LoadMenu());
                }
                return;
            }

            BeatsTime = (audioSource.time + beatAnticipationTime + beatWarmupTime) * 1000;
        }
        else
        {
            BeatsTime = BeatsPreloadTime.Value;
        }
        

        double msPerBeat = 1000 * 60 / BeatsPerMinute;

        // Notes
        // TODO(junlinp): using a queue to process
        //
        for (int i = 0; i < NotesToSpawn.Count; ++i)
        {
            var noteTime = NotesToSpawn[i].TimeInBeat * msPerBeat;
            if (noteTime > prevBeatsTime && noteTime <= BeatsTime)
            {
                //
                // FIXME(junlinp): it's necessary?
                //NotesToSpawn[i].Time = noteTime;
                //
                //
                GenerateNote(NotesToSpawn[i]);
            }
        }

        //Obstacles
        for (int i = 0; i < ObstaclesToSpawn.Count; ++i)
        {
            var noteTime = ObstaclesToSpawn[i].Time * msPerBeat;
            if (noteTime > prevBeatsTime && noteTime <= BeatsTime)
            {
                ObstaclesToSpawn[i].Time = noteTime;
                GenerateObstacle(ObstaclesToSpawn[i]);
            }
        }

        if (BeatsPreloadTime == null) { return; }

        if (BeatsPreloadTime.Value >= BeatsPreloadTimeTotal)
        {
            if (audioLoaded)
            {
                // Finished preload.
                BeatsPreloadTime = null;
                audioSource.Play();
            }
        }
        else
        {
            // Continue preload.
            BeatsPreloadTime += Time.deltaTime;
        }
        */
    }

    IEnumerator LoadMenu()
    {
        yield return new WaitForSeconds(5);
        yield return SceneHandling.LoadScene("Menu", LoadSceneMode.Additive);
        yield return SceneHandling.UnloadScene("OpenSaber");
    }

    void GenerateNote(ColorNote note)
    {
        int point = note.yLayer * 4 + note.xColumn;
        /*
        switch (note.LineLayer)
        {
            case 0:
                point = note.LineIndex;
                break;
            case 1:
                point = note.LineIndex + 4;
                break;
            case 2:
                point = note.LineIndex + 8;
                break;
            default:
                break;
        }
        */

        int offset = 0;
        if (note.CutDirection == CutDirection.ANY)
        {
            // the nondirection cubes are stored at the index+2 in the array
            offset = 2;
        }

        GameObject cube = Instantiate(Cubes[(int)note.NoteColorType + offset], SpawnPoints[point]);

        //cube.transform.localPosition = Vector3.zero;

        float rotation = 0f;

        switch (note.CutDirection)
        {
            case CutDirection.UP:
                rotation = 0f;
                break;
            case CutDirection.DOWN:
                rotation = 180f;
                break;
            case CutDirection.LEFT:
                rotation = 270f;
                break;
            case CutDirection.RIGHT:
                rotation = 90f;
                break;
            case CutDirection.UPLEFT:
                rotation = 315f;
                break;
            case CutDirection.UPRIGHT:
                rotation = 45f;
                break;
            case CutDirection.DOWNLEFT:
                rotation = 225f;
                break;
            case CutDirection.DOWNRIGHT:
                rotation = 125f;
                break;
            case CutDirection.ANY:
                rotation = 0f;
                break;
            default:
                break;
        }



        var handling = cube.GetComponent<CubeHandling>();
        handling.AnticipationPosition = (BeatsConstants.ANTICIPATION_DURATION_SECONDS * BeatsConstants.ANTICIPATION_SPEED_METER_PER_SECONDS);
        handling.Speed = (float)beatSpeed;
        handling.WarmUpPosition = -beatWarmupTime * beatWarmupSpeed;
        handling.rotation = rotation;
        handling.target_x = cube.transform.position.x;
        handling.target_y = cube.transform.position.y;

        //cube.transform.Rotate(transform.forward, rotation);
        Vector3 cube_position = cube.transform.position;
        cube_position.z = BeatsConstants.ANTICIPATION_DURATION_SECONDS * BeatsConstants.ANTICIPATION_SPEED_METER_PER_SECONDS + BeatsConstants.WARM_UP_DURATION_SECONDS * BeatsConstants.WARM_UP_SPEED_METER_PER_SECONDS;
        cube_position.y = 20.0f;
        cube_position.x = 0;
        cube.transform.position = cube_position;
    }

    public void GenerateObstacle(Obstacle obstacle)
    {
        double WALL_THICKNESS = 0.5;

        double durationSeconds = 60 * (obstacle.Duration / BeatsPerMinute);

        GameObject wall = Instantiate(Wall, SpawnPoints[obstacle.LineIndex]);

        var wallHandling = wall.GetComponent<ObstacleHandling>();
        wallHandling.AnticipationPosition = (float)(-beatAnticipationTime * beatSpeed - BeatsConstants.SWORD_OFFSET);
        wallHandling.Speed = (float)beatSpeed;
        wallHandling.WarmUpPosition = -beatWarmupTime * beatWarmupSpeed;
        wallHandling.Width = obstacle.Width * WALL_THICKNESS;
        wallHandling.Ceiling = obstacle.Type == ObstacleType.CEILING;
        wallHandling.Duration = obstacle.Duration;

        //wall.transform.localScale = new Vector3((float)wallHandling.Width, wall.transform.localScale.y, wall.transform.localScale.z);
    }
    /*
    public class Note
    {
        public double Time { get; set; }
        public double TimeInSeconds { get; set; }
        public int LineIndex { get; set; }
        public int LineLayer { get; set; }
        public NoteType Hand { get; set; }
        public CutDirection CutDirection { get; set; }

        public override bool Equals(object obj)
        {
            return Time == ((Note)obj).Time && LineIndex == ((Note)obj).LineIndex && LineLayer == ((Note)obj).LineLayer;
        }

        public override int GetHashCode()
        {
            var hashCode = -702342995;
            hashCode = hashCode * -1521134295 + Time.GetHashCode();
            hashCode = hashCode * -1521134295 + TimeInSeconds.GetHashCode();
            hashCode = hashCode * -1521134295 + LineIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + LineLayer.GetHashCode();
            hashCode = hashCode * -1521134295 + Hand.GetHashCode();
            hashCode = hashCode * -1521134295 + CutDirection.GetHashCode();
            return hashCode;
        }
    }

    public enum NoteType
    {
        LEFT = 0,
        RIGHT = 1
    }

    public enum CutDirection
    {
        TOP = 1,
        BOTTOM = 0,
        LEFT = 2,
        RIGHT = 3,
        TOPLEFT = 6,
        TOPRIGHT = 7,
        BOTTOMLEFT = 4,
        BOTTOMRIGHT = 5,
        NONDIRECTION = 8
    }

    */
    public class Obstacle
    {
        internal double TimeInSeconds;
        internal double Time;
        internal int LineIndex;
        internal double Duration;
        internal ObstacleType Type;
        internal double Width;
    }

    public enum ObstacleType
    {
        WALL = 0,
        CEILING = 1
    }
}



