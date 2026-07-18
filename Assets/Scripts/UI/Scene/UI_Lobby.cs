using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Lobby : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button startButton;
    [SerializeField] private Button wishlistButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_Text versionText;
    [SerializeField] private InputActionReference confirmAction;

    private bool isTransitioning;
    private InputAction confirmInputAction;

    private void Awake()
    {
        quitButton.onClick.AddListener(Application.Quit);
        startButton.onClick.AddListener(OnGameStart);
        wishlistButton.onClick.AddListener(OnWishlistButton);
        optionButton.onClick.AddListener(OnOptionButton);

        versionText.text = $"{Application.version}({BuildInfo.GitHash})";

        confirmInputAction = confirmAction.action.Clone();

        GameManager.Instance.Sound.PlayBGM(Definitions.SoundType.Lobby);
    }

    private void OnEnable()
    {
        confirmInputAction.performed += OnConfirmAction;
        confirmInputAction.Enable();
    }

    private void OnDisable()
    {
        confirmInputAction.performed -= OnConfirmAction;
        confirmInputAction.Disable();
    }

    private void OnConfirmAction(InputAction.CallbackContext context)
    {
        if (SceneManager.GetSceneByName(Definitions.OptionSceneName).isLoaded ||
            SceneManager.GetSceneByName(Definitions.StageSelectSceneName).isLoaded)
        {
            return;
        }

        OnGameStart();
    }

    private void OnGameStart()
    {
        if (isTransitioning)
        {
            return;
        }

        isTransitioning = true;
        canvasGroup.blocksRaycasts = false;

        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.GameStart);
        SceneManager.LoadScene(Definitions.ChapterSelectSceneName);
    }

    private void OnOptionButton()
    {
        if (isTransitioning || SceneManager.GetSceneByName(Definitions.OptionSceneName).isLoaded)
        {
            return;
        }

        isTransitioning = true;
        canvasGroup.blocksRaycasts = false;

        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
        var loadOperation = SceneManager.LoadSceneAsync(Definitions.OptionSceneName, LoadSceneMode.Additive);
        loadOperation.completed += _ => isTransitioning = false;
    }

    private void OnWishlistButton()
    {
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
        SteamManager.OpenStorePage();
    }

    public void RestoreAfterOptionsClose()
    {
        canvasGroup.blocksRaycasts = true;
        isTransitioning = false;
    }

    private void OnDestroy()
    {
        confirmInputAction.Dispose();
    }
}
