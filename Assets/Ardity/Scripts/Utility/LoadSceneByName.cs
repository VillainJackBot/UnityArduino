using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoadSceneByName : MonoBehaviour
{
    public RawImage fadeImage;
    public UnityEvent OnSceneLoaded;

    // Unload all scenes except a scene named "Arduino"
    public void UnloadAllScenes()
    {
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (scene.name != "Arduino")
            {
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
            }
        }
    }

    public void LoadSceneDefault(string sceneName)
    {
        LoadScene(sceneName);
    }

    // Load scene additively 
    public void LoadScene(string sceneName, float time = 1.0f)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName, time));
    }

    IEnumerator LoadSceneCoroutine(string sceneName, float time)
    {
        fadeImage.gameObject.SetActive(true);
        yield return Fade(time, Color.clear, Color.black);
        UnloadAllScenes();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        yield return Fade(time, Color.black, Color.clear);
        fadeImage.gameObject.SetActive(false);
        OnSceneLoaded.Invoke();
    }


    IEnumerator Fade(float time, Color startColor, Color endColor)
    {
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            fadeImage.color = Color.Lerp(startColor, endColor, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        fadeImage.color = endColor;
    }
}
