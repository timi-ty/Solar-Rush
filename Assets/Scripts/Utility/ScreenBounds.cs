//In Progress
using UnityEngine;
using System.Collections.Generic;

public enum Side { Left, Right}

/// <summary>
/// A utility class that interfaces the screen/viewport with 2D world coodinates.
/// </summary>
struct ScreenBounds
{
    #region Instance

    #region Properties
    /// <summary>
    /// The object is kept below this bound.
    /// </summary>
    public float topBoundary { get; private set; }
    /// <summary>
    /// The object is kept below this bound.
    /// </summary>
    public float bottomBoundary { get; private set; }
    /// <summary>
    /// The object is kept in front of this bound.
    /// </summary>
    public float leftBoundary { get; private set; }
    /// <summary>
    /// The object is kept behind this bound.
    /// </summary>
    public float rightBoundary { get; private set; }
    #endregion

    #region Constructor
    /// <summary>
    /// Create a new ScreenBounds object.
    /// </summary>
    /// <param name="top"></param>
    /// <param name="bottom"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    public ScreenBounds(float top, float left, float bottom, float right)
    {
        topBoundary = top;
        leftBoundary = left;
        bottomBoundary = bottom;
        rightBoundary = right;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Enforce this ScreenBounds on the <b>transform</b>.
    /// </summary>
    /// <param name="transform"></param>
    public void Enforce(Transform transform)
    {
        Vector2 position = transform.position;
        position.y = Mathf.Clamp(position.y, bottomBoundary, topBoundary);
        position.x = Mathf.Clamp(position.x, leftBoundary, rightBoundary);
        transform.position = position;
    }
    #endregion

    #endregion

    #region Static

    #region Properties
    public static Vector2 centre
    {
        get
        {
            return new Vector2(width/2, height/2) + min;
        }
    }
    public static Vector2 extents
    {
        get
        {
            return size / 2;
        }
    }
    public static Vector2 max
    {
        get
        {
            return Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, 0));
        }
    }
    public static Vector2 min
    {
        get
        {
            return Camera.main.ScreenToWorldPoint(Vector3.zero);
        }
    }
    public static Vector2 size
    {
        get
        {
            return new Vector2(width, height);
        }
    }
    public static float height
    {
        get
        {
            return max.y - min.y;
        }
    }
    public static float width
    {
        get
        {
            return max.x - min.x;
        }
    }
    public struct TopEdge
    {
        public Vector2 middle { get { return new Vector2(centre.x, max.y); } }
        public Vector2 leftVertex { get { return new Vector2(min.x, max.y); } }
        public Vector2 rightVertex { get { return max; } }
    }
    public static TopEdge topEdge { get; }
    public struct BottomEdge
    {
        public Vector2 middle { get { return new Vector2(centre.x, min.y); } }
        public Vector2 leftVertex { get { return min; } }
        public Vector2 rightVertex { get { return new Vector2(max.x, min.y); } }
    }
    public static BottomEdge bottomEdge { get; }
    public struct LeftEdge
    {
        public Vector2 middle { get { return new Vector2(min.x, centre.y); } }
        public Vector2 topVertex { get { return new Vector2(min.x, max.y); } }
        public Vector2 bottomVertex { get { return new Vector2(min.x, min.y); } }
    }
    public static LeftEdge leftEdge { get; }
    public struct RightEdge
    {
        public Vector2 middle { get { return new Vector2(max.x, centre.y); } }
        public Vector2 topVertex { get { return max; } }
        public Vector2 bottomVertex { get { return new Vector2(max.x, min.y); } }
    }
    public static RightEdge rightEdge { get; }
    #endregion

    #region Methods
    /// <summary>
    /// Creates edge colliders along the screen boundaries.
    /// </summary>
    /// <param name="top"></param>
    /// <param name="left"></param>
    /// <param name="bottom"></param>
    /// <param name="right"></param>
    public static void CreateColliders(Transform parent, bool top = true, bool bottom = true, bool left = true, bool right = true, 
        bool isTrigger = false, string tag = null)
    {
        GameObject colliderObject = new GameObject();

        if (parent) colliderObject.transform.SetParent(parent);

        colliderObject.name = "Screen Boundary";

        colliderObject.tag = tag ?? colliderObject.tag;

        colliderObject.layer = parent.gameObject.layer;

        colliderObject.transform.position = centre;

        if (top)
        {
            EdgeCollider2D upEdgeCollider = colliderObject.AddComponent<EdgeCollider2D>();
            List<Vector2> points = new List<Vector2>();
            points.Add(max);
            points.Add(new Vector2(min.x, max.y));
            upEdgeCollider.points = points.ToArray();
            upEdgeCollider.isTrigger = isTrigger;
        }

        if (left)
        {
            EdgeCollider2D leftEdgeCollider = colliderObject.AddComponent<EdgeCollider2D>();
            List<Vector2> points = new List<Vector2>();
            points.Add(new Vector2(min.x, max.y));
            points.Add(min);
            leftEdgeCollider.points = points.ToArray();
            leftEdgeCollider.isTrigger = isTrigger;
        }

        if (bottom)
        {
            EdgeCollider2D downEdgeCollider = colliderObject.AddComponent<EdgeCollider2D>();
            List<Vector2> points = new List<Vector2>();
            points.Add(min);
            points.Add(new Vector2(max.x, min.y));
            downEdgeCollider.points = points.ToArray();
            downEdgeCollider.isTrigger = isTrigger;
        }

        if (right)
        {
            EdgeCollider2D rightEdgeCollider = colliderObject.AddComponent<EdgeCollider2D>();
            List<Vector2> points = new List<Vector2>();
            points.Add(new Vector2(max.x, min.y));
            points.Add(max);
            rightEdgeCollider.points = points.ToArray();
            rightEdgeCollider.isTrigger = isTrigger;
        }
    }

    /// <summary>
    /// Returns a ScreenBounds object with the screen bounds needed for an object of extents <b>objectExtents</b>.
    /// </summary>
    /// <param name="objectExtents"></param>
    /// <returns></returns>
    public static ScreenBounds GetBoundaryFor(Vector2 objectExtents)
    {
        ScreenBounds screenBoundary = new ScreenBounds();

        Bounds screenBounds = new Bounds(centre, size);

        screenBoundary.topBoundary = screenBounds.max.y - objectExtents.y/2;
        screenBoundary.bottomBoundary = screenBounds.min.y + objectExtents.y/2;
        screenBoundary.leftBoundary = screenBounds.min.x + objectExtents.x/2;
        screenBoundary.rightBoundary = screenBounds.max.x - objectExtents.x/2;

        return screenBoundary;
    }

    /// <summary>
    /// A random X coordinate, within the screen bounds, out of <b>sampleSapce</b> possible points.
    /// </summary>
    /// <param name="sampleSpace"></param>
    /// <param name="index"></param>
    /// <returns>The X coordinate corresponding to the point chosen and writes the index of the point chosen to <b>index</b>.</returns>
    public static float RandomXCoord(int sampleSpace, out int index)
    {
        float startCoord = min.x;

        float spacing = (max.x - min.x) / (sampleSpace + 1);

        index = Random.Range(0, sampleSpace) + 1;

        return startCoord + index * spacing;
    }

    /// <summary>
    /// The exact X coordinate that corresponds to the given index, out of <b>sampleSapce</b> possible points.
    /// Where if index is between 1 (inclusive) and sampleSpace (inclusive) the returned coordinate will be within the screen bounds. 
    /// </summary>
    /// <param name="sampleSpace"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static float SpecificXCoord(int index, int sampleSpace)
    {
        float startCoord = min.x;

        float spacing = (max.x - min.x) / (sampleSpace + 1);

        return startCoord + index * spacing;
    }

    public static void ScaleToFitScreenWidth(Transform transform, Bounds bounds)
    {
        transform.localScale = Vector3.one;

        float scale = width / bounds.size.x;

        transform.localScale = Vector3.one * scale;
    }

    public static void ScaleToFitScreenHeight(Transform transform, Bounds bounds)
    {
        transform.localScale = Vector3.one;

        float scale = height / bounds.size.y;

        transform.localScale = Vector3.one * scale;
    }

    public static void AlignWithScreenBottom(Transform transform, Bounds bounds)
    {
        Vector2 position = transform.position;

        position.y = min.y + bounds.extents.y;

        transform.position = position;
    }

    /// <summary>
    /// Returns true only if the <b>bounds</b> is completely out of the playable area and should not be allowed to re-enter the playable area.
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    public static bool IsOutOfPlayableArea(Bounds bounds)
    {
        return bounds.max.y < min.y;
    }

    /// <summary>
    /// Returns true only if the <b>bounds</b> is completely encircled by the screen bounds.
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    public static bool IsInsideScreenBounds(Bounds bounds)
    {
        return bounds.max.y < max.y && bounds.min.y > min.y && bounds.max.x < max.x && bounds.min.x > min.x;
    }

    /// <summary>
    /// Returns true only if the <b>bounds</b> is completely out of the screen bounds.
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    public static bool IsOutsideScreenBounds(Bounds bounds)
    {
        return bounds.max.y < min.y || bounds.min.y > max.y || bounds.max.x < min.x || bounds.min.x > max.x;
    }

    /// <summary>
    /// Returns true only if the <b>bounds</b> is overlapping with one or more edges of the screen.
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    public static bool IsOverlappingScreenBounds(Bounds bounds)
    {
        return !IsInsideScreenBounds(bounds) && !IsOutsideScreenBounds(bounds);
    }
    #endregion

    #endregion
}