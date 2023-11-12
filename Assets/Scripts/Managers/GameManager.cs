using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Menu,
        Searching,
        Playing
    }

    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CurrentState = GameState.Menu;
    }

    public void TransitionState(GameState to)
    {
        Debug.Log($"Transitioning to state '{to}'");

        switch (to)
        {
            case GameState.Menu:
                break;
        }
    }
}
