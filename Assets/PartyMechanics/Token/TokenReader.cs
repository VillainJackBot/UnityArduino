using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    public UnityEvent OnGameStart;

    private ArduinoManager arduinoManager;
    private TokenState tokenState = TokenState.disconnected;
    private float timeSinceScanPole = 0.0f;

    private void Start()
    {
        Debug.Log("Starting token reader");
        arduinoManager = GetComponent<ArduinoManager>();
        arduinoManager.SubscribeToMessages(OnMessageArrive);
        arduinoManager.SubscribeToConnected(OnConnected);
    }

    private void OnConnected()
    {
        Debug.Log("Connected to Arduino");
        tokenState = TokenState.scanning;

        // Wait 1 second
        StartCoroutine(WaitAndScanOnce());
    }

    private IEnumerator WaitAndScanOnce()
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

        if(tokenState == TokenState.scanning && ScanFailed(message)) {
            Debug.Log("--- Scan failed ---");
            arduinoManager.SendToArduino("scan");
            return;
        }

        // We want to start scanning if the arduino is available
        if(tokenState == TokenState.scanning && ScanSucceeded(message)) {
            Debug.Log("--- Scan succeeded ---");
            tokenState = TokenState.readingData;
            arduinoManager.SendToArduino("readMapJSON");
            return;
        }

        // We want to read data if the scan is a success
        if(tokenState == TokenState.readingData && IsMessageJSON(message)) {
            Token.Publish(message);
            Debug.Log("--- Read data succeeded ---");
            tokenState = TokenState.gameRunning;
            OnGameStart.Invoke();
            return;
        }

        // We want to write data if the player ends the game
        if(tokenState == TokenState.writingData && WriteSucceeded(message)) {
            Debug.Log("--- Write data succeeded ---");
            tokenState = TokenState.ejecting;
            arduinoManager.SendToArduino("eject");

            Token.Invalidate();
            // Wait two seconds and then send "uneject"
            StartCoroutine(WaitAndUneject());
            return;
        }

        if(Crashed(message)) {
            Debug.Log("--- Arduino Crashed ---");
            Debug.Log(message);
            Token.Invalidate();
            arduinoManager.SendToArduinoRaw(char.ConvertFromUtf32(0x00000004));
            tokenState = TokenState.disconnected;
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
    }

    private bool IsMessageJSON(string message)
    {
        return message.StartsWith("{");
    }

    private bool ScanSucceeded(string message)
    {
        return message.Contains("scan success");
    }

    private bool ScanFailed(string message)
    {
        return message.Contains("scan failed");
    }

    private bool WriteSucceeded(string message)
    {
        return message.Contains("write success");
    }

    private bool Crashed(string message)
    {
        return message.Contains("Traceback") || message.Contains("REPL");
    }

    // Public interface -- -- -- 
    public void EjectCoin()
    {
        if(tokenState != TokenState.gameRunning) {
            Debug.Log("Tried to eject coin, but we never found a coin.");
            return;
        }

        tokenState = TokenState.writingData;
        Token.Data.pandoras_box = 3;
        arduinoManager.SendToArduino("updateJSON:" + Token.Data.ToJSON());
        Debug.Log("--- Game exit, start a data write ---");
    }

    private IEnumerator WaitAndUneject()
    {
        yield return new WaitForSeconds(2f);
        arduinoManager.SendToArduino("uneject");
        Debug.Log("--- Eject succeeded ---");
        tokenState = TokenState.scanning;
        yield return new WaitForSeconds(1f);
        arduinoManager.SendToArduino("scan");
    }
}
