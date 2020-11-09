﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HUD : MonoBehaviour
{
    public bool enableDiscord = true;
    public static bool paused = false;
    public DiscordController discord;
    public Image stealthBarOne;
    public Image stealthBarTwo;
    [Range(0f, 1f)]
    public float stealthFill;
    public Animator stealthColor;
    public Animator stealthGroup;
    public Animator pauseMenuAnimator;

    public void Play()
    {

    }

    public void UpdateStealth()
    {
        stealthFill = PlayerMovementController.alertLevel;
        stealthBarOne.fillAmount = stealthFill;
        stealthBarTwo.fillAmount = stealthFill;
        if (stealthFill <= 0f)
        {
            stealthGroup.SetBool("Visible", false);
        }
        else
        {
            stealthGroup.SetBool("Visible", true);
        }
        if (stealthFill >= 0.8f)
        {
            stealthColor.SetBool("Alerted", true);
        }
        else if (stealthFill < 0.8f)
        {
            stealthColor.SetBool("Alerted", false);
        }
    }

    private void Update()
    {
        UpdateStealth();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused == false)
            {
                TogglePause(true);
            }
            else
            {
                TogglePause(false);
            }
        }
    }

    public void TogglePause(bool pauseToggle)
    {
        if (pauseToggle == true)
        {
            pauseMenuAnimator.SetBool("Paused", true);
            paused = true;
        }
        else
        {
            pauseMenuAnimator.SetBool("Paused", false);
            paused = false;
        }
    }

    public void Settings()
    {

    }
    /// <summary>
    /// Quits the game and closes discord (if present)
    /// </summary>
    public void QuitGame()
    {
        if (enableDiscord)
        {
            discord.DRPShutdown();
        }
        Debug.Log("Game Quit.");
        Application.Quit();
    }

    public void Restart()
    {
        SceneManager.LoadScene(2);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
