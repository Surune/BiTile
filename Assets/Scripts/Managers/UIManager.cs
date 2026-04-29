using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    private int popupOrder = 10;

    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    
    public int loadStageNum = 1; // 스테이지 클릭시 로딩값
    
    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
            {
                root = new GameObject { name = "@UI_Root" };
            }

            return root;
        }
    }

    public T ShowPopupUI <T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }
        
        var go = Managers.Resource.Instantiate($"UI/Popup/{name}");
        go.GetOrAddComponent<Canvas>().sortingOrder = popupOrder;
        popupOrder += 10;
        
        var popup = Utils.GetOrAddComponent<T>(go);
        _popupStack.Push(popup);

        go.transform.SetParent(Root.transform); 

        return popup;
    }

    public void ClosePopupUI(UI_Popup popup)
    {
        if (_popupStack.Count == 0)
        {
            return;
        }

        if (_popupStack.Peek() != popup)
        {
            Debug.Log("close popup failed");
            return;
        }
        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
        {
            return;
        }
        
        var popup = _popupStack.Pop();
        Managers.Resource.Destroy(popup.gameObject);
    }
}
