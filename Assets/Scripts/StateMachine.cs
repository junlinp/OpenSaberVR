using UnityEngine;

public enum GameState
{
    ChooseSongs,
    LoadingSongs,
    GameReady,
    GameInPrograss,
    GameEnd,
}

public class GameStateMachine : MonoBehaviour {
    public static GameStateMachine instance_;

    public static GameStateMachine Instance {
        get {
            if (instance_ == null) {
                instance_ = FindObjectOfType<GameStateMachine>();
                if (instance_ == null) {
                    GameObject singletonObject_for_state = new GameObject("GameStateMachine");
                    instance_ = singletonObject_for_state.AddComponent<GameStateMachine>();
                }
            }
            return instance_;
        }
        private set { instance_ = value; }
    }
    private GameState game_state_ = GameState.ChooseSongs;

    public GameState GameState {
        get { return game_state_; }
        set { game_state_ = value;}
    }
}

