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
using System.Threading;
using UnityEngine.SceneManagement;

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
    const int COUNTDOWN_DURATION = 10;
    const int INTRO_DIALOGUE_COUNT = 12;
    const int INTRO_HIDE_SPEAKER_UNTIL_THIS_INDEX = 3;

    public static GameManager singleton;

    [Header("Tracks and Player")]
    public Transform playerObject;
    public TrainRail mainFirstTrack;
    public TrainRail mainFinalTrack;
    public TrainRail killTrack;
    public TrainRail victimTrack;

    [Header("Dilemmas")]
    public Dilemma[] dilemmas;
    public EthicalFrameworks.Frameworks targettedFramework;

    [Header("Intro Cinematic")]
    public Animation[] introAnimations;

    [Header ("UI Dilemma Intro")]
    public TMP_Text txt_DilemmaNumber;
    public TMP_Text txt_DilemmaTitle;
    public Animation anim_UIIntroAnimation;

    [Header ("UI Subtitles")]
    public TMP_Text txt_SubtitleSpeaker;
    public TMP_Text txt_SubtitleText;
    public Animation anim_Subtitle;
    public Animation anim_Switch;

    [Header ("UI End Game")]
    public GameObject ui_EndGame;
    public Animation anim_EndGame;
    public GameObject ui_StatBlockElement;
    public Transform ui_StatBlockParent;
    public Animation anim_FrameworkFollowed;
    public TMP_Text txt_FrameworkFollowedHeader;
    public TMP_Text txt_FrameworkFollowedCorrect;
    public TMP_Text txt_FrameworkFollowedIncorrect;

    [Header ("UI Timer")]
    public RectTransform slider_TimeRemainingProgressBar;
    public Animation anim_TimeRemainingProgressBar;

    private LocalizedString localizedDialogue;
    private LocalizedString localizedGeneral;
    private Dilemma currentDilemma;
    private FrameworkSupport[] frameworkSupports;
    private Queue<LocalizedString> dialogueQueue;
    private GameObject victimInstance;
    private float nextSwitchFire = 0f;
    private int dilemmaIndex = 0;
    private float timerProgress = 1f;
    private SwitchDirection direction = SwitchDirection.Right;
    private SwitchDirection decidedDirection;
    private bool runningIntro = false;
    private bool switchChangeLocked = false;
    private bool dialogueActive = false;
    private static Mutex mutexLock = new Mutex();

    public SwitchDirection Direction { get => direction; }
    public bool RunningIntro { get => runningIntro; }

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

        StartCoroutine (RunIntroCinematic());
    }

    private void InitializeFrameworkSupportTracker ()
    {
        int targetFrameworkIndex = PlayerPrefs.GetInt ("TargetFramework", 0);
        targettedFramework = (EthicalFrameworks.Frameworks)targetFrameworkIndex;

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
    }

    private IEnumerator RunIntroCinematic()
    {
        runningIntro = true;
        foreach (Animation anim in introAnimations)
        {
            anim.Play ();
        }

        yield return new WaitForSeconds (introAnimations[0].clip.length);

        for (int i = 1; i <= INTRO_DIALOGUE_COUNT; i++)
        {
            LocalizedString dialogueLocale = new LocalizedString();
            dialogueLocale = GetLocale (LocaleTables.Dialogue, "DialogueIntro" + i);
            QueueDialogue (dialogueLocale);
        }

        runningIntro = false;

        yield return new WaitUntil (() => !dialogueActive);

        StartCoroutine (InitiateDilemma (dilemmaIndex));
    }
    private IEnumerator InitiateDilemma(int dilemmaIndex)
    {
        if (dilemmaIndex >= dilemmas.Length)
        {
            StartCoroutine (EndGame());
            yield break;
        }
        Dilemma dilemma = dilemmas [dilemmaIndex];
        currentDilemma = dilemma;

        PerformDilemma (dilemmaIndex);

        yield return new WaitForSeconds (DILEMMA_HEADER_DURATION);

        QueueDialogues (dilemma.descriptions);

        yield return new WaitForSeconds (GetSubtitleDurationFromArray (dilemma.descriptions) / 3);

        StartCoroutine (StartCountdown());
    }
    private IEnumerator EndGame ()
    {
        ui_EndGame.SetActive (true);
        anim_EndGame.Play("EndGame");

        yield return new WaitForSeconds (anim_EndGame.GetClip("EndGame").length);

        FrameworkSupport targettedSupport = new FrameworkSupport();

        foreach (FrameworkSupport support in frameworkSupports)
        {
            if (support.framework == targettedFramework)
            {
                targettedSupport = support;
                break;
            }
        }

        txt_FrameworkFollowedHeader.text = "While Trying to Follow\n" + EthicalFrameworks.FrameworkToString (targettedFramework) + "\nYou Got:";
        txt_FrameworkFollowedCorrect.text = String.Format ("{0} Correct Decisions", targettedSupport.correctResponses.ToString());
        txt_FrameworkFollowedIncorrect.text = String.Format ("{0} Incorrect Decisions", dilemmas.Length - targettedSupport.correctResponses);
        anim_FrameworkFollowed.Play ("FrameworkFollowedIn");

        yield return new WaitForSeconds (anim_FrameworkFollowed.GetClip ("FrameworkFollowedIn").length / 1.5f);

        foreach (FrameworkSupport support in frameworkSupports)
        {
            GameObject instance = Instantiate (ui_StatBlockElement, ui_StatBlockParent);
            Animation anim_instanceAnim = instance.GetComponent<Animation>();
            TMP_Text txt_instanceFramework = instance.transform.Find ("Text").Find("Framework").GetComponent<TMP_Text>();
            TMP_Text txt_instanceCount = instance.transform.Find ("Text").Find("Count").GetComponent<TMP_Text>();

            txt_instanceFramework.text = EthicalFrameworks.FrameworkToString (support.framework);
            txt_instanceCount.text = support.correctResponses.ToString();

            anim_instanceAnim.Play ("StatBlockIn");

            float instanceAnimClipLength = anim_instanceAnim.GetClip ("StatBlockIn").length;
            yield return new WaitForSeconds (instanceAnimClipLength / 2);
        }

        yield return new WaitForSeconds (7f);

        SceneManager.LoadScene (0);
    }
    private IEnumerator StartCountdown ()
    {
        timerProgress = 1f;

        float start = Time.time;
        float end = start + COUNTDOWN_DURATION;

        anim_TimeRemainingProgressBar.Play ("TimerIn");

        while (timerProgress > 0)
        {
            float progress = (Time.time - start) / (end - start);
            timerProgress = Mathf.Lerp (1, 0, progress);
            slider_TimeRemainingProgressBar.transform.localScale = new Vector3 (timerProgress, 1, 1);
            yield return null;
        }

        anim_TimeRemainingProgressBar.Play ("TimerOut");

        TransferToSplitTrack ();
    }
    private void TransferToSplitTrack ()
    {
        mainFinalTrack.nextRail = killTrack;
    }
    public void FlipSwitch ()
    {
        if (runningIntro)
            return;
            
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

        if (victimInstance != null)
            Destroy (victimInstance);

        return Direction;
    }
    public void OffFinalTrack ()
    {
        decidedDirection = direction;
        switchChangeLocked = false;

        Transform killSpot = victimTrack.killArea;
        GameObject victims;

        if (decidedDirection == SwitchDirection.Left)
        {
            victims = currentDilemma.leftTrackKill;
        }
        else
        {
            victims = currentDilemma.rightTrackKill;
        }

        victimInstance = Instantiate (victims, killSpot.transform.position, killSpot.transform.rotation);
    }
    public void OnKillTrack ()
    {
        mainFinalTrack.nextRail = mainFirstTrack;
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
        LocalizedString[] dialogueStrings = new LocalizedString[] { GetLocale (LocaleTables.General, "Error") };

        if (decidedDirection == SwitchDirection.Left)
        {
            foreach (FrameworkResponse response in currentDilemma.leftTrackSupport)
            {
                if (response.framework == targettedFramework)
                {
                    dialogueStrings = response.correctTexts;
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
                    dialogueStrings = response.correctTexts;
                    break;
                }
            }
        }

        QueueDialogues (dialogueStrings);

        yield return new WaitUntil (() => !dialogueActive);

        StartCoroutine (InitiateDilemma (dilemmaIndex));
    }
    private IEnumerator MadeIncorrectDecision ()
    {
        LocalizedString[] dialogueStrings = new LocalizedString[] { GetLocale (LocaleTables.General, "Error") };

        if (decidedDirection == SwitchDirection.Left)
        {
            foreach (FrameworkResponse response in currentDilemma.rightTrackSupport)
            {
                if (response.framework == targettedFramework)
                {
                    dialogueStrings = response.incorrectTexts;
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
                    dialogueStrings = response.incorrectTexts;
                    break;
                }
            }
        }

        QueueDialogues (dialogueStrings);

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
            mutexLock.WaitOne ();
            LocalizedString dialogueLocaleString = dialogueQueue.Dequeue ();
            string dialogueString = dialogueLocaleString.GetLocalizedString ();
            mutexLock.ReleaseMutex();

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
        mutexLock.WaitOne();
        dialogueQueue.Enqueue (dialogue);
        mutexLock.ReleaseMutex();

        if (!dialogueActive)
        {
            StartCoroutine (PerformDialogue());
        }
    }
    private void QueueDialogues (LocalizedString[] dialogues)
    {
        mutexLock.WaitOne();
        foreach (LocalizedString dialogue in dialogues)
        {
            dialogueQueue.Enqueue (dialogue);
        }
        mutexLock.ReleaseMutex();

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
        localizedDialogue = new LocalizedString();
        localizedGeneral = new LocalizedString();

        localizedDialogue.TableReference = "Dialogue";
        localizedGeneral.TableReference = "General";

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
    private float GetSubtitleDurationFromArray (LocalizedString[] texts)
    {
        string masterString = "";
        int wordCount = 0;
        foreach (LocalizedString text in texts)
        {
            string localeString = text.GetLocalizedString();
            string[] words = localeString.Split (" ");
            masterString += words;
            wordCount += words.Length;
        }

        float duration = wordCount * TIME_PER_WORD + (WORD_BUFFER_TIME * texts.Length);

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