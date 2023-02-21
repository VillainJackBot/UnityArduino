using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ArduinoManager : MonoBehaviour
{
    public bool AutoFindPort = true;
    public bool debug = false;
    public string portName = "/dev/ttyUSB0";

    [SerializeField]
    public SerialController.EditorEvents editorEvents;
    private SerialController serialController;
    private string defaultPortType = "/dev/ttyS5";

    private void OnEnable() {
        if(AutoFindPort) {
            StartCoroutine(ScanForNewControllers());
        } else {
            CreateSerialConnection(portName);
        }
    }

    private void Update()
    {
        serialController?.UpdateMessageQueue();
    }

    private void OnDisable()
    {
        serialController?.Dispose();
    }

    public IEnumerator ScanForNewControllers()
    {
        while((portName = SerialController.GetNewestPort()) == defaultPortType) {
            if(debug) Debug.Log("Scanning for new controllers...");
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("Found new controller on port: " + portName);
        CreateSerialConnection(portName);
    }

    private void CreateSerialConnection(string port) 
    {
        if(serialController != null) serialController.Dispose();
        serialController = new SerialController(port, editorEvents);

        serialController.onDisconnectDynamic.Add(() => {
            Debug.Log("Disconnected from Arduino");
            StartCoroutine(ScanForNewControllers());
        });
    }

    public void SendToArduino(string message)
    {
        serialController?.SendSerialMessage(message + "\r\n");
    }

    public void SubscribeToMessages(Action<string> callback)
    {
        serialController?.onMessageArriveDynamic.Add(callback);
    }

    public void UnsubscribeFromMessages(Action<string> callback)
    {
        serialController?.onMessageArriveDynamic.Remove(callback);
    }

    public void SubscribeToConnected(Action callback)
    {
        serialController?.onConnectDynamic.Add(callback);
    }

    public void UnsubscribeFromConnected(Action callback)
    {
        serialController?.onConnectDynamic.Remove(callback);
    }

    public void SubscribeToDisconnected(Action callback)
    {
        serialController?.onDisconnectDynamic.Add(callback);
    }

    public void UnsubscribeFromDisconnected(Action callback)
    {
        serialController?.onDisconnectDynamic.Remove(callback);
    }

}