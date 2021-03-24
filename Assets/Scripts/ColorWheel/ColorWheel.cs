using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorWheel : MonoBehaviour, IInputHandler
{
    #region Components
    private Animator mAnimator { get; set; }
    #endregion

    #region Inspector Parameters
    [Header("Movement Scheme")]
    [SerializeField]
    private bool usePhysics;
    [SerializeField]
    private float wheelDeceleration;
    [SerializeField]
    private float maxAgularVelocity;

    [Header("Control Scheme")]
    [SerializeField]
    private float controlSensitivity = 1.0f;
    [SerializeField]
    private ControlSide _controlSide;
    public ControlSide controlSide => _controlSide;

    [Header("Visual Scheme")]
    [SerializeReference]
    private  WheelConfig wheelConfig;
    public int segmentCount;
    public float radius;
    [HideInInspector]
    public bool useCustomGraphic;
    public List<Sprite> segmentGraphics;
    [HideInInspector]
    public float gapCompensation;
    [HideInInspector]
    public float wheelThickness;
    [HideInInspector]
    public float colliderRefinement;
    [HideInInspector]
    public Transform ringTransform;
    [HideInInspector]
    public float ringScale;
    [HideInInspector]
    public bool optimizeForCircularSegments;
    #endregion

    #region Worker Parameters
    private float angleBetweenSegments;
    private float wheelReferenceAngle;
    private float wheelAngle;
    private StabilizedVector wheelDeltaAngle;
    private float wheelAngularVelocity;
    private Vector3 wheelReferenceVector;
    private bool isUnderControl;
    private System.Action onWheelRefreshed;
    #endregion

    #region Unity Runtime
    void Start()
    {
        RefreshWheel();

        wheelDeltaAngle = new StabilizedVector(12);

        GameManager.RegisterColorWheel(this);

        mAnimator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        RotateWheel();

        FollowCentreEarth();
    }
    #endregion
    
    #region Wheel Setup
    public void RefreshWheel(bool animate = false, System.Action onWheelRefreshed = null)
    {
        this.onWheelRefreshed = onWheelRefreshed ?? this.onWheelRefreshed;

        if (animate)
        {
            isUnderControl = false;
            wheelAngularVelocity = 2000;
            mAnimator.SetTrigger("Shrink");
            return;
        }

        if (!HasNeededReferences())
        {
            Debug.LogWarning("Wheel could not be refreshed because some or all needed references were not assigned.");
            return;
        }

        ClearWheel();

        if (useCustomGraphic)
        {
            SetupWithSprites();
        }
        else
        {
            SetupWithShader();
        }

        if (ringTransform)
        {
            ringTransform.localScale = Vector3.one * radius * 2 * ringScale;
        }

        onWheelRefreshed?.Invoke();
    }

    public void FinishedShrinking()
    {
        RefreshWheel();
        mAnimator.SetTrigger("Grow");
    }

    public void FinishedGrowing()
    {
        onWheelRefreshed?.Invoke();
    }

    private void SetupWithSprites()
    {
        //Disable mesh renderer.
        if (GetComponent<Renderer>())
        {
            GetComponent<Renderer>().enabled = false;
        }

        transform.localScale = Vector3.one;

        //Set up parameters.
        angleBetweenSegments = 360.0f / segmentCount;
        Quaternion rotationBetweenSegments = Quaternion.AngleAxis(angleBetweenSegments, transform.forward);
        Quaternion halfRotationBetweenSegments = Quaternion.AngleAxis(angleBetweenSegments/2.0f, transform.forward);
        float graphicWidth = segmentGraphics[0].bounds.size.x;
        float requiredSegmentWidth = optimizeForCircularSegments ? Mathf.PI * radius / segmentCount: 
            2 * radius * Mathf.Sin(Mathf.Deg2Rad * angleBetweenSegments / 2);
        float requiredGraphicScale = requiredSegmentWidth / graphicWidth + gapCompensation;
        float scaledGraphicHeight = requiredGraphicScale * segmentGraphics[0].bounds.size.y;

        Vector3 segmentPosition = MathOps.RotateByQuaternion(
            new Vector3(radius - (optimizeForCircularSegments ? 0 : scaledGraphicHeight / 2), 0, 0), halfRotationBetweenSegments);
        Quaternion segmentRotation = Quaternion.AngleAxis(-90, transform.forward) * halfRotationBetweenSegments;

        //Create new wheel segments.
        for (int i = 0; i < segmentCount; i++)
        {
            //Segment Setup.
            GameObject segment = new GameObject();
            ColorWheelSegment cwSegment = segment.AddComponent<ColorWheelSegment>();
            cwSegment.Init(color: wheelConfig.GetWheelColor(i, out int colorId),
                           colorId: colorId,
                           graphic: segmentGraphics[i % segmentGraphics.Count],
                           scale: requiredGraphicScale,
                           position: segmentPosition,
                           rotation: segmentRotation,
                           name: "CW_Segment " + i.ToString(),
                           parent: transform);

            //Update Transformation Values.
            segmentRotation *= rotationBetweenSegments;
            segmentPosition = MathOps.RotateByQuaternion(segmentPosition, rotationBetweenSegments);
        }
    }

    private void SetupWithShader()
    {
        //Enable mesh renderer.
        if (GetComponent<Renderer>())
        {
            GetComponent<Renderer>().enabled = true;
        }
        else
        {
            Debug.LogWarning(this.name + " requires a quad and a " + nameof(Renderer) + " to be set up");
            return;
        }

        transform.localScale = 2 * radius * Vector3.one;

        Material material;
        if (Application.isPlaying)
        {
            material = GetComponent<Renderer>().material;
        }
        else
        {
            material = GetComponent<Renderer>().sharedMaterial;
        }
        

        Color[] mColors = wheelConfig.GetWheelColors();

        material.SetColorArray("_WheelColors", mColors);
        material.SetFloat("_SegmentAngleSpread", Mathf.PI * 2 / segmentCount); //Shader code uses radians for angles.
        material.SetFloat("_ColorCount", mColors.Length);
        material.SetFloat("_Thickness", wheelThickness);

        GenerateCollisionSegments();
    }

    private void GenerateCollisionSegments()
    {
        //Set up PolygonCollider path.
        float segmentAngleSpread = 360.0f / segmentCount;
        float angleDelta = segmentAngleSpread / (5.0f * colliderRefinement);
        float coveredAngle = 0;
        Vector2 outerRef = MathOps.RotateByQuaternion((Vector2.up * radius), Quaternion.AngleAxis(-segmentAngleSpread / 2, transform.forward));
        Vector2 innerRef = MathOps.RotateByQuaternion(Vector2.up * (radius - wheelThickness * transform.lossyScale.x / 2), 
            Quaternion.AngleAxis(segmentAngleSpread / 2, transform.forward));
        List<Vector2> colliderPath = new List<Vector2>();
        
        //outer
        while(coveredAngle < segmentAngleSpread)
        {
            if (coveredAngle >= segmentAngleSpread)
            {
                coveredAngle = segmentAngleSpread;
            }

            //Upper path.
            colliderPath.Add(MathOps.RotateByQuaternion(outerRef, Quaternion.AngleAxis(coveredAngle, transform.forward)));

            coveredAngle += angleDelta;
        }

        colliderPath.Add(MathOps.RotateByQuaternion(outerRef, Quaternion.AngleAxis(segmentAngleSpread, transform.forward)));

        coveredAngle = 0;

        //inner
        while (coveredAngle < segmentAngleSpread)
        {
            if (coveredAngle >= segmentAngleSpread)
            {
                coveredAngle = segmentAngleSpread;
            }

            //Lower path.
            colliderPath.Add(MathOps.RotateByQuaternion(innerRef, Quaternion.AngleAxis(-coveredAngle, transform.forward)));

            coveredAngle += angleDelta;
        }

        colliderPath.Add(MathOps.RotateByQuaternion(innerRef, Quaternion.AngleAxis(-segmentAngleSpread, transform.forward)));

        //Set up parameters.
        angleBetweenSegments = 360.0f / segmentCount;
        Quaternion rotationBetweenSegments = Quaternion.AngleAxis(angleBetweenSegments, transform.forward);
        Quaternion halfRotationBetweenSegments = Quaternion.AngleAxis(angleBetweenSegments / 2.0f, transform.forward);

        Vector3 segmentPosition = MathOps.RotateByQuaternion(
            new Vector3((radius - wheelThickness * transform.lossyScale.x / 4) / transform.lossyScale.x, 0, 0), halfRotationBetweenSegments);
        Quaternion segmentRotation = Quaternion.AngleAxis(-90, transform.forward) * halfRotationBetweenSegments;

        //Create new wheel segments.
        for (int i = 0; i < segmentCount; i++)
        {
            //Segment Setup.
            GameObject segment = new GameObject();
            ColorWheelSegment cwSegment = segment.AddComponent<ColorWheelSegment>();
            wheelConfig.GetWheelColor(i, out int colorId);
            cwSegment.Init(colorId: colorId,
                           colliderPath.ToArray(),
                           rotation: segmentRotation,
                           name: "CW_Segment " + i.ToString(),
                           parent: transform);

            //Update Transformation Values.
            segmentRotation *= rotationBetweenSegments;
            segmentPosition = MathOps.RotateByQuaternion(segmentPosition, rotationBetweenSegments);
        }
    }

    public void ClearWheel()
    {
        //Destroy all old wheel segments.
        int sCount = transform.childCount;
        for (int i = 0; i < sCount; i++)
        {
            ColorWheelSegment cwSegment = transform.GetChild(i).GetComponent<ColorWheelSegment>();
            if (Application.isPlaying && cwSegment)
                Destroy(transform.GetChild(i).gameObject);
            else if(cwSegment)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
                sCount--;
                i--;
            }
        }
    }

    private bool HasNeededReferences()
    {
        return (segmentGraphics.Count > 0 || !useCustomGraphic) && wheelConfig != null;
    }
    #endregion

    #region Wheel Control
    private void RotateWheel()
    {
        wheelDeltaAngle.x = (wheelAngle - MathOps.RemapAngle(transform.eulerAngles.z));

        transform.rotation = Quaternion.AngleAxis(wheelAngle, transform.forward);

        if (!isUnderControl)
        {
            wheelAngularVelocity = Mathf.Clamp(Mathf.Abs(wheelAngularVelocity) - wheelDeceleration * 
                Time.fixedDeltaTime, 0, maxAgularVelocity) * (wheelAngularVelocity > 0 ? 1 : -1);
        }

        if (usePhysics)
        {
            wheelAngle += wheelAngularVelocity * Time.deltaTime;

            wheelAngle = MathOps.RemapAngle(wheelAngle);
        }
    }

    private void UpdateReferenceRotation()
    {
        wheelReferenceVector = MathOps.RotateByQuaternion(Vector3.up, Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0));
        wheelReferenceAngle = Vector3.SignedAngle(wheelReferenceVector, transform.up, transform.forward);
    }

    private void FollowCentreEarth()
    {
        transform.position = GameManager.centreEarth.transform.position;
    }
    #endregion

    #region Input Handler Interface
    public void OnInputStart(ControlSide controlSide)
    {
        if (!IsMyControlSide(controlSide)) return;

        UpdateReferenceRotation();
        wheelAngularVelocity = 0;
        wheelDeltaAngle.Reset();
        isUnderControl = true;
    }
    public void OnRecieveInput(float input, ControlSide controlSide)
    {
        if (!isUnderControl || !IsMyControlSide(controlSide)) return;

        wheelAngle = controlSensitivity * input * 360;

        wheelAngle += wheelReferenceAngle;

        wheelAngle = MathOps.RemapAngle(wheelAngle);
    }

    public void OnInputFinished(ControlSide controlSide)
    {
        if (!IsMyControlSide(controlSide)) return;

        wheelAngularVelocity = wheelDeltaAngle.timeStabilizedValue.x;

        wheelAngularVelocity = Mathf.Min(wheelAngularVelocity, maxAgularVelocity);

        isUnderControl = false;
    }
    #endregion

    #region Utility Methods
    public List<SpawnInfo> GetBeatableMultiSpawn(int spawnCount)
    {
        spawnCount = Mathf.Min(spawnCount, segmentCount);

        List<SpawnInfo> multiSpawnInfo = new List<SpawnInfo>();

        int maxAvailableJump = segmentCount - spawnCount;

        float nextAngle = Random.Range(0, 360);
        int nextColorIdIndex = Random.Range(0, segmentCount);

        for (int i = 0; i < spawnCount; i++)
        {
            multiSpawnInfo.Add(new SpawnInfo(wheelConfig.wheelColors[(nextColorIdIndex % segmentCount) % wheelConfig.wheelColors.Count].colorId, nextAngle));

            int jump = Random.Range(0, maxAvailableJump + 1);
            maxAvailableJump -= jump;
            nextAngle += angleBetweenSegments * (1 + jump);
            nextColorIdIndex += 1 + jump;
        }

        return multiSpawnInfo;
    }

    public int GetRandomColorId()
    {
        int cid = wheelConfig.wheelColors[Random.Range(0, segmentCount)%wheelConfig.wheelColors.Count].colorId;
        return cid;
    }

    private bool IsMyControlSide(ControlSide controlSide)
    {
        return this.controlSide == ControlSide.All || controlSide == this.controlSide;
    }
    #endregion
}
