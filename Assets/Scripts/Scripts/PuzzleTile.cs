using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PuzzleTile : MonoBehaviour
{
    public int row;
    public int col;
    public char type;
    public char color;
    public Image imageObject;
    public PuzzleManager puzzleManager;
    [HideInInspector] public bool isAnimating = false;
    
    private float delay = 0f;
    private float delayInterval = 0.02f;
    [SerializeField] private float rotationTime = 0.4f;

    public void OnTileClick()
    {
        if (!isAnimating && puzzleManager.clickable)
        {
            if (gameObject.GetComponent<Outline>() != null)
            {
                Destroy(gameObject.GetComponent<Outline>());
            }
            switch (type)
            {
                case '.':
                    ChangeAdjacentColors();
                    puzzleManager.TileClicked();
                    break;
                case '+':
                    ChangeCrossColors();
                    puzzleManager.TileClicked();
                    break;
                case '*':
                    ChangeXcrossColors();
                    puzzleManager.TileClicked();
                    break;
                case '!':
                    StartCoroutine(StartShake());
                    Managers.Sound.Play("decline");
                    break;
                default:
                    break;
            }
        }
    }

    void ChangeAdjacentColors()
    {
        delay = 0;
        delay += puzzleManager.ChangeTileColor(row , col, delay) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row, col - 1, delay) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row + 1, col - 1, delay) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row + 1, col, delay) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row + 1, col + 1, delay) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row, col + 1, delay) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row - 1, col + 1, delay) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row - 1, col, delay) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row - 1, col - 1, delay) ? delayInterval : 0;
    }

    void ChangeCrossColors()
    {
        delay = 0;
        delay += puzzleManager.ChangeTileColor(row , col) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row, col - 1) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row + 1, col) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row - 1, col) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row, col + 1) ? delayInterval : 0;
    }

    void ChangeXcrossColors()
    {
        delay = 0;
        delay += puzzleManager.ChangeTileColor(row, col) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row - 1, col - 1) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row - 1, col + 1) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row + 1, col - 1) ? delayInterval : 0;
        delay += puzzleManager.ChangeTileColor(row + 1, col + 1) ? delayInterval : 0;
    }
    
    public IEnumerator StartRotate(float delayTime = 0f)
    {
        isAnimating = true;
        yield return new WaitForSeconds(delayTime);
        transform.DORotate(new Vector3(0, 180, 0), rotationTime).SetRelative(true);

        yield return new WaitForSeconds(rotationTime);

        isAnimating = false;
    }

    public IEnumerator StartShake(float delayTime = 0f)
    {
        isAnimating = true;
        yield return new WaitForSeconds(delayTime);
        
        Vector3 originalPosition = transform.position;
        
        Sequence shakeSequence = DOTween.Sequence();
        for (int i = 0; i < 4; i++)
        {
            shakeSequence.Append(transform.DOMoveX(originalPosition.x + 2f, 0.04f));
            shakeSequence.Append(transform.DOMoveX(originalPosition.x - 2f, 0.04f));
        }
        shakeSequence.Append(transform.DOMove(originalPosition, 0.04f));
        
        shakeSequence.Play();
        yield return new WaitForSeconds(rotationTime);
        isAnimating = false;
    }
}
    