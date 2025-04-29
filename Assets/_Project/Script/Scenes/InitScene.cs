using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitScene : MonoBehaviour
{
    void Start()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        
        SceneTransitioner.Instance.TransitionToNextScene();
    }
    
}
