using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileskinManager : MonoBehaviour
{
    public SkinScriptableObject[] availableSkins;
    public GameObject UI_SkinPrefab;
    public GameObject skinPanelPrefab;

    private int currentSkinIndex = 0;
    private GameObject popup;

    private void Start()
    {
        availableSkins = Resources.LoadAll<SkinScriptableObject>("ScriptableObjects/SkinInfo");
        currentSkinIndex = PlayerPrefs.GetInt("TILE_SKIN", 0);
        PlayerPrefs.SetInt("HAVE_SKIN_0", 1);
        popup = null;

        // TODO
        // 스킨 정보 불러올때, 선택가능/선택됨/잠김 보여주기
    }

    public void ShowPopupOnClick()
    {
        if (popup == null)
        {
            popup = Instantiate(UI_SkinPrefab, Managers.UI.Root.transform.GetChild(0));
            //popup.transform.localScale *= (Screen.width / 700f);
            SetPanels();
            popup.transform.GetChild(0).GetChild(2).GetComponent<Button>().onClick.AddListener(ClosePopupOnClick);
            Managers.Sound.Play("select");
        }
    }

    public void ClosePopupOnClick()
    {
        if (popup != null)
        {
            ApplyCurrentSkinToTiles();
            Managers.Sound.Play("undo2");
            Destroy(popup);
            popup = null;
        }
    }

    private void SetPanels()
    {
        for (int i = 0 ; i < availableSkins.Length; i++)
        {
            var panel = Instantiate(skinPanelPrefab, GameObject.Find("SkinGrid").transform);
            panel.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = availableSkins[i].skinImage;
            int skinIndex = i;
            if (PlayerPrefs.GetInt($"HAVE_SKIN_{skinIndex}", 0) == 1)
            {
                panel.transform.GetComponentInChildren<Button>().interactable = true;
            }
            else
            {
                panel.transform.GetComponentInChildren<Button>().interactable = false;
                panel.transform.GetComponentInChildren<TextMeshProUGUI>().text = $"STAGE {i * 50}";
            }
            panel.transform.GetComponentInChildren<Button>().onClick.AddListener(() => SetCurrentSkin(skinIndex));
        }
    }

    
    public void SetCurrentSkin(int skinIndex)
    {
        if (skinIndex >= 0 && skinIndex < availableSkins.Length)
        {
            currentSkinIndex = skinIndex;
            PlayerPrefs.SetInt("TILE_SKIN", currentSkinIndex);
            PlayerPrefs.Save();
            Managers.Sound.Play("flip4");
        }
    }

    private void ApplyCurrentSkinToTiles()
    {
        PuzzleManager.Instance.ChangeTileSkin(currentSkinIndex);
    }
}