using System.Collections;
using UnityEngine;
using static EthicalFrameworks;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Dilemma", order = 1), System.Serializable]
public class Dilemma : ScriptableObject
{
    public string title;
    public string description;
    public FrameworkResponse[] rightTrackSupport;
    public FrameworkResponse[] leftTrackSupport;
}

[System.Serializable]
public class FrameworkResponse
{
    public EthicalFrameworks.Frameworks framework;
    public string text;
}