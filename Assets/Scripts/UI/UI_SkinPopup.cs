using UnityEngine;
using UnityEngine.UI;

public class UI_SkinPopup : MonoBehaviour
{
    [SerializeField] private SkinScriptableObject[] availableSkins;
    [SerializeField] private Transform skinGrid;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject skinPanelPrefab;

    private int currentSkinIndex;

    private void Awake()
    {
        currentSkinIndex = SaveManager.TileSkinIndex;

        for (var i = 0 ; i < availableSkins.Length; i++)
        {
            var panel = Instantiate(skinPanelPrefab, skinGrid).GetComponent<UI_SkinPanel>();
            panel.Init(availableSkins[i], i);
        }
        
        closeButton.onClick.AddListener(ClosePopupOnClick);
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Select);
    }
    
    private void ApplyCurrentSkinToTiles()
    {
        PuzzleManager.Instance.ChangeTileSkin(currentSkinIndex);
    }
    
    private void ClosePopupOnClick()
    {
        ApplyCurrentSkinToTiles();
        GameManager.Instance.Sound.PlaySFX(Definitions.SoundType.Undo);
        Destroy(gameObject);
    }
}
