using UnityEngine;

public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        if (typeof(T) != typeof(GameObject))
        {
            return Resources.Load<T>(path);
        }
        
        var name = path;
        var index = name.LastIndexOf('/');
        if (index >= 0)
        {
            name = name.Substring(index+1);
        }
        return Resources.Load<T>(name);
    }

    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        if(original == null)
        {
            Debug.Log($"failed to load prefab : {path}");
            return null;
        }

        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;

        return go;
    }

    public void Destroy(GameObject go, float time = 0)
    {
        if (go == null)
            return;

        Object.Destroy(go, time);
    }
}
