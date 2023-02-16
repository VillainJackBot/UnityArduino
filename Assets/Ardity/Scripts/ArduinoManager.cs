using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoManager : MonoBehaviour
{
    public string portName = "/dev/ttyUSB0";
    [SerializeField]
    public SerialController.EditorEvents editorEvents;
    private SerialController serialController;

    private void OnEnable()
    {
        serialController = new SerialController(portName, editorEvents);
    }

    private void OnDisable()
    {
        serialController.Dispose();
    }

    private void Update()
    {
        serialController.UpdateMessageQueue();
    }

    public void SendToArduino(string message)
    {
        serialController.SendSerialMessage(message);
    }
}