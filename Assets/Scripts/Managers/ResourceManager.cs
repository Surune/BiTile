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
}
