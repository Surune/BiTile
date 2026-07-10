using UnityEngine;

public class UI_DiagonalFlowBackground : MonoBehaviour
{
    [SerializeField] private float tileSize = 0.55f;
    [SerializeField] private float tileSpacing = 1.4f;
    [SerializeField] private float speed = 0.25f;
    [SerializeField] private float alpha = 0.08f;
    [SerializeField] private float rotationZ = -45f;

    private Sprite[] sprites;
    private Camera mainCamera;
    private Transform backgroundTransform;
    private Mesh mesh;
    private Material material;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private Color[] colors;
    private int[] triangles;
    private int columns;
    private int tileCount;
    private float lastOrthographicSize;
    private float lastAspect;
    private float backgroundSize;
    private float scroll;
    private bool isStarted;

    public void SetSprites(Sprite[] backgroundSprites)
    {
        sprites = backgroundSprites;
        if (!isStarted)
        {
            return;
        }
        
        ApplyMaterialTexture();
        RebuildMesh();
        LayoutTiles();
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
        background.layer = gameObject.layer;
        background.transform.SetParent(mainCamera.transform, false);
        backgroundTransform = background.transform;

        mesh = new Mesh();
        mesh.name = "Diagonal Flow Background Mesh";
        mesh.MarkDynamic();

        var meshFilter = background.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        material = new Material(Shader.Find("Sprites/Default"));
        var color = material.color;
        color.a = alpha;
        material.color = color;

        var meshRenderer = background.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

        ApplyMaterialTexture();
    }

    private void ApplyMaterialTexture()
    {
        material.mainTexture = sprites[0].texture;
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

        RebuildMesh();
        LayoutTiles();
    }

    private void RebuildMesh()
    {
        columns = Mathf.CeilToInt(backgroundSize / tileSpacing) + 3;
        tileCount = columns * columns;

        vertices = new Vector3[tileCount * 4];
        uvs = new Vector2[tileCount * 4];
        colors = new Color[tileCount * 4];
        triangles = new int[tileCount * 6];

        var color = Color.white;
        color.a = alpha;

        for (var i = 0; i < tileCount; i++)
        {
            var vertexIndex = i * 4;
            var triangleIndex = i * 6;
            colors[vertexIndex] = color;
            colors[vertexIndex + 1] = color;
            colors[vertexIndex + 2] = color;
            colors[vertexIndex + 3] = color;

            triangles[triangleIndex] = vertexIndex;
            triangles[triangleIndex + 1] = vertexIndex + 1;
            triangles[triangleIndex + 2] = vertexIndex + 2;
            triangles[triangleIndex + 3] = vertexIndex + 2;
            triangles[triangleIndex + 4] = vertexIndex + 1;
            triangles[triangleIndex + 5] = vertexIndex + 3;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.triangles = triangles;
    }

    private void LayoutTiles()
    {
        var startX = backgroundSize * -0.5f - tileSpacing;
        var startY = backgroundSize * 0.5f + tileSpacing;

        for (var i = 0; i < tileCount; i++)
        {
            var column = i % columns;
            var row = i / columns;
            var x = startX + column * tileSpacing - scroll;
            var y = startY - row * tileSpacing - scroll;
            var sprite = sprites[(row + column) % sprites.Length];
            SetTile(i, x, y, sprite);
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.RecalculateBounds();
    }

    private void SetTile(int index, float x, float y, Sprite sprite)
    {
        var vertexIndex = index * 4;
        var spriteBoundsSize = sprite.bounds.size;
        var spriteSize = Mathf.Max(spriteBoundsSize.x, spriteBoundsSize.y);
        var width = tileSize * spriteBoundsSize.x / spriteSize;
        var height = tileSize * spriteBoundsSize.y / spriteSize;
        var halfWidth = width * 0.5f;
        var halfHeight = height * 0.5f;

        vertices[vertexIndex] = Vector3.right * (x - halfWidth) + Vector3.up * (y - halfHeight);
        vertices[vertexIndex + 1] = Vector3.right * (x + halfWidth) + Vector3.up * (y - halfHeight);
        vertices[vertexIndex + 2] = Vector3.right * (x - halfWidth) + Vector3.up * (y + halfHeight);
        vertices[vertexIndex + 3] = Vector3.right * (x + halfWidth) + Vector3.up * (y + halfHeight);

        SetUv(vertexIndex, sprite);
    }

    private void SetUv(int vertexIndex, Sprite sprite)
    {
        var rect = sprite.textureRect;
        var texture = sprite.texture;
        var minX = rect.xMin / texture.width;
        var maxX = rect.xMax / texture.width;
        var minY = rect.yMin / texture.height;
        var maxY = rect.yMax / texture.height;

        uvs[vertexIndex] = Vector2.right * minX + Vector2.up * minY;
        uvs[vertexIndex + 1] = Vector2.right * maxX + Vector2.up * minY;
        uvs[vertexIndex + 2] = Vector2.right * minX + Vector2.up * maxY;
        uvs[vertexIndex + 3] = Vector2.right * maxX + Vector2.up * maxY;
    }

    private void OnDestroy()
    {
        Destroy(mesh);
        Destroy(material);
    }
}
