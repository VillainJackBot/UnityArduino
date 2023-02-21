using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TokenState
{
    disconnected,
    scanning,
    readingData,
    gameRunning,
    gameExit,
    writingData,
    ejecting,
}

[RequireComponent(typeof(ArduinoManager))]
public class TokenReader : MonoBehaviour
{
    private ArduinoManager arduinoManager;
    private TokenState tokenState = TokenState.disconnected;

    private void Start()
    {
        arduinoManager = GetComponent<ArduinoManager>();
        arduinoManager.SubscribeToMessages(OnMessageArrive);
        arduinoManager.SubscribeToConnected(OnConnected);
        arduinoManager.SubscribeToDisconnected(OnDisconnected);
    }

    private void OnConnected()
    {
        Debug.Log("Connected to Arduino");
        tokenState = TokenState.scanning;
        arduinoManager.SendToArduino("scan");
    }

    private void OnDisconnected() 
    {
        Debug.Log("Disconnected from Arduino");
    }


    private void OnDestroy()
    {
        arduinoManager?.UnsubscribeFromMessages(OnMessageArrive);
        arduinoManager?.UnsubscribeFromConnected(OnConnected);
        arduinoManager?.UnsubscribeFromDisconnected(OnDisconnected);
    }

    private void OnMessageArrive(string message)
    {
        // We want to start scanning if the arduino is available
        if(tokenState == TokenState.scanning && ScanSucceeded(message)) {
            Debug.Log("--- Scan succeeded ---");
            tokenState = TokenState.readingData;
            arduinoManager.SendToArduino("readMapJSON");
        }

        // We want to read data if the scan is a success
        if(tokenState == TokenState.readingData && IsMessageJSON(message)) {
            Debug.Log("--- Read data succeeded ---");
            tokenState = TokenState.gameRunning;
            Debug.Log(message);
        }

        if(tokenState == TokenState.gameExit) {
            Debug.Log("--- Game exit, start a data write ---");
            arduinoManager.SendToArduino("writeMaster");
            tokenState = TokenState.writingData;
        }

        // We want to write data if the player ends the game
        if(tokenState == TokenState.writingData && WriteSucceeded(message)) {
            Debug.Log("--- Write data succeeded ---");
            tokenState = TokenState.ejecting;
            arduinoManager.SendToArduino("eject");
        }

        // We want to start scanning again if the player ejects the token
        if(tokenState == TokenState.ejecting && EjectSucceeded(message)) {
            Debug.Log("--- Eject succeeded ---");
            tokenState = TokenState.scanning;
            arduinoManager.SendToArduino("scan");
        }
    }

    private bool IsMessageJSON(string message)
    {
        return message.StartsWith("{");
    }

    private bool ScanSucceeded(string message)
    {
        return message.Contains("UID");
    }

    private bool WriteSucceeded(string message)
    {
        return message.Contains("write success");
    }

    private bool EjectSucceeded(string message)
    {
        return true;
        // return message.Contains("eject success");
    }

    // Public interface -- -- -- 
    public void EjectCoin()
    {
        if(tokenState != TokenState.gameRunning) {
            Debug.Log("Tried to eject coin, but we never found a coin.");
            return;
        }

        tokenState = TokenState.gameExit;
        arduinoManager.SendToArduino("write: {\"name\": \"test\"}");
    }
}
