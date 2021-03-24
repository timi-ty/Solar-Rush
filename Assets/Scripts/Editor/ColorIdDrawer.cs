using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ColorIdAttribute))]
public class ColorIdDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!GameManager.colorPool)
        {
            Debug.LogWarning("You must assign a \"Color Pool\" to the active Game Manager");
            return;
        }

        Rect intRect = new Rect(position.x + 100, position.y, position.width - 100, position.height);
        Rect colorRect = new Rect(position.x, position.y, 100, position.height);

        if (property.propertyType == SerializedPropertyType.Integer)
        {
            EditorGUI.IntSlider(intRect, property, 0, GameManager.colorPool.colors.Count - 1, label);

            EditorGUI.ColorField(colorRect, GameManager.colorPool.colors[property.intValue]);
        }
        else
            EditorGUI.LabelField(position, label.text, "Use ColorId with an int.");
    }
}