using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UI_Base : MonoBehaviour
{
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

    public abstract void Init();


    private void Start()
    {
        Init();
    }

    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);

        GameObject go = GameObject.Find("@UI_Root");
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];

        if (_objects.ContainsKey(typeof(T)))
            _objects.Remove(typeof(T));
        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
                objects[i] = Util.FindChild(go, names[i], true);
            else
                objects[i] = Util.FindChild<T>(go, names[i], true);

            if (objects[i] == null)
                Debug.Log("Binding Failed : " + names[i]);
        }
        _objects.Add(typeof(T), objects);
    }
    
    protected void BindObject(Type type) { Bind<GameObject>(type);  }
    protected void BindImage(Type type) { Bind<Image>(type);  }
    protected void BindText(Type type) { Bind<TextMeshProUGUI>(type);  }
    protected void BindButton(Type type) { Bind<Button>(type);  }

    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }

    protected GameObject GetGameObject(int idx) { return Get<GameObject>(idx); }
    protected TextMeshProUGUI GetText(int idx) { return Get<TextMeshProUGUI>(idx); }
    protected Button GetButton(int idx) { return Get<Button>(idx); }
    protected Image GetImage(int idx) { return Get<Image>(idx); }

    public static void BindEvent(GameObject go, Action<PointerEventData> action, Define.UIEvent type =Define.UIEvent.Click)
    {
        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);
        evt.OnClickHandler -= action;
        evt.OnClickHandler += action;
    }

}
