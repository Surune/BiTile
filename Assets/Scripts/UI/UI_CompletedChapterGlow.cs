using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_CompletedChapterGlow : MonoBehaviour
{
    private static readonly int FlowOffsetId = Shader.PropertyToID("_FlowOffset");

    private readonly List<Renderer> renderers = new List<Renderer>();
    private MaterialPropertyBlock propertyBlock;
    private float flowSpeed;

    public void Init(Material glowMaterial, float flowSpeed)
    {
        this.flowSpeed = flowSpeed;
        propertyBlock = new MaterialPropertyBlock();

        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer.GetComponent<TMP_Text>())
            {
                continue;
            }

            var materials = renderer.sharedMaterials;
            for (var i = 0; i < materials.Length; i++)
            {
                materials[i] = glowMaterial;
            }
            renderer.sharedMaterials = materials;
            renderers.Add(renderer);
        }

        RefreshGlow();
    }

    private void Update()
    {
        RefreshGlow();
    }

    private void RefreshGlow()
    {
        var flowOffset = Mathf.Repeat(Time.time * flowSpeed, 1f);
        propertyBlock.SetFloat(FlowOffsetId, flowOffset);
        foreach (var renderer in renderers)
        {
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
