using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(LoadSceneByName))]
public class LoadScenebasedOnArgs : MonoBehaviour
{
    [Serializable]
    public class SceneToLoadFromArgument
    {
        public int argumentIndex;
        public string sceneName;
    }

    [SerializeField]
    public SceneToLoadFromArgument[] scenesToLoad;

    private void Awake()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        if(args.Length < 1) { 
            Debug.Log("No arguments passed to application!");
            return; 
        }
        

        int index = BuildNumber(args);
        foreach(var sceneToLoad in scenesToLoad)
        {   
            if(index == sceneToLoad.argumentIndex)
            {
                GetComponent<LoadSceneByName>().LoadScene(sceneToLoad.sceneName);
            }
        }
    }

    // Find the string argument with the format "build:1"
    private int BuildNumber(string[] args) 
    {
        foreach(string arg in args)
        {
            if(arg.Contains("game:"))
            {
                string[] split = arg.Split(':');
                return Int32.Parse(split[1]);
            }
        }

        // Default to the first game
        return 0;
    }
}
