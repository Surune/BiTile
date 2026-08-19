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
        await Task.Delay(powerOnDelay.ToMilliseconds());
        logoImage.gameObject.SetActive(true);
        logoImage.color = new Color(1f, 1f, 1f, 0f);
        await logoImage.DOFade(1f, fadeInDuration).AsyncWaitForCompletion();
        await Task.Delay(powerOffDelay.ToMilliseconds());
        await logoImage.DOFade(0f, fadeOutDuration).AsyncWaitForCompletion();
        await Task.Delay(sceneMovementDelay.ToMilliseconds());
        await SceneManager.LoadSceneAsync(Definitions.LobbySceneName);
    }
}
