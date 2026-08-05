using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleLetterFlip : MonoBehaviour
{
    [SerializeField] private float duration = 0.35f;

    private Camera titleCamera;
    private Transform[] letters;
    private MeshRenderer[] renderers;
    private Material[] materials;
    private Color[] defaultColors;
    private bool[] isFlipped;
    private Color flippedColor;
    private bool isRotating;

    private void Awake()
    {
        titleCamera = GetComponentInChildren<Camera>();
        letters = GetComponentsInChildren<Transform>()
            .Where(child => child.name.StartsWith("TITLE_"))
            .ToArray();
        renderers = new MeshRenderer[letters.Length];
        materials = new Material[letters.Length];
        defaultColors = new Color[letters.Length];
        isFlipped = new bool[letters.Length];

        flippedColor = Color.white;
        flippedColor.g = 189f / 255f;
        flippedColor.b = 56f / 255f;

        for (var i = 0; i < letters.Length; i++)
        {
            renderers[i] = letters[i].GetComponentInChildren<MeshRenderer>();
            materials[i] = renderers[i].material;
            defaultColors[i] = materials[i].color;
        }
    }

    private void Update()
    {
        if (isRotating || SceneManager.GetSceneByName(Definitions.OptionSceneName).isLoaded ||
            !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        var ray = titleCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        var hitIndex = -1;
        var nearestDistance = float.PositiveInfinity;

        for (var i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i].bounds.IntersectRay(ray, out var distance) || distance >= nearestDistance)
                continue;

            hitIndex = i;
            nearestDistance = distance;
        }

        if (hitIndex < 0)
            return;

        isRotating = true;
        var nextFlipped = !isFlipped[hitIndex];
        var letter = letters[hitIndex];
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Flip_Base);
        DOTween.Sequence()
            .Append(letter
                .DOLocalRotate(Vector3.up * 180f, duration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutCubic))
            .InsertCallback(duration * 0.5f, () =>
            {
                isFlipped[hitIndex] = nextFlipped;
                materials[hitIndex].color = nextFlipped
                    ? flippedColor
                    : defaultColors[hitIndex];
            })
            .SetTarget(letter)
            .SetLink(gameObject)
            .OnComplete(() => isRotating = false);
    }

    private void OnDisable()
    {
        foreach (var t in letters)
            DOTween.Kill(t);

        isRotating = false;
    }
}
