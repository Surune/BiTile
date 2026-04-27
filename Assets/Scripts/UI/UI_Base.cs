using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UI_Base : MonoBehaviour
{
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

    public abstract void Init();
    
    private void Awake()
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
                objects[i] = Utils.FindChild(go, names[i], true);
            else
                objects[i] = Utils.FindChild<T>(go, names[i], true);

            if (objects[i] == null)
                Debug.Log("Binding Failed : " + names[i]);
        }
        _objects.Add(typeof(T), objects);
    }
    
    protected void BindObject(Type type) { Bind<GameObject>(type);  }
    protected void BindButton(Type type) { Bind<Button>(type);  }

    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }

    protected GameObject GetGameObject(int idx) { return Get<GameObject>(idx); }
    protected Button GetButton(int idx) { return Get<Button>(idx); }
}
