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
    public TrainRail nextAlternateRail;
    public float speedMultiplier = 1;

    [SerializeField]
    private bool isKillTrack = false;
    public Transform killArea;
    private bool hasAlternatePath;

    public bool HasAlternatePath { get => hasAlternatePath; }
    public bool IsKillTrack { get => isKillTrack; }

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
