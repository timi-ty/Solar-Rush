using UnityEngine;
using System.Collections;

public class ColorWheelSegment : MonoBehaviour
{
    #region Properties
    public int colorId { get; private set; }
    #endregion

    public void Init(Color color, int colorId, Sprite graphic, float scale, Vector3 position, Quaternion rotation, 
        string name = "CW_Segment", Transform parent = null)
    {

        if(parent) transform.SetParent(parent);
        this.name = name;

        //Segment Visuals.
        SpriteRenderer segmentRenderer = gameObject.AddComponent<SpriteRenderer>();
        segmentRenderer.sprite = graphic;
        segmentRenderer.color = color;
        segmentRenderer.sortingOrder = 1;
        this.colorId = colorId;

        //Segment Transformation.
        transform.localScale = new Vector3(scale / transform.lossyScale.x,
                                                        scale / transform.lossyScale.y,
                                                        scale / transform.lossyScale.z);
        transform.localRotation = rotation;
        transform.localPosition = position;

        gameObject.AddComponent<PolygonCollider2D>().isTrigger = true;
    }

    public void Init(int colorId, Vector2[] colliderPath, Quaternion rotation, string name = "CW_Segment", Transform parent = null)
    {

        if (parent) transform.SetParent(parent);
        this.name = name;

        this.colorId = colorId;

        transform.localRotation = rotation;

        PolygonCollider2D polygonCollider2D = gameObject.AddComponent<PolygonCollider2D>();
        polygonCollider2D.isTrigger = true;
        polygonCollider2D.points = colliderPath;
    }
}
