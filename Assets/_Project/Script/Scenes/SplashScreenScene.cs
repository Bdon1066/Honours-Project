using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreenScene : MonoBehaviour
{
    public void LoadNextScene()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        SceneTransitioner.Instance.TransitionToNextScene();
    }
    public void ExitGame()
    {
       Application.Quit();
    }
   
}
