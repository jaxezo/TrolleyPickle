using System;
using System.Collections;
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
        ApplyTrolleyMovement();
    }
    void ApplyTrolleyMovement ()
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
            DetermineNextTrack ();
        }
        else
        {
            MoveTrolley ();
        }
    }
    void MoveTrolley ()
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

        if (Vector3.Distance (transform.rotation.eulerAngles, new Vector3 (targetTangent.x, targetTangent.y, targetTangent.z)) < 75)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetTangent), rotationCatchupSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation (targetTangent);
        }
    }

    void DetermineNextTrack ()
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
            if (currentRail.IsKillTrack)
            {
                OffKillTrack();
            }
            currentRail = currentRail.nextRail;
        }
        
        t = 0;

        if (currentRail != null)
        {
            if (currentRail.HasAlternatePath)
            {
                OnSplitTrack();
            }
            else if (currentRail.IsKillTrack)
            {
                OnKillTrack ();
            }
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
    void OnKillTrack ()
    {
        GameManager.singleton.OnKillTrack();
    }
    void OffKillTrack ()
    {
        GameManager.singleton.OffKillTrack ();
    }
    void OutOfTrack ()
    {
        Debug.LogError ("End of the line");   
    }
}
