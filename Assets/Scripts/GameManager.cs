using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using System;
using Unity.VisualScripting;
using System.Diagnostics;

public class GameManager : MonoBehaviour
{
    const float DILEMMA_HEADER_DURATION = 10f;
    const float TIME_PER_WORD = 0.2f;
    const float WORD_BUFFER_TIME = 2f;
    const float SWITCH_FLIP_FIRERATE = 0.35f;

    public static GameManager singleton;
    public Dilemma[] dilemmas;
    public TMP_Text txt_DilemmaNumber;
    public TMP_Text txt_DilemmaTitle;
    public Animation anim_UIIntroAnimation;

    public TMP_Text txt_SubtitleSpeaker;
    public TMP_Text txt_SubtitleText;
    public Animation anim_Subtitle;
    public Animation anim_Switch;

    private LocalizedString localizedDialogue;
    private LocalizedString localizedGeneral;
    private float nextSwitchFire = 0f;
    private SwitchDirection direction = SwitchDirection.Right;
    private bool switchChangeLocked = false;

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

        localizedDialogue = new LocalizedString();
        localizedGeneral = new LocalizedString();

        localizedDialogue.TableReference = "Dialogue";
        localizedGeneral.TableReference = "General";

        StartCoroutine (InitiateDilemma(dilemmas[0]));
    }

    private IEnumerator InitiateDilemma(Dilemma dilemma)
    {
        txt_DilemmaNumber.text = String.Format ("{0} 1", GetLocaleString (LocaleTables.General, "Dilemma"));
        txt_DilemmaTitle.text = dilemma.title.GetLocalizedString();
        anim_UIIntroAnimation.Play();

        yield return new WaitForSeconds (DILEMMA_HEADER_DURATION);

        anim_Subtitle.Play("SubtitleIn");
        txt_SubtitleSpeaker.text = GetLocaleString (LocaleTables.Dialogue, "TheConductor");
        txt_SubtitleText.text = dilemma.description.GetLocalizedString();

        float subtitleDuration = GetSubtitleDuration (dilemma.description.GetLocalizedString());

        yield return new WaitForSeconds (subtitleDuration);
        
        anim_Subtitle.Play ("SubtitleOut");
    }
    private string GetLocaleString (LocaleTables table, string entry)
    {
        switch (table)
        {
            case LocaleTables.General:
                localizedGeneral.TableEntryReference = entry;
                return localizedGeneral.GetLocalizedString();
            case LocaleTables.Dialogue:
                localizedDialogue.TableEntryReference = entry;
                return localizedDialogue.GetLocalizedString();
            default:
                return "";
        }
    }
    private float GetSubtitleDuration (string text)
    {
        string[] words = text.Split (" ");
        int wordCount = words.Length;

        float duration = wordCount * TIME_PER_WORD + WORD_BUFFER_TIME;

        return duration;
    }
    public void FlipSwitch ()
    {
        if (switchChangeLocked)
            return;

        if (Time.time > nextSwitchFire)
        {
            switch (direction)
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

        return direction;
    }
    public void OffFinalTrack ()
    {
        switchChangeLocked = false;
    }
    private void PerformDialogue ()
    {
        
    }
}
