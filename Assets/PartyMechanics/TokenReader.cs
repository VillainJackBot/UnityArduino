using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TokenState
{
    disconnected,
    scanning,
    readingData,
    gameRunning,
    writingData,
    ejecting,
}

[RequireComponent(typeof(ArduinoManager))]
public class TokenReader : MonoBehaviour
{
    private ArduinoManager arduinoManager;
    private TokenState tokenState = TokenState.disconnected;
    private float timeSinceScanPole = 0.0f;

    private void Start()
    {
        arduinoManager = GetComponent<ArduinoManager>();
        arduinoManager.SubscribeToMessages(OnMessageArrive);
        arduinoManager.SubscribeToConnected(OnConnected);
    }

    private void OnConnected()
    {
        Debug.Log("Connected to Arduino");
        tokenState = TokenState.scanning;

        // Wait 1 second
        StartCoroutine(WaitAndScan());
    }

    private IEnumerator WaitAndScan()
    {
        yield return new WaitForSeconds(1.0f);
        arduinoManager.SendToArduino("scan");
    }

    private void OnDestroy()
    {
        arduinoManager?.UnsubscribeFromMessages(OnMessageArrive);
        arduinoManager?.UnsubscribeFromConnected(OnConnected);
    }

    private void OnMessageArrive(string message)
    {
        // If the arduino dies while scanning, we have to keep track
        if(tokenState == TokenState.scanning || tokenState == TokenState.readingData || tokenState == TokenState.writingData) {
            timeSinceScanPole = 0.0f;
        }

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
        }

        // We want to write data if the player ends the game
        if(tokenState == TokenState.writingData && WriteSucceeded(message)) {
            Debug.Log("--- Write data succeeded ---");
            tokenState = TokenState.ejecting;
            arduinoManager.SendToArduino("eject");

            // Wait two seconds and then send "uneject"
            StartCoroutine(WaitAndUneject());
        }
    }

    private void Update() 
    {
        if(tokenState == TokenState.scanning || tokenState == TokenState.readingData || tokenState == TokenState.writingData) {
            timeSinceScanPole += Time.deltaTime;
            if(timeSinceScanPole > 5.0f) {
                Debug.LogWarning("The arduino is not responding!");
                tokenState = TokenState.disconnected;
                arduinoManager.DisconnectManually();
                timeSinceScanPole = 0.0f;
            }
        }

        if(Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("Sending (0x04)");
            arduinoManager.SendToArduinoRaw(char.ConvertFromUtf32(0x00000004));
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

    // Public interface -- -- -- 
    public void EjectCoin()
    {
        if(tokenState != TokenState.gameRunning) {
            Debug.Log("Tried to eject coin, but we never found a coin.");
            return;
        }

        tokenState = TokenState.writingData;
        arduinoManager.SendToArduino("writeMaster");
        Debug.Log("--- Game exit, start a data write ---");
    }

    private IEnumerator WaitAndUneject()
    {
        yield return new WaitForSeconds(2f);
        arduinoManager.SendToArduino("uneject");
        Debug.Log("--- Eject succeeded ---");
        tokenState = TokenState.scanning;
        arduinoManager.SendToArduino("scan");
    }
}
