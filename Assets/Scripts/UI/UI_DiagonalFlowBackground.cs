using System.Collections.Generic;
using UnityEngine;

public class UI_DiagonalFlowBackground : MonoBehaviour
{
    [SerializeField] private float tileSize = 0.55f;
    [SerializeField] private float tileSpacing = 1.4f;
    [SerializeField] private float speed = 0.25f;
    [SerializeField] private float alpha = 0.08f;
    [SerializeField] private float rotationZ = -45f;

    private Sprite[] sprites;
    private readonly List<Transform> tiles = new List<Transform>();
    private readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    private Camera mainCamera;
    private Transform backgroundTransform;
    private float lastOrthographicSize;
    private float lastAspect;
    private float backgroundSize;
    private float scroll;
    private bool isStarted;

    public void SetSprites(Sprite[] backgroundSprites)
    {
        sprites = backgroundSprites;
        if (isStarted)
        {
            RebuildTiles();
            LayoutTiles();
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        CreateBackground();
        LayoutBackground();
        isStarted = true;
    }

    private void Update()
    {
        if (mainCamera.orthographicSize != lastOrthographicSize || mainCamera.aspect != lastAspect)
        {
            LayoutBackground();
        }

        scroll = Mathf.Repeat(scroll + Time.deltaTime * speed, tileSpacing);
        LayoutTiles();
    }

    private void CreateBackground()
    {
        var background = new GameObject("Diagonal Flow Background");
        background.name = "Diagonal Flow Background";
        background.layer = gameObject.layer;
        background.transform.SetParent(mainCamera.transform, false);

        backgroundTransform = background.transform;
    }

    private void LayoutBackground()
    {
        lastOrthographicSize = mainCamera.orthographicSize;
        lastAspect = mainCamera.aspect;

        var visibleHeight = lastOrthographicSize * 2f;
        var visibleWidth = visibleHeight * lastAspect;
        backgroundSize = Mathf.Max(visibleWidth, visibleHeight) * 1.8f;

        backgroundTransform.localPosition = Vector3.forward * (mainCamera.farClipPlane - 0.5f);
        backgroundTransform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);

        RebuildTiles();
        LayoutTiles();
    }

    private void RebuildTiles()
    {
        for (var i = tiles.Count - 1; i >= 0; i--)
        {
            Destroy(tiles[i].gameObject);
        }

        tiles.Clear();
        renderers.Clear();

        var columns = Mathf.CeilToInt(backgroundSize / tileSpacing) + 3;
        var count = columns * columns;
        for (var i = 0; i < count; i++)
        {
            tiles.Add(CreateTile());
        }
    }

    private Transform CreateTile()
    {
        var tile = new GameObject("Flow Tile");
        tile.name = "Flow Tile";
        tile.layer = gameObject.layer;
        tile.transform.SetParent(backgroundTransform, false);

        var spriteRenderer = tile.AddComponent<SpriteRenderer>();
        var color = Color.white;
        color.a = alpha;
        spriteRenderer.color = color;
        renderers.Add(spriteRenderer);
        return tile.transform;
    }

    private void LayoutTiles()
    {
        var columns = Mathf.CeilToInt(backgroundSize / tileSpacing) + 3;
        var startX = backgroundSize * -0.5f - tileSpacing;
        var startY = backgroundSize * 0.5f + tileSpacing;

        for (var i = 0; i < tiles.Count; i++)
        {
            var column = i % columns;
            var row = i / columns;
            var x = startX + column * tileSpacing - scroll;
            var y = startY - row * tileSpacing - scroll;
            tiles[i].localPosition = Vector3.right * x + Vector3.up * y;
            var sprite = sprites[(row + column) % sprites.Length];
            renderers[i].sprite = sprite;
            tiles[i].localScale = Vector3.one * GetSpriteScale(sprite);
        }
    }

    private float GetSpriteScale(Sprite sprite)
    {
        var spriteSize = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        return tileSize / spriteSize;
    }
}
