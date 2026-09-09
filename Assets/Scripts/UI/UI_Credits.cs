using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Credits : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private InputActionReference backAction;
    
    private InputAction backInputAction;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        backInputAction = backAction.action.Clone();
    }

    private void OnEnable()
    {
        backInputAction.Enable();
    }

    private void OnDisable()
    {
        backInputAction.Disable();
    }

    private void Update()
    {
        if (backInputAction.WasPressedThisFrame())
        {
            Close();
        }
    }

    private void Close()
    {
        ReturnToGameStartScreen();
    }

    private void ReturnToGameStartScreen()
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
        if (gameObject.scene.name == Definitions.CreditsSceneName)
        {
            FindFirstObjectByType<UI_Lobby>().RestoreAfterOptionsClose();
            SceneManager.UnloadSceneAsync(Definitions.CreditsSceneName);
            return;
        }

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        backInputAction.Dispose();
    }
}
