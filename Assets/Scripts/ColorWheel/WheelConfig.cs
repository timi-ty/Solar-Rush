using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
public class WheelConfig : ScriptableObject
{
    public List<WheelColor> wheelColors = new List<WheelColor>();

    public Color GetWheelColor(int selector, out int colorId)
    {
        selector %= wheelColors.Count;
        colorId = wheelColors[selector].colorId;

        return GameManager.colorPool.colors[colorId];
    }

    public Color[] GetWheelColors()
    {
        Color[] colors = new Color[wheelColors.Count];
        for (int i = 0; i < colors.Length; i++)
        {
            int colorId = wheelColors[i].colorId;

            colors[i] = GameManager.colorPool.colors[colorId];
        }

        return colors;
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/Wheel Config")]
    public static void CreateLocomotorSettings()
    {
        string path = EditorUtility.SaveFilePanelInProject("New Wheel Config", "WheelConfig", "Asset",
            "Save Wheel Config", "Assets/Resources/Wheel Configs");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(CreateInstance<WheelConfig>(), path);
    }
#endif
}

[System.Serializable]
public class WheelColor
{
    [ColorId]
    public int colorId;
}
