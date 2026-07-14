using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class UI_TextWave : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float amplitude = 5f;
    [SerializeField] private float phaseOffset = 0.5f;
    [SerializeField] private float speed = 5f;

    private void OnValidate()
    {
        text = GetComponent<TMP_Text>();
    }

    private void LateUpdate()
    {
        text.ForceMeshUpdate();

        var textInfo = text.textInfo;
        var phase = Time.unscaledTime * speed;

        for (var characterIndex = 0; characterIndex < textInfo.characterCount; characterIndex++)
        {
            var characterInfo = textInfo.characterInfo[characterIndex];
            if (!characterInfo.isVisible)
            {
                continue;
            }

            var vertices = textInfo.meshInfo[characterInfo.materialReferenceIndex].vertices;
            var vertexIndex = characterInfo.vertexIndex;
            var offset = Mathf.Sin(phase + characterIndex * phaseOffset) * amplitude;

            vertices[vertexIndex].y += offset;
            vertices[vertexIndex + 1].y += offset;
            vertices[vertexIndex + 2].y += offset;
            vertices[vertexIndex + 3].y += offset;
        }

        text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}
