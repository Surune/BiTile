using UnityEngine;

public static class Utils
{
    public static T Load<T>(string path) where T : Object
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
    
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        if (component == null)
        {
            component = go.AddComponent<T>();
        }
        return component;
    }
    
    public static int ToMilliseconds(this float seconds)
    {
        return Mathf.RoundToInt(seconds * 1000f);
    }
}
