using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenReader : MonoBehaviour
{
    SerialController serialController;

    private void OnEnable()
    {
        serialController = new SerialController();
        serialController.onConnectDynamic.Add(OnConnect);
    }

    private void OnDisable()
    {
        serialController.onConnectDynamic.Remove(OnConnect);
        serialController?.Dispose();
    }

    private void OnConnect()
    {
        TokenData tokenData = new TokenData();
        Debug.Log(tokenData.SerializeIntoJSON());
    }
}
