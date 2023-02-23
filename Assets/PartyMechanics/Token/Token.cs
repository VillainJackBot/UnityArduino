using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Token
{
    public static bool tokenLoaded = false;
    private static GameData _data = null;

    public static GameData Data {
        get {
            if (_data == null) {
                Debug.LogError("Token not loaded!");
                return null;
            }
            return _data;
        }
    }

    public static bool ValidToken()
    {
        return tokenLoaded;
    }

    public static void Publish(string JSON)
    {
        _data = JsonUtility.FromJson<GameData>(JSON);
        tokenLoaded = true;

        Debug.Log("--- Token Loaded ---\n" + _data.ToString());
    }

    public static void Invalidate()
    {
        tokenLoaded = false;
    }
}
