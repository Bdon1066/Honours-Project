using System;
using System.Collections;
using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : PersistentSingleton<SceneTransitioner>
{
   public float transitionDuration = 0.5f;
      
   public Animator transition;
   
   static readonly int StartHash = Animator.StringToHash("SceneStart");
   static readonly int EndHash = Animator.StringToHash("SceneEnd");
   
   private void Start()
   {
      transition.gameObject.SetActive(false);
   }

   public void TransitionToScene(string sceneName)
   {
      StartCoroutine(LoadScene(SceneManager.GetSceneByName(sceneName).buildIndex));
   }
   public void TransitionToScene(int sceneIndex)
   {
      StartCoroutine(LoadScene(sceneIndex));
   }
   public void TransitionToNextScene()
   {
      StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
   }

   IEnumerator LoadScene(int sceneIndex)
   {
      //play transition
      transition.gameObject.SetActive(true);
      transition.SetTrigger(EndHash);
      var scene = SceneManager.LoadSceneAsync(sceneIndex);
      scene.allowSceneActivation = false;
      
      yield return new WaitForSeconds(transitionDuration);
      
      scene.allowSceneActivation = true;
      transition.SetTrigger(StartHash);
   }
}
