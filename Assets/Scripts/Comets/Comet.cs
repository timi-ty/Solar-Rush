using UnityEngine;
using System.Collections;

public class Comet : MonoBehaviour
{
    #region Components
    [Header("Components")]
    private Rigidbody2D cometBody;
    private SpriteRenderer cometRenderer;
    private TrailRenderer cometTrail;
    public CometDebris destructionFX;
    public SpriteRenderer alertRenderer;
    public ParticleSystem trailPS;
    #endregion

    #region Move Settings
    [Header("Move Settings")]
    [SerializeField]
    private float speed;
    #endregion

    #region Worker Parameters
    private Vector2 velocity;
    private int screenBoundsMask;
    #endregion

    #region Properties
    public int nextColorId { get; set; }
    private bool _needsAlert;
    public bool needsAlert
    {
        get => _needsAlert; set
        {
            _needsAlert = value;
            alertRenderer.enabled = _needsAlert;
        }
    }
    private int colorId { get; set; }
    public CometManager cometManager { get; set; }
    private bool isDestroyed { get; set; }
    #endregion

    private void Start()
    {
        cometBody = GetComponent<Rigidbody2D>();
        cometRenderer = GetComponent<SpriteRenderer>();
        cometTrail = GetComponentInChildren<TrailRenderer>();

        velocity = (GameManager.centreEarth.transform.position - transform.position).normalized * speed;
        cometBody.velocity = velocity;

        UpdateColor();

        screenBoundsMask = LayerMask.GetMask("ScreenBounds");

        Invoke("DestroySelf", 120);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ColorWheelSegment cwSegment = collision.GetComponent<ColorWheelSegment>();

        if (cwSegment && cwSegment.colorId == colorId)
        {
            DestroySelf();
        }

        ScreenBoundsBehaviour screenBoundsBehaviour = collision.GetComponentInParent<ScreenBoundsBehaviour>();

        if (screenBoundsBehaviour)
        {
            needsAlert = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ColorWheelSegment cwSegment = collision.GetComponent<ColorWheelSegment>();

        if (cwSegment)
        {
            ColorWheel colorWheel = GetComponentInParent<ColorWheel>();
            ColorWheel nextColorWheel = GameManager.GetEnclosedWheel(colorWheel);
            nextColorId = nextColorWheel?.GetRandomColorId() ?? nextColorId;
            UpdateColor();
        }

        ScreenBoundsBehaviour screenBoundsBehaviour = collision.GetComponentInParent<ScreenBoundsBehaviour>();

        if (screenBoundsBehaviour)
        {
            needsAlert = false;

            Invoke("SelfDestructIfOut", 3.0f);
        }
    }

    private void SelfDestructIfOut()
    {
        if (!ScreenBounds.IsInsideScreenBounds(cometRenderer.bounds))
        {
            DestroySelf();
        }
    }

    private void Update()
    {
        if (!needsAlert) return;

        ShowAlert();
    }

    private void ShowAlert()
    {
        RaycastHit2D screenBoundHit = Physics2D.Raycast(transform.position, velocity, 200, screenBoundsMask);
        alertRenderer.transform.position = screenBoundHit.point;

        if (alertRenderer.bounds.max.y > ScreenBounds.topEdge.middle.y)
        {
            alertRenderer.transform.position += Vector3.down * alertRenderer.bounds.extents.y;
        }
        if (alertRenderer.bounds.min.y < ScreenBounds.bottomEdge.middle.y)
        {
            alertRenderer.transform.position += Vector3.up * alertRenderer.bounds.extents.y;
        }
        if (alertRenderer.bounds.max.x > ScreenBounds.rightEdge.middle.x)
        {
            alertRenderer.transform.position += Vector3.left * alertRenderer.bounds.extents.x;
        }
        if (alertRenderer.bounds.min.x < ScreenBounds.leftEdge.middle.x)
        {
            alertRenderer.transform.position += Vector3.right * alertRenderer.bounds.extents.x;
        }

        alertRenderer.transform.rotation = Quaternion.identity;
    }

    public void DestroySelf()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        cometManager.OnCometDestroyed();

        CometDebris debris = Instantiate(destructionFX, transform.position, transform.rotation);

        debris.Initialize(cometRenderer.color);

        Destroy(gameObject);
    }

    private void UpdateColor()
    {
        colorId = nextColorId;

        Color color = GameManager.colorPool.colors[colorId];

        cometRenderer.color = color;
        alertRenderer.color = color;

        cometTrail.startColor = new Color(color.r, color.g, color.b, 0.46f);
        cometTrail.endColor = color;

        Color.RGBToHSV(color, out float hue, out float sat, out float val);

        ParticleSystem.MainModule main = trailPS.main;

        ParticleSystem.MinMaxGradient startColor = new ParticleSystem.MinMaxGradient(Color.HSVToRGB(hue, sat, val), main.startColor.colorMax);
        main.startColor = startColor;
    }
}
