﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
namespace dm
{
    public class MainMenu : MonoBehaviour
    {
        public GameObject StartButton;
        public GameObject ExitButton;
        public GameObject SongChooser;
        public GameObject PanelAreYouSure;
        public GameObject LevelChooser;
        public GameObject LevelButtonTemplate;
        public GameObject Title;
        public GameObject NoSongsFound;

        public AudioSource SongPreview;
        public LoadSongInfos SongInfos;
        private SongSettings Songsettings;
        private SceneHandling SceneHandling;

        AudioClip PreviewAudioClip = null;
        bool PlayNewPreview = false;

        private void Awake()
        {
            Songsettings = SongSettings.Instance;
            SceneHandling = SceneHandling.Instance;
        }

        public void Start()
        {
            //ShowSongs();
        }

        public void ShowSongs()
        {
            if (GameMachineState.Instance.State == GameState.GAME_ENTRY ||
                GameMachineState.Instance.State == GameState.CHOOSES_SONGS)
            {
                GameMachineState.Instance.ChangeToChooseSongs();
                //TODO(junlinp): refactory to SearchLongs
                SongInfos.AddSong();
                if (SongInfos.AllSongs.Count == 0)
                {
                    Title.gameObject.SetActive(false);
                    NoSongsFound.gameObject.SetActive(true);
                    return;
                }
                Songsettings.CurrentSong = SongInfos.AllSongs[Songsettings.CurrentSongIndex];

                Title.gameObject.SetActive(false);
                PanelAreYouSure.gameObject.SetActive(false);
                LevelChooser.gameObject.SetActive(false);
                SongChooser.gameObject.SetActive(true);
                var song = SongInfos.GetCurrentSong();

                SongInfos.SongName.text = song.Name;
                SongInfos.Artist.text = song.AuthorName;
                SongInfos.BPM.text = song.BPM;
                SongInfos.Levels.text = song.Difficulties.Count.ToString();

                byte[] byteArray = File.ReadAllBytes(song.CoverImagePath);
                Texture2D sampleTexture = new Texture2D(2, 2);
                bool isLoaded = sampleTexture.LoadImage(byteArray);

                if (isLoaded)
                {
                    SongInfos.Cover.texture = sampleTexture;
                }

                StartCoroutine(PreviewSong(Songsettings.CurrentSong.AudioFilePath));
            }
            else
            {
                Debug.LogFormat("Invalid State : {0}", GameMachineState.Instance.State);
            }
        }

        public IEnumerator PreviewSong(string audioFilePath)
        {
            Debug.Log(Songsettings.CurrentSong.AudioFilePath);

            if (GameMachineState.Instance.State == GameState.CHOOSES_SONGS)
            {

                SongPreview.Stop();
                PreviewAudioClip = null;
                PlayNewPreview = true;

                yield return null;

                var downloadHandler = new DownloadHandlerAudioClip(Songsettings.CurrentSong.AudioFilePath, AudioType.OGGVORBIS);
                downloadHandler.compressed = false;
                downloadHandler.streamAudio = true;
                var uwr = new UnityWebRequest(
                        Songsettings.CurrentSong.AudioFilePath,
                    UnityWebRequest.kHttpVerbGET,
                    downloadHandler,
                    null);

                var request = uwr.SendWebRequest();
                while (!request.isDone)
                    yield return null;

                PreviewAudioClip = DownloadHandlerAudioClip.GetContent(uwr);
            }
        }

        private void FixedUpdate()
        {
            //  play audio at 40 seconds
            if (PreviewAudioClip != null && PlayNewPreview)
            {
                PlayNewPreview = false;
                SongPreview.Stop();
                SongPreview.clip = PreviewAudioClip;
                SongPreview.time = 40f;
                SongPreview.Play();
            }
        }

