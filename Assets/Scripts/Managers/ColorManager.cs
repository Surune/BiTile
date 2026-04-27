using UnityEngine;
using UnityEngine.UI;

public class ColorManager : MonoBehaviour
{
    public static ColorManager Instance;
    
    public Image backgroundImage;
    public Color tileColor;
    public int index;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    public void SetColor(int stage)
    {
        index = (stage - 1) / 35;
        backgroundImage.color = Colorset.backgroundColors[index];
        tileColor = Colorset.tileColors[index];
    }
}
