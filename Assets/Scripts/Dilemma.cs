using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using static EthicalFrameworks;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Dilemma", order = 1), System.Serializable]
public class Dilemma : ScriptableObject
{
    public LocalizedString title;
    public LocalizedString[] descriptions;
    public FrameworkResponse[] rightTrackSupport;
    public FrameworkResponse[] leftTrackSupport;
    public GameObject rightTrackKill;
    public GameObject leftTrackKill;
}

[System.Serializable]
public class FrameworkResponse
{
    public EthicalFrameworks.Frameworks framework;
    public LocalizedString[] correctTexts;
    public LocalizedString[] incorrectTexts;
    public LocalizedString alternateText;
}