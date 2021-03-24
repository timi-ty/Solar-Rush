using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ColorPool : ScriptableObject
{
    public List<Color> colors = new List<Color>();

#if UNITY_EDITOR
    [MenuItem("Assets/Create/Color Pool")]
    public static void CreateLocomotorSettings()
    {
        string path = EditorUtility.SaveFilePanelInProject("New Color Pool", "ColorPool", "Asset",
            "Save Color Pool", "Assets/Resources/Color Pools");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(CreateInstance<ColorPool>(), path);
    }
#endif
}