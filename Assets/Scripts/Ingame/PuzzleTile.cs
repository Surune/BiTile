using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class PuzzleTile : MonoBehaviour
{
    public int row;
    public int col;
    public char type;
    public char color;

    [SerializeField] private float rotationTime = 0.4f;
    [SerializeField] private Color blackColor;
    [SerializeField] private Color whiteColor;
    private TileScriptableObject tileInfo;
    private PuzzleManager puzzleManager;
    private MeshRenderer meshRenderer;
    private bool isHintVisible;
    private bool isAnimating;
    private const float DelayInterval = 0.02f;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    public void Init(PuzzleManager instance, int row, int col, char type, char color, TileScriptableObject tileInfo, Color tileColor)
    {
        puzzleManager = instance;
        this.tileInfo = tileInfo;
        this.row = row;
        this.col = col;
        this.type = type;
        this.color = color;
        blackColor = tileColor;
        
        var tileObject = Instantiate(tileInfo.model, transform);
        meshRenderer = tileObject.GetComponentInChildren<MeshRenderer>(); 
        RefreshColor();
    }
    
    private void OnMouseDown()
    {
        OnTileClick();
    }

    public async void OnTileClick()
    {
        if (isAnimating || !puzzleManager.IsClickable)
        {
            return;
        }
        
        HideHint();

        if (type == '!')
        {
            await StartShake();
            GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Decline);
            return;
        }

        puzzleManager.RecordUndoState();
        var changeTask = tileInfo.ChangeTiles(puzzleManager, row, col, DelayInterval);
        puzzleManager.TileClicked();
        await changeTask;
    }

    public async Task RefreshColorWithDelay(float delay)
    {
        await Task.Delay((delay + 0.1f).ToMilliseconds());
        RefreshColor();
    }

    private void RefreshColor()
    {
        switch (color)
        {
            case 'B':
                meshRenderer.material.color = blackColor;
                break;
            case 'W':
                meshRenderer.material.color = whiteColor;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(color));
        }
    }

    public void SetColor(char value)
    {
        color = value;
        RefreshColor();
    }

    public async Task SetColorWithDelay(char value, float delayTime)
    {
        await Task.Delay((delayTime + 0.1f).ToMilliseconds());
        SetColor(value);
    }

    public async Task StartRotate(float delayTime = 0f)
    {
        isAnimating = true;
        await Task.Delay(Mathf.RoundToInt(delayTime * 1000f));
        transform.DORotate(Vector3.forward * 180, rotationTime).SetRelative(true);
        await Task.Delay(Mathf.RoundToInt(rotationTime * 1000f));
        isAnimating = false;
    }

    public async Task StartUndoRotate(float delayTime = 0f)
    {
        isAnimating = true;
        await Task.Delay(Mathf.RoundToInt(delayTime * 1000f));
        transform.DORotate(Vector3.back * 180, rotationTime).SetRelative(true);
        await Task.Delay(Mathf.RoundToInt(rotationTime * 1000f));
        isAnimating = false;
    }

    public async Task StartShake(float delayTime = 0f)
    {
        isAnimating = true;
        await Task.Delay(delayTime.ToMilliseconds());

        var originalPos = transform.position;
        var shakeSequence = DOTween.Sequence();
        for (var i = 0; i < 4; i++)
        {
            shakeSequence.Append(transform.DOMoveX(originalPos.x + 0.05f, 0.04f));
            shakeSequence.Append(transform.DOMoveX(originalPos.x - 0.05f, 0.04f));
        }
        shakeSequence.Append(transform.DOMove(originalPos, 0.04f));
        shakeSequence.Play();
        
        await Task.Delay(rotationTime.ToMilliseconds());
        isAnimating = false;
    }

    public void ShowHint()
    {
        if (isHintVisible)
        {
            return;
        }

        meshRenderer.material.EnableKeyword("_EMISSION");
        meshRenderer.material.SetColor(EmissionColor, Color.red);
        isHintVisible = true;
    }

    public void HideHint()
    {
        if (isHintVisible)
        {
            meshRenderer.material.SetColor(EmissionColor, Color.black);
            meshRenderer.material.DisableKeyword("_EMISSION");
            isHintVisible = false;
        }
    }
}
