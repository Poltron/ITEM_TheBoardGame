﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : UIPanel
{
    [Header("FR Settings")]
    [SerializeField]
    private string localButtonTextFR;
    [SerializeField]
    private string remoteButtonTextFR;
    [SerializeField]
    private string aiButtonTextFR;
    [SerializeField]
    private string aiChoiceTextFR;

    [Header("EN Settings")]
    [SerializeField]
    private string localButtonTextEN;
    [SerializeField]
    private string remoteButtonTextEN;
    [SerializeField]
    private string aiButtonTextEN;
    [SerializeField]
    private string aiChoiceTextEN;

    [Header("")]
    [SerializeField]
    private Text localButtonText;
    [SerializeField]
    private Text remoteButtonText;
    [SerializeField]
    private Text aiButtonText;
    [SerializeField]
    private TextMeshProUGUI aiChoiceText;

    [SerializeField]
    private Animator leftMenuAnimator;
    [SerializeField]
    private Animator rightMenuAnimator;
    [SerializeField]
    private Animator fbPanelAnimator;
    [SerializeField]
    private GameObject menuButtons;
    [SerializeField]
    private GameObject loginButtons;
    [SerializeField]
    private Animator aiChoiceButtons;
    [SerializeField]
    private GameObject waitingForPlayerPanel;
    [SerializeField]
    private AIPanel[] aiPanels;

    protected override void SetLanguageFR()
    {
        localButtonText.text = localButtonTextFR;
        remoteButtonText.text = remoteButtonTextFR;
        aiButtonText.text = aiButtonTextFR;
        aiChoiceText.text = aiChoiceTextFR;
    }

    protected override void SetLanguageEN()
    {
        localButtonText.text = localButtonTextEN;
        remoteButtonText.text = remoteButtonTextEN;
        aiButtonText.text = aiButtonTextEN;
        aiChoiceText.text = aiChoiceTextEN;
    }

    public void PopIn()
    {
        leftMenuAnimator.SetBool("bPopIn", true);
        rightMenuAnimator.SetBool("bPopIn", true);
    }

    public void PopOut()
    {
        leftMenuAnimator.SetBool("bPopIn", false);
        rightMenuAnimator.SetBool("bPopIn", false);
        aiChoiceButtons.SetBool("bPopIn", false);
        UIManager.Instance.DisplayWaitingForPlayerPanel(false);
    }

    public void ShowAIs(bool showed)
    {
        aiChoiceButtons.SetBool("bPopIn", showed);

        if (showed)
        {
            foreach (AIPanel aiPanel in aiPanels)
            {
                aiPanel.Refresh();
            }
        }
    }

    public void PlayLocalDuel()
    {
        Debug.Log("PlayLocalDuel");
        GameManager.Instance.StartLocalGame();
    }

    public void PlayRemoteDuel()
    {
        Debug.Log("PlayRemoteDuel");
        ShowAIs(false);
        GameManager.Instance.StartLookingForOpponent();
    }

    public void PlayVSAI()
    {
        Debug.Log("PlayVSAI");

        if (GameManager.Instance.GameState == GameState.LookingForPlayer)
            GameManager.Instance.StopLookingForOpponent();

        ShowAIs(true);
    }
}
