using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TrolleyMovement : MonoBehaviour
{

    public TrainRail startRail;
    public float speed = 0.5f;
    public float rotationCatchupSpeed = 1f;

    private Rigidbody rb;
    private float t;
    private TrainRail currentRail;
    private GameManager.SwitchDirection direction;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (startRail == null)
            Debug.LogError ("Start Rail not assigned");

        currentRail = startRail;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentRail == null)
        {
            OutOfTrack();
            return;
        }

        float trackSpeedMultiplier = currentRail.speedMultiplier;
        t += speed * Time.deltaTime * trackSpeedMultiplier;

        if (t >= 1)
        {
            if (currentRail.HasAlternatePath)
            {
                if (direction == GameManager.SwitchDirection.Left)
                {
                    currentRail = currentRail.nextAlternateRail;
                }
                else
                {
                    currentRail = currentRail.nextRail;
                }

                OffSplitTrack();
            }
            else
            {
                currentRail = currentRail.nextRail;
            }
            
            t = 0;

            if (currentRail != null)
            {
                if (currentRail.HasAlternatePath)
                {
                    OnSplitTrack();
                }
            }
        }
        else
        {
            Vector3 newPos = Vector3.zero;
            Vector3 targetTangent = Vector3.zero;

            if (currentRail.alternatePath != null)
            {
                switch (direction)
                {
                    case GameManager.SwitchDirection.Left:
                        newPos = currentRail.alternatePath.EvaluatePosition (t);
                        targetTangent = currentRail.alternatePath.EvaluateTangent (t);
                        break;
                    case GameManager.SwitchDirection.Right:
                        newPos = currentRail.mainPath.EvaluatePosition (t);
                        targetTangent = currentRail.mainPath.EvaluateTangent (t);
                        break;
                }
            }
            else
            {
                newPos = currentRail.mainPath.EvaluatePosition (t);
                targetTangent = currentRail.mainPath.EvaluateTangent (t);
            }

            transform.position = newPos;

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetTangent), rotationCatchupSpeed * Time.deltaTime);

        }
    }
    void OnSplitTrack ()
    {
        direction = GameManager.singleton.OnFinalTrack();        
    }
    void OffSplitTrack ()
    {
        GameManager.singleton.OffFinalTrack();
    }
    void OutOfTrack ()
    {
        
    }
}
