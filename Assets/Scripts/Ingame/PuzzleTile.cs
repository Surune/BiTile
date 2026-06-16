using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleTile : MonoBehaviour
{
    public int row;
    public int col;
    public char type;
    public char color;

    [SerializeField] private float rotationTime = 0.4f;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color blackColor;
    [SerializeField] private Color whiteColor;
    private PuzzleManager puzzleManager;
    private bool isAnimating;
    private float delay = 0f;
    private float delayInterval = 0.02f;

    public void Init(PuzzleManager instance, int row, int col, char type, char color)
    {
        puzzleManager = instance;
        this.row = row;
        this.col = col;
        this.type = type;
        this.color = color;
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
        
        if (gameObject.GetComponent<Outline>() != null)
        {
            Destroy(gameObject.GetComponent<Outline>());
        }

        switch (type)
        {
            case '.':
                var adjacentTask = ChangeAdjacentColors();
                puzzleManager.TileClicked();
                await adjacentTask;
                break;
            case '+':
                var crossTask = ChangeCrossColors();
                puzzleManager.TileClicked();
                await crossTask;
                break;
            case '*':
                var xcrossTask = ChangeXcrossColors();
                puzzleManager.TileClicked();
                await xcrossTask;
                break;
            case '!':
                await StartShake();
                GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Decline);
                break;
        }
    }

    private Task ChangeAdjacentColors()
    {
        delay = 0;
        var tasks = new List<Task>(9);
        QueueTileColorChange(tasks, row, col);
        QueueTileColorChange(tasks, row, col - 1);
        QueueTileColorChange(tasks, row + 1, col - 1);
        QueueTileColorChange(tasks, row + 1, col);
        QueueTileColorChange(tasks, row + 1, col + 1);
        QueueTileColorChange(tasks, row, col + 1);
        QueueTileColorChange(tasks, row - 1, col + 1);
        QueueTileColorChange(tasks, row - 1, col);
        QueueTileColorChange(tasks, row - 1, col - 1);
        return Task.WhenAll(tasks);
    }

    private Task ChangeCrossColors()
    {
        delay = 0;
        var tasks = new List<Task>(5);
        QueueTileColorChange(tasks, row, col);
        QueueTileColorChange(tasks, row, col - 1);
        QueueTileColorChange(tasks, row + 1, col);
        QueueTileColorChange(tasks, row - 1, col);
        QueueTileColorChange(tasks, row, col + 1);
        return Task.WhenAll(tasks);
    }

    private Task ChangeXcrossColors()
    {
        delay = 0;
        var tasks = new List<Task>(5);
        QueueTileColorChange(tasks, row, col);
        QueueTileColorChange(tasks, row - 1, col - 1);
        QueueTileColorChange(tasks, row - 1, col + 1);
        QueueTileColorChange(tasks, row + 1, col - 1);
        QueueTileColorChange(tasks, row + 1, col + 1);
        return Task.WhenAll(tasks);
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

    private void QueueTileColorChange(List<Task> tasks, int targetRow, int targetCol)
    {
        if (!puzzleManager.CanChangeTileColor(targetRow, targetCol))
        {
            return;
        }

        tasks.Add(puzzleManager.ChangeTileColor(targetRow, targetCol, delay));
        delay += delayInterval;
    }

    public async Task StartRotate(float delayTime = 0f)
    {
        isAnimating = true;
        await Task.Delay(Mathf.RoundToInt(delayTime * 1000f));
        transform.DORotate(Vector3.forward * 180, rotationTime).SetRelative(true);
        await Task.Delay(Mathf.RoundToInt(rotationTime * 1000f));
        isAnimating = false;
    }

    public async Task StartShake(float delayTime = 0f)
    {
        isAnimating = true;
        await Task.Delay(Mathf.RoundToInt(delayTime * 1000f));

        var originalPosition = transform.position;

        var shakeSequence = DOTween.Sequence();
        for (var i = 0; i < 4; i++)
        {
            shakeSequence.Append(transform.DOMoveX(originalPosition.x + 2f, 0.04f));
            shakeSequence.Append(transform.DOMoveX(originalPosition.x - 2f, 0.04f));
        }
        shakeSequence.Append(transform.DOMove(originalPosition, 0.04f));

        shakeSequence.Play();
        await Task.Delay(Mathf.RoundToInt(rotationTime * 1000f));
        isAnimating = false;
    }
}
