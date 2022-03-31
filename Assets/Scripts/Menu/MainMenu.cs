using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private bool loaded;
    public Slider LoadingBar = null;
    public Image LoadingSplashScreen = null;

    public void PlayGame()
    {
        loaded = false;
        StartCoroutine(loadAsyncScene());
    }

    IEnumerator loadAsyncScene()
    {
        // start coroutine animation that fade in the SplashScreen for 1 sec
        StartCoroutine(
            AnimationSplashScreen((float v) =>
            {
                LoadingSplashScreen.color = new Color(1, 1, 1, v);
            }, 1.0f)
        );

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        // prevent Scene Activation after load
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        // 0.9 correspond to a Scene loaded but not displayed
        while (asyncLoad.progress < 0.9f)
        {
            LoadingBar.value = asyncLoad.progress;
            yield return null;
        }
        // wait 
        while (inProgress)
        {
            yield return null;
        }

        // make the loading slider bar to 100% for graphique aesthetic
        LoadingBar.value = 1f;
        // start coroutine animation that fade out the SplashScreen for 1 sec
        StartCoroutine(
            AnimationSplashScreen((float v) =>
            {
                LoadingSplashScreen.color = new Color(1, 1, 1, 1.0f - v);
            }, 1.0f)
        );

        // wait for SplashScreen to completely fade out 
        while (inProgress)
        {
            yield return null;
        }


        // Display the Scene
        asyncLoad.allowSceneActivation = true;

        loaded = true;
    }


    private bool inProgress = false;
    IEnumerator AnimationSplashScreen(System.Action<float> function, float time)
    {
        inProgress = true;

        float timePassed = 0;
        while (timePassed <= time)
        {
            function(timePassed);
            timePassed += Time.deltaTime;
            yield return null;
        }

        inProgress = false;
    }



    public void QuitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
}
