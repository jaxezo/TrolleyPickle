using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Dilemma[] dilemmas;
    public TMP_Text txt_DilemmaNumber;
    public TMP_Text txt_DilemmaTitle;
    public Animation anim_UIIntroAnimation;

    public void Start()
    {
        InitiateDilemma(dilemmas[0]);
    }

    private void InitiateDilemma(Dilemma dilemma)
    {
        Debug.Log(dilemma.title);
        txt_DilemmaNumber.text = "Dilemma 1";
        txt_DilemmaTitle.text = dilemma.title;
        anim_UIIntroAnimation.Play();
    }
}
