using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneByName : MonoBehaviour
{
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
    // Load scene additively 
    public void LoadScene(string sceneName)
    {
        UnloadAllScenes();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }
}
