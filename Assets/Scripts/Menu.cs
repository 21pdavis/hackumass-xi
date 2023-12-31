using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField]
    private Button StartButton, ExitButton;

    private void Start()
    {
        StartButton.onClick.AddListener(OnStartClick);
        ExitButton.onClick.AddListener(OnExitClick);
    }
    
    private void OnStartClick()
    {
        gameObject.SetActive(false);
        GameManager.Instance.TransitionState(GameManager.GameState.Playing);
    }

    private void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
