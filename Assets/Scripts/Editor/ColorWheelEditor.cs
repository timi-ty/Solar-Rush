using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ColorWheel))]
public class ColorWheelEditor : Editor
{
    private ColorWheel targetWheel;

    private void OnEnable()
    {
        targetWheel = (ColorWheel) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        targetWheel.useCustomGraphic = GUILayout.Toggle(targetWheel.useCustomGraphic, "Use Custom Graphic");

        if (targetWheel.useCustomGraphic)
        {
            targetWheel.gapCompensation = EditorGUILayout.FloatField("Gap Compensation", targetWheel.gapCompensation);
            targetWheel.ringTransform = (Transform)EditorGUILayout.ObjectField("Ring Transform", targetWheel.ringTransform, typeof(Transform), true);
            targetWheel.ringScale = EditorGUILayout.FloatField("Ring Scale", targetWheel.ringScale);
            targetWheel.optimizeForCircularSegments = EditorGUILayout.Toggle("Optimize For Circular Segments", targetWheel.optimizeForCircularSegments);
        }
        else
        {
            targetWheel.wheelThickness = EditorGUILayout.FloatField("Wheel Thickness", targetWheel.wheelThickness);
            targetWheel.colliderRefinement = EditorGUILayout.FloatField("Collider Refinement", targetWheel.colliderRefinement);
        }


        if (GUILayout.Button("Refresh Touch Wheel"))
        {
            targetWheel.RefreshWheel();
        }

        if (GUILayout.Button("Clear Touch Wheel"))
        {
            targetWheel.ClearWheel();
        }
    }
}