        public void NextSong()
        {

            if (GameMachineState.Instance.State == GameState.CHOOSES_SONGS)
            {
                var song = SongInfos.NextSong();
                SongInfos.SongName.text = song.Name;
                SongInfos.Artist.text = song.AuthorName;
                SongInfos.BPM.text = song.BPM;
                SongInfos.Levels.text = song.Difficulties.Count.ToString();

                byte[] byteArray = File.ReadAllBytes(song.CoverImagePath);
                Texture2D sampleTexture = new Texture2D(2, 2);
                bool isLoaded = sampleTexture.LoadImage(byteArray);

                if (isLoaded)
                {
                    SongInfos.Cover.texture = sampleTexture;
                }

                StartCoroutine(PreviewSong(Songsettings.CurrentSong.AudioFilePath));
            }
        }

        public void PreviousSong()
        {

            if (GameMachineState.Instance.State == GameState.CHOOSES_SONGS)
            {
                var song = SongInfos.PreviousSong();

                SongInfos.SongName.text = song.Name;
                SongInfos.Artist.text = song.AuthorName;
                SongInfos.BPM.text = song.BPM;
                SongInfos.Levels.text = song.Difficulties.Count.ToString();

                byte[] byteArray = File.ReadAllBytes(song.CoverImagePath);
                Texture2D sampleTexture = new Texture2D(2, 2);
                bool isLoaded = sampleTexture.LoadImage(byteArray);

                if (isLoaded)
                {
                    SongInfos.Cover.texture = sampleTexture;
                }

                StartCoroutine(PreviewSong(Songsettings.CurrentSong.AudioFilePath));
            }
        }

