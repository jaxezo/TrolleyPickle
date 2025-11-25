using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using static EthicalFrameworks;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Dilemma", order = 1), System.Serializable]
public class Dilemma : ScriptableObject
{
    public LocalizedString title;
    public LocalizedString description;
    public FrameworkResponse[] rightTrackSupport;
    public FrameworkResponse[] leftTrackSupport;
}

[System.Serializable]
public class FrameworkResponse
{
    public EthicalFrameworks.Frameworks framework;
    public LocalizedString text;
}