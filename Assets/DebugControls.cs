using System;
using System.Collections;
using System.Collections.Generic;
using ImprovedTimers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugControls : MonoBehaviour
{
    public InputReader input;

    CountdownTimer resetTimer;
    private bool canReset = true;

    private void Awake()
    {
        resetTimer = new CountdownTimer(2f);
        resetTimer.OnTimerStop += OnResetTimerStop;
        DontDestroyOnLoad(this);
    }
    void Start()
    {
        input.Exit += ExitGame;
        input.Reset += ResetScene;

    }
    private void OnResetTimerStop()
    {
        canReset = true;
    }
    void ResetScene(bool isButtonPressed)
    {
        if (isButtonPressed && canReset)
        {
            canReset = false;
            resetTimer.Start();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    void ExitGame(bool isButtonPressed)
    {
        if (isButtonPressed)
        {
            Application.Quit();
        }
    }
}
