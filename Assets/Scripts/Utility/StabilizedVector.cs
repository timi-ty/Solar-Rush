using UnityEngine;
using System.Collections.Generic;

public class StabilizedVector
{
    #region Properties
    private Vector3 _rawValue;
    public Vector3 rawValue
    {
        get => _rawValue;
        set
        {
            _rawValue = value;
            Stabilize();
        }
    }
    public Vector3 stabilizedValue { get; set; }
    public Vector3 timeStabilizedValue { get; set; }
    public int historyLength { get; set; }
    /// <summary>
    /// The raw x value of the stabilized vector.
    /// </summary>
    public float x
    {
        get => _rawValue.x; 
        set
        {
            _rawValue.x = value;
            Stabilize();
        }
    }
    /// <summary>
    /// The raw y value of the stabilized vector.
    /// </summary>
    public float y
    {
        get => _rawValue.x; 
        set
        {
            _rawValue.x = value;
            Stabilize();
        }
    }
    /// <summary>
    /// The raw z value of the stabilized vector.
    /// </summary>
    public float z
    {
        get => _rawValue.x; 
        set
        {
            _rawValue.x = value;
            Stabilize();
        }
    }
    private bool useTimeStabilization;
    #endregion

    #region Worker Parameters
    private LinkedList<Vector3> valueHistory;
    private LinkedList<float> timeHistory;
    private Vector3 runningSum;
    #endregion

    /// <summary>
    /// Create an instance of a vector stabilized over n historical values.
    /// </summary>
    /// <param name="n">The number of historical values used to stabilize the vector.</param>
    public StabilizedVector(int n = 5)
    {
        historyLength = n;
        valueHistory = new LinkedList<Vector3>();
        timeHistory = new LinkedList<float>();
    }

    private void Stabilize()
    {
        if (valueHistory.Count >= historyLength)
        {
            runningSum -= valueHistory.Last.Value;
            valueHistory.RemoveLast();

            timeHistory.RemoveLast();
        }

        runningSum += rawValue;
        valueHistory.AddFirst(rawValue);

        timeHistory.AddFirst(Time.time);

        timeStabilizedValue = runningSum / (Time.time - timeHistory.Last.Value);

        //Debug.Log(runningSum);

        stabilizedValue = runningSum / valueHistory.Count;
    }

    /// <summary>
    /// Cleans the memory of the vector stabilizer to make it start fresh. 
    /// </summary>
    public void Reset()
    {
        rawValue = Vector3.zero;
        stabilizedValue = Vector3.zero;
        valueHistory = new LinkedList<Vector3>();
        timeHistory = new LinkedList<float>();
        runningSum = Vector3.zero;
    }
}