        public void ChooseDifficulty()
        {
            if (GameMachineState.Instance.State == GameState.CHOOSES_SONGS)
            {
                GameMachineState.Instance.ChangeToChooseDifficulty();
                SongPreview.Stop();
                var song = SongInfos.GetCurrentSong();
                if (song.Difficulties.Count > 1)
                {
                    foreach (var gameObj in LevelChooser.GetComponentsInChildren<Button>(true))
                    {
                        if (gameObj.gameObject.name == "ButtonTemplate")
                            continue;
                        Destroy(gameObj.gameObject);
                    }

                    SongChooser.gameObject.SetActive(false);
                    PanelAreYouSure.gameObject.SetActive(false);
                    LevelChooser.gameObject.SetActive(true);
                    var buttonsCreated = new List<GameObject>();
                    foreach (var difficulty in song.Difficulties)
                    {
                        var button = GameObject.Instantiate(LevelButtonTemplate, LevelChooser.transform);

                        button.GetComponentInChildren<TextMeshProUGUI>().text = difficulty;
                        button.GetComponentInChildren<Button>().onClick.AddListener(() => StartSceneWithDifficulty(difficulty));
                        button.SetActive(true);
                        buttonsCreated.Add(button);
                    }
                    switch (buttonsCreated.Count)
                    {
                        case 2:
                            buttonsCreated[0].GetComponent<RectTransform>().localPosition = new Vector3(-287, buttonsCreated[0].GetComponent<RectTransform>().localPosition.y);
                            buttonsCreated[1].GetComponent<RectTransform>().localPosition = new Vector3(287, buttonsCreated[1].GetComponent<RectTransform>().localPosition.y);
                            break;
                        case 3:
                            buttonsCreated[0].GetComponent<RectTransform>().localPosition = new Vector3(-287, buttonsCreated[0].GetComponent<RectTransform>().position.y);
                            buttonsCreated[1].GetComponent<RectTransform>().localPosition = new Vector3(0, buttonsCreated[1].GetComponent<RectTransform>().position.y);
                            buttonsCreated[2].GetComponent<RectTransform>().localPosition = new Vector3(287, buttonsCreated[2].GetComponent<RectTransform>().position.y);
                            break;
                        case 4:
                            buttonsCreated[0].GetComponent<RectTransform>().localPosition = new Vector3(-430, buttonsCreated[0].GetComponent<RectTransform>().localPosition.y);
                            buttonsCreated[1].GetComponent<RectTransform>().localPosition = new Vector3(-144, buttonsCreated[1].GetComponent<RectTransform>().localPosition.y);
                            buttonsCreated[2].GetComponent<RectTransform>().localPosition = new Vector3(144, buttonsCreated[2].GetComponent<RectTransform>().localPosition.y);
                            buttonsCreated[3].GetComponent<RectTransform>().localPosition = new Vector3(430, buttonsCreated[3].GetComponent<RectTransform>().localPosition.y);
                            break;
                        case 5:
                            buttonsCreated[0].GetComponent<RectTransform>().localPosition = new Vector3(-560, buttonsCreated[0].GetComponent<RectTransform>().localPosition.y);
                            buttonsCreated[1].GetComponent<RectTransform>().localPosition = new Vector3(-287, buttonsCreated[1].GetComponent<RectTransform>().localPosition.y);
                            buttonsCreated[2].GetComponent<RectTransform>().localPosition = new Vector3(0, buttonsCreated[2].GetComponent<RectTransform>().localPosition.y);
                            buttonsCreated[3].GetComponent<RectTransform>().localPosition = new Vector3(287, buttonsCreated[3].GetComponent<RectTransform>().localPosition.y);
                            buttonsCreated[4].GetComponent<RectTransform>().localPosition = new Vector3(560, buttonsCreated[4].GetComponent<RectTransform>().localPosition.y);
                            break;
                        case 6:
                            buttonsCreated[0].GetComponent<RectTransform>().localPosition = new Vector3(1000, buttonsCreated[0].GetComponent<RectTransform>().localPosition.y);
                            buttonsCreated[1].GetComponent<RectTransform>().localPosition = new Vector3(-560, buttonsCreated[1].GetComponent<RectTransform>().localPosition.y);
                            buttonsCreated[2].GetComponent<RectTransform>().localPosition = new Vector3(-287, buttonsCreated[2].GetComponent<RectTransform>().localPosition.y);
                            buttonsCreated[3].GetComponent<RectTransform>().localPosition = new Vector3(0, buttonsCreated[3].GetComponent<RectTransform>().localPosition.y);
                            buttonsCreated[4].GetComponent<RectTransform>().localPosition = new Vector3(287, buttonsCreated[4].GetComponent<RectTransform>().localPosition.y);
                            buttonsCreated[5].GetComponent<RectTransform>().localPosition = new Vector3(560, buttonsCreated[4].GetComponent<RectTransform>().localPosition.y);
                            break;
                        default:
                            Debug.LogWarningFormat("Unknow Difficuly Count : {0}", buttonsCreated.Count);
                            break;

                    }

                }
                else
                {
                    StartSceneWithDifficulty(song.Difficulties[0]);
                }
            }
        }

        private void StartSceneWithDifficulty(string difficulty)
        {
            if (GameMachineState.Instance.State == GameState.CHOSSES_DIFICULTY)
            {
                SongInfos.GetCurrentSong().SelectedDifficulty = difficulty;
                //StartCoroutine(LoadSongScene());

                SongChooser.SetActive(false);
                PanelAreYouSure.SetActive(false);
                LevelChooser.SetActive(false);
                Title.SetActive(false);
                StartButton.SetActive(false);
                ExitButton.SetActive(false);
                GameMachineState.Instance.ChangeToLoadingSongs();
            }
        }

        public void AreYouSure()
        {
            NoSongsFound.gameObject.SetActive(false);
            Title.gameObject.SetActive(false);
            SongChooser.gameObject.SetActive(false);
            LevelChooser.gameObject.SetActive(false);
            PanelAreYouSure.gameObject.SetActive(true);
        }

        public void No()
        {
            PanelAreYouSure.gameObject.SetActive(false);
            Title.gameObject.SetActive(true);
        }

        public void Yes()
        {
            Application.Quit();
        }

        public void Update()
        {
            if (GameMachineState.Instance.State == GameState.GAME_FINISH) {
                StartButton.SetActive(true);
                ExitButton.SetActive(true);
                GameMachineState.Instance.ChangeToGameEntry();
	        }
        }
    }
}
