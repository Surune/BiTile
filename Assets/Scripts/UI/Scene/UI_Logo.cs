using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Logo : MonoBehaviour
{
    [SerializeField] private Image logoImage;
    [SerializeField] private float powerOnDelay = 1.25f;
    [SerializeField] private float powerOffDelay = 2f;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private float sceneMovementDelay = 0.2f;

    private void Awake()
    {
        logoImage.gameObject.SetActive(false);
        _ = Display();
    }

    private async Task Display()
    {
        await Awaitable.WaitForSecondsAsync(powerOnDelay);
        logoImage.gameObject.SetActive(true);
        logoImage.color = new Color(1f, 1f, 1f, 0f);
        await logoImage.DOFade(1f, fadeInDuration).AsyncWaitForCompletion();
        await Awaitable.WaitForSecondsAsync(powerOffDelay);
        await logoImage.DOFade(0f, fadeOutDuration).AsyncWaitForCompletion();
        await Awaitable.WaitForSecondsAsync(sceneMovementDelay);
        await SceneManager.LoadSceneAsync(Definitions.LobbySceneName);
    }
}
