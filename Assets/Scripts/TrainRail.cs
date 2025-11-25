using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class TrainRail : MonoBehaviour
{
    public SplineContainer mainPath;
    public SplineContainer alternatePath;
    public TrainRail nextRail;
    public float speedMultiplier = 1;

    private bool hasAlternatePath;

    public bool HasAlternatePath { get => hasAlternatePath; }

    void Start()
    {
        if (alternatePath == null)
            hasAlternatePath = false;
        else
            hasAlternatePath = true;

        if (nextRail == null)
            Debug.LogWarning ("Next Rail not defined.");
    }
}
