using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dm
{
    public class GameMachineState : MonoBehaviour
    {
        public static GameMachineState instance_;

        public static GameMachineState Instance
        {
            get
            {
                if (instance_ == null)
                {
                    // If the instance is null, find it in the scene or create a new GameObject
                    instance_ = FindObjectOfType<GameMachineState>();
                    if (instance_ == null)
                    {
                        // If still null, create a new GameObject and attach the singleton script
                        GameObject singletonObject = new GameObject("SongSettings");
                        instance_ = singletonObject.AddComponent<GameMachineState>();
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
            else
            {
                Instance = this;
            }
        }

        private GameState game_state;

        private GameMachineState() {
            game_state = GameState.GAME_ENTRY;
	    }

        public GameState State {
            get => game_state;
            private set => game_state = value;
	    }


        public void ChangeToChooseSongs() {
            State = GameState.CHOOSES_SONGS;
	    }

        public void ChangeToLoadingSongs() {
            State = GameState.LOADING_SONGS;
	    }

        public void ChangeToChooseDifficulty() {
            State = GameState.CHOSSES_DIFICULTY;
	    }

        public void ChangeToGameReady() {
            State = GameState.GAME_READY;
	    }

        public void ChangeToGameInProgress() {
            if (State == GameState.GAME_READY) {
                State = GameState.GMAE_IN_PROGRESS;
            } else {
                Debug.LogWarningFormat("ChangeToGameInProgress with a invalid GameState : {0}", State);
            }
        }

        public void ChangeToGameEntry() {
            State = GameState.GAME_ENTRY;
	    }

        public void ChangeToGameFinish() {
            State = GameState.GAME_FINISH;
	    }
    }
}
