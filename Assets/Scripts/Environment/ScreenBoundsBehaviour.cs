using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenBoundsBehaviour : MonoBehaviour
{
    void Start()
    {
        ScreenBounds.CreateColliders(parent: transform, isTrigger: true);
    }
}
