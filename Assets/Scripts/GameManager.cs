using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using System;
using Unity.VisualScripting;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    private struct FrameworkSupport {
        public FrameworkSupport (EthicalFrameworks.Frameworks framework)
        {
            this.framework = framework;
            this.correctResponses = 0;
        }
        public EthicalFrameworks.Frameworks framework;
        public int correctResponses;
    }
    const float DILEMMA_HEADER_DURATION = 10f;
    const float TIME_PER_WORD = 0.2f;
    const float WORD_BUFFER_TIME = 2f;
    const float SWITCH_FLIP_FIRERATE = 0.35f;

    public static GameManager singleton;

    public Transform playerObject;

    public Dilemma[] dilemmas;
    public EthicalFrameworks.Frameworks targettedFramework;
    public TMP_Text txt_DilemmaNumber;
    public TMP_Text txt_DilemmaTitle;
    public Animation anim_UIIntroAnimation;

    public TMP_Text txt_SubtitleSpeaker;
    public TMP_Text txt_SubtitleText;
    public Animation anim_Subtitle;
    public Animation anim_Switch;

    private LocalizedString localizedDialogue;
    private LocalizedString localizedGeneral;
    private Dilemma currentDilemma;
    private FrameworkSupport[] frameworkSupports;
    private Queue<LocalizedString> dialogueQueue;
    private float nextSwitchFire = 0f;
    private int dilemmaIndex = 0;
    private SwitchDirection direction = SwitchDirection.Right;
    private SwitchDirection decidedDirection;
    private bool switchChangeLocked = false;
    private bool dialogueActive = false;

    public SwitchDirection Direction { get => direction; }

    public enum SwitchDirection
    {
        Left,
        Right
    }
    private enum LocaleTables
    {
        General,
        Dialogue
    }

    public void Start()
    {
        singleton = this;

        InitializeFrameworkSupportTracker();

        InitializeLocalization();

        StartCoroutine (InitiateDilemma(dilemmaIndex));
    }

    private void InitializeFrameworkSupportTracker ()
    {
        frameworkSupports = new FrameworkSupport[Enum.GetNames (typeof (EthicalFrameworks.Frameworks)).Length];
        int i = 0;

        foreach (var name in Enum.GetValues(typeof (EthicalFrameworks.Frameworks)))
        {
            EthicalFrameworks.Frameworks framework = (EthicalFrameworks.Frameworks)name;
            frameworkSupports[i] = new FrameworkSupport (framework);
            i++;
        }
    }

    private void InitializeLocalization ()
    {
        dialogueQueue = new Queue<LocalizedString>();

        localizedDialogue = new LocalizedString();
        localizedGeneral = new LocalizedString();

        localizedDialogue.TableReference = "Dialogue";
        localizedGeneral.TableReference = "General";
    }

    private IEnumerator InitiateDilemma(int dilemmaIndex)
    {
        if (dilemmaIndex >= dilemmas.Length)
        {
            EndGame();
            yield break;
        }
        Dilemma dilemma = dilemmas [dilemmaIndex];
        currentDilemma = dilemma;

        PerformDilemma (dilemmaIndex);

        yield return new WaitForSeconds (DILEMMA_HEADER_DURATION);

        QueueDialogue (dilemma.description);
    }
    private void EndGame ()
    {
        
    }
    public void FlipSwitch ()
    {
        if (switchChangeLocked)
        {
            switch (Direction)
            {
                case SwitchDirection.Left:
                    anim_Switch.Play ("LeftLocked");
                    return;
                case SwitchDirection.Right:
                    anim_Switch.Play ("RightLocked");
                    return;
            }
        }

        if (Time.time > nextSwitchFire)
        {
            switch (Direction)
            {
                case SwitchDirection.Left:
                    anim_Switch.Play ("SwitchRight");
                    direction = SwitchDirection.Right;
                    break;
                case SwitchDirection.Right:
                    anim_Switch.Play ("SwitchLeft");
                    direction = SwitchDirection.Left;
                    break;
            }

            nextSwitchFire = Time.time + SWITCH_FLIP_FIRERATE;
        }
    }
    public SwitchDirection OnFinalTrack ()
    {
        switchChangeLocked = true;

        return Direction;
    }
    public void OffFinalTrack ()
    {
        decidedDirection = direction;
        switchChangeLocked = false;
    }
    public void OnKillTrack ()
    {
        
    }
    public void OffKillTrack ()
    {
        DecisionMade ();
    }
    private void DecisionMade ()
    {
        bool correctResponseDone = false;

        FrameworkResponse[] responses;
        if (decidedDirection == SwitchDirection.Left)
        {
            responses = currentDilemma.leftTrackSupport;
        }
        else
        {
            responses = currentDilemma.rightTrackSupport;
        }

        foreach (FrameworkResponse response in responses)
        {
            if (response.framework == targettedFramework)
            {
                correctResponseDone = true;
            }

            for (int i = 0; i < frameworkSupports.Length; i++)
            {
                if (frameworkSupports[i].framework == response.framework)
                {
                    frameworkSupports[i].correctResponses++;
                }
            }
        }

        if (correctResponseDone)
        {
            StartCoroutine (MadeCorrectDecision ());   
        }
        else
        {
            StartCoroutine (MadeIncorrectDecision ());   
        }

        dilemmaIndex++;
    }
    private IEnumerator MadeCorrectDecision ()
    {
        LocalizedString dialogueString = GetLocale (LocaleTables.General, "Error");

        if (decidedDirection == SwitchDirection.Left)
        {
            foreach (FrameworkResponse response in currentDilemma.leftTrackSupport)
            {
                if (response.framework == targettedFramework)
                {
                    dialogueString = response.correctText;
                    break;
                }
            }
        }
        else if (decidedDirection == SwitchDirection.Right)
        {
            foreach (FrameworkResponse response in currentDilemma.rightTrackSupport)
            {
                if (response.framework == targettedFramework)
                {
                    dialogueString = response.correctText;
                    break;
                }
            }
        }

        QueueDialogue (dialogueString);

        yield return new WaitUntil (() => !dialogueActive);

        StartCoroutine (InitiateDilemma (dilemmaIndex));
    }
    private IEnumerator MadeIncorrectDecision ()
    {
        LocalizedString dialogueString = GetLocale (LocaleTables.General, "Error");

        if (decidedDirection == SwitchDirection.Left)
        {
            foreach (FrameworkResponse response in currentDilemma.rightTrackSupport)
            {
                if (response.framework == targettedFramework)
                {
                    dialogueString = response.incorrectText;
                    break;
                }
            }
        }
        else if (decidedDirection == SwitchDirection.Right)
        {
            foreach (FrameworkResponse response in currentDilemma.leftTrackSupport)
            {
                if (response.framework == targettedFramework)
                {
                    dialogueString = response.incorrectText;
                    break;
                }
            }
        }

        QueueDialogue (dialogueString);

        yield return new WaitUntil (() => !dialogueActive);

        StartCoroutine (InitiateDilemma (dilemmaIndex));
    }
    private IEnumerator PerformDialogue ()
    {
        if (dialogueActive)
            yield break;
        
        dialogueActive = true;

        while (dialogueQueue.Count > 0)
        {
            LocalizedString dialogueLocaleString = dialogueQueue.Dequeue ();
            string dialogueString = dialogueLocaleString.GetLocalizedString ();

            txt_SubtitleSpeaker.text = GetLocaleString (LocaleTables.Dialogue, "TheConductor");
            txt_SubtitleText.text = dialogueString;
            anim_Subtitle.Play ("SubtitleIn");

            float subtitleDuration = GetSubtitleDuration (dialogueString);

            yield return new WaitForSeconds (subtitleDuration);
        
            anim_Subtitle.Play ("SubtitleOut");

            yield return new WaitForSeconds (0.5f);
        }
        
        dialogueActive = false;
    }
    private void PerformDilemma (int index)
    {
        txt_DilemmaNumber.text = String.Format ("{0} {1}", GetLocaleString (LocaleTables.General, "Dilemma"), index + 1);
        txt_DilemmaTitle.text = currentDilemma.title.GetLocalizedString();
        anim_UIIntroAnimation.Play();
    }
    private void QueueDialogue (LocalizedString dialogue)
    {
        dialogueQueue.Enqueue (dialogue);

        if (!dialogueActive)
        {
            StartCoroutine (PerformDialogue());
        }
    }
    private string GetLocaleString (LocaleTables table, string entry)
    {
        return GetLocale (table, entry).GetLocalizedString();
    }
    private LocalizedString GetLocale (LocaleTables table, string entry)
    {
        switch (table)
        {
            case LocaleTables.General:
                localizedGeneral.TableEntryReference = entry;
                return localizedGeneral;
            case LocaleTables.Dialogue:
                localizedDialogue.TableEntryReference = entry;
                return localizedDialogue;
            default:
                throw new LocaleNotFound (table.ToString(), entry);
        }
    }
    private float GetSubtitleDuration (string text)
    {
        string[] words = text.Split (" ");
        int wordCount = words.Length;

        float duration = wordCount * TIME_PER_WORD + WORD_BUFFER_TIME;

        return duration;
    }
}

public class LocaleNotFound : Exception
{
    public LocaleNotFound ()
    {
        
    }
    public LocaleNotFound (string message) : base (message)
    {}
    public LocaleNotFound (string table, string entry) 
    : base (String.Format ("Could not find entry of {0} in table {1}", entry, table))
    {}
}