using System;
using System.Collections;
using System.Collections.Generic;
using ImprovedTimers;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private void OnEnable()
    {
        resetTimer.OnTimerStop += OnResetTimerStop;
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
            if (SceneManager.GetActiveScene().buildIndex != 0 || SceneManager.GetActiveScene().buildIndex != 1)
            {
                canReset = false;
                resetTimer.Start();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
           
        }
    }
    void ExitGame(bool isButtonPressed)
    {
        if (isButtonPressed)
        {
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                Application.Quit();
            }
            else if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                SceneManager.LoadScene(1);
            }
        }
    }
}
