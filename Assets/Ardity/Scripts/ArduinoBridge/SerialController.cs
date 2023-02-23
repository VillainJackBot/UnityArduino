/**
 * Ardity (Serial Communication for Arduino + Unity)
 * Author: Daniel Wilches <dwilches@gmail.com>
 *
 * This work is released under the Creative Commons Attributions license.
 * https://creativecommons.org/licenses/by/2.0/
 */

using UnityEngine;
using System.Threading;
using System;
using System.IO;
using System.IO.Ports;
using UnityEngine.Events;
using System.Collections.Generic;

/**
 * This class allows a Unity program to continually check for messages from a
 * serial device.
 *
 * It creates a Thread that communicates with the serial port and continually
 * polls the messages on the wire.
 * That Thread puts all the messages inside a Queue, and this SerialController
 * class polls that queue by means of invoking SerialThread.GetSerialMessage().
 *
 * The serial device must send its messages separated by a newline character.
 * Neither the SerialController nor the SerialThread perform any validation
 * on the integrity of the message. It's up to the one that makes sense of the
 * data.
 */
public class SerialController
{
    [Serializable]
    public class EditorEvents
    {
        // When the ardunio connects, provide a onConnected callback.
        [SerializeField]
        public UnityEvent onConnected;

        // When the arduino disconnects, provide a onDisconnected callback.
        [SerializeField]
        public UnityEvent onDisconnected;

        // When a message arrives, provide a onMessageArrived callback.
        [SerializeField]
        public UnityEvent<string> onMessageArrived;
    }

    // Port name with which the SerialPort object will be created.
    private string portName = "/dev/ttyUSB0";

    // Baud rate that the serial device is using to transmit data.
    private int baudRate = 9600;

    // After an error in the serial communication, or an unsuccessful connect, how many milliseconds we should wait.
    private int reconnectionDelay = 1000;

    // Maximum number of unread messages in the queue. New or old (depending on
    // "Drop Old Message" configuration) messages will be discarded.
    private int maxUnreadMessages = 1;

    // When the queue is full, prefer dropping the oldest message in the queue 
    // instead of the new incoming message. Use this if you prefer to keep the
    // newest messages from the port.
    private bool dropOldMessage;

    // Callbacks to be invoked when the serial device connects or disconnects.
    EditorEvents editorEvents;

    // Is connected?
    private bool isConnected = false;

    // Message debug enabled
    private bool messageDebug = false;

    // Constants used to mark the start and end of a connection. There is no
    // way you can generate clashing messages from your serial device, as I
    // compare the references of these strings, no their contents. So if you
    // send these same strings from the serial device, upon reconstruction they
    // will have different reference ids.
    public const string SERIAL_DEVICE_CONNECTED = "__Connected__";
    public const string SERIAL_DEVICE_DISCONNECTED = "__Disconnected__";

    // Internal reference to the Thread and the object that runs in it.
    protected Thread thread;
    protected SerialThreadLines serialThread;

    public List<Action> onConnectDynamic = new List<Action>();
    public List<Action> onDisconnectDynamic = new List<Action>();
    public List<Action<string>> onMessageArriveDynamic = new List<Action<string>>();

    public SerialController(EditorEvents editorEvents = null, int baudRate = 9600, int reconnectionDelay = 100, int maxUnreadMessages = 10, bool dropOldMessage = true)
    {
        this.baudRate = baudRate;
        this.reconnectionDelay = reconnectionDelay;
        this.maxUnreadMessages = maxUnreadMessages;
        this.dropOldMessage = dropOldMessage;
        this.editorEvents = editorEvents;
    }

    public void EnableDebug(bool state)
    {
        messageDebug = state;
    }

    // ------------------------------------------------------------------------
    // Invoked whenever the SerialController gameobject is activated.
    // It creates a new thread that tries to connect to the serial device
    // and start reading from it.
    // ------------------------------------------------------------------------
    private void Initialize()
    {
        Debug.Log("Initializing SerialController with port: " + portName);
        serialThread = new SerialThreadLines(portName,
                                             baudRate,
                                             reconnectionDelay,
                                             maxUnreadMessages,
                                             dropOldMessage);
        thread = new Thread(new ThreadStart(serialThread.RunForever));
        thread.Start();
    }

    public void Connect(string portName) {
        this.portName = portName;
        Dispose();
        Initialize();
    }
    
    // ------------------------------------------------------------------------
    // Invoked whenever the SerialController gameobject is deactivated.
    // It stops and destroys the thread that was reading from the serial device.
    // ------------------------------------------------------------------------
    public void Dispose()
    {
        // If there is a user-defined tear-down function, execute it before
        // closing the underlying COM port.
        if(isConnected) Disconnected();

        // The serialThread reference should never be null at this point,
        // unless an Exception happened in the OnEnable(), in which case I've
        // no idea what face Unity will make.
        if (serialThread != null)
        {
            serialThread.RequestStop();
            serialThread = null;
        }

        // This reference shouldn't be null at this point anyway.
        if (thread != null)
        {
            Debug.Log("Closing thread: " + thread.ManagedThreadId);
            thread.Join();
            thread = null;
        }
    }

    // ------------------------------------------------------------------------
    // Polls messages from the queue that the SerialThread object keeps. Once a
    // message has been polled it is removed from the queue. There are some
    // special messages that mark the start/end of the communication with the
    // device.
    // ------------------------------------------------------------------------
    public void UpdateMessageQueue()
    {
        if(serialThread == null) return;

        // Read the next message from the queue
        string message = (string)serialThread.ReadMessage();

        // TODO investigate empty message spam after disconnect
        if (String.IsNullOrEmpty(message) || message.Length < 1)
            return;

        if(messageDebug) {
            Debug.Log(message);
        }

        // Check if the message is plain data or a connect/disconnect event.
        if (ReferenceEquals(message, SERIAL_DEVICE_CONNECTED) && !isConnected) Connected();
        else if (ReferenceEquals(message, SERIAL_DEVICE_DISCONNECTED) && isConnected) Disconnected();
        else if(isConnected) MessageArrived(message);
    }

    // ------------------------------------------------------------------------
    // Returns a new unread message from the serial device. You only need to
    // call this if you don't provide a message listener.
    // ------------------------------------------------------------------------
    public string ReadSerialMessage()
    {
        // Read the next message from the queue
        return (string)serialThread.ReadMessage();
    }

    // ------------------------------------------------------------------------
    // Puts a message in the outgoing queue. The thread object will send the
    // message to the serial device when it considers it's appropriate.
    // ------------------------------------------------------------------------
    public void SendSerialMessage(string message)
    {
        serialThread.SendMessage(message);
    }

    // ------------------------------------------------------------------------
    // Executes a user-defined function after Unity opens the COM port, so the
    // user can send some initialization message to the hardware reliably.
    // ------------------------------------------------------------------------
    public void AddConnectedListener(Action action)
    {
        this.onConnectDynamic.Add(action);
    }

    // ------------------------------------------------------------------------
    // Executes a user-defined function before Unity closes the COM port, so
    // the user can send some tear-down message to the hardware reliably.
    // ------------------------------------------------------------------------
    public void AddDisconnectedListener(Action action)
    {
        this.onDisconnectDynamic.Add(action);
    }

    // ------------------------------------------------------------------------
    // Executes a user-defined function when a new message arrives from the
    // serial device.
    // ------------------------------------------------------------------------
    public void AddMessageArrivedListener(Action<string> action)
    {
        this.onMessageArriveDynamic.Add(action);
    }

    // The connected event is triggered when the serial device is connected.
    public void Connected()
    {
        isConnected = true;
        editorEvents?.onConnected?.Invoke();

        foreach (Action action in this.onConnectDynamic)
        {
            action.Invoke();
        }
    }

    // The disconnected event is triggered when the serial device is disconnected.
    public void Disconnected()
    {
        isConnected = false;
        editorEvents?.onDisconnected?.Invoke();

        foreach (Action action in this.onDisconnectDynamic)
        {
            action.Invoke();
        }
    }

    // The message arrived event is triggered when a new message arrives from the serial device.
    public void MessageArrived(string message)
    {
        editorEvents?.onMessageArrived?.Invoke(message);

        foreach (Action<string> action in this.onMessageArriveDynamic)
        {
            action.Invoke(message);
        }
    }

    public static string[] GetPortNames()
    {
        return SerialPort.GetPortNames();
    }

    public static string GetNewestPort()
    {
        string[] ports = GetPortNames();
        if (ports.Length == 0)
        {
            return null;
        }

        string name = ports[0];
        DateTime mostRecent = File.GetLastWriteTime(name);
        foreach (string port in ports)
        {
            DateTime lastWrite = File.GetLastWriteTime(port);
            if (lastWrite > mostRecent)
            {
                mostRecent = lastWrite;
                name = port;
            }
        }
        return name;
    }

    public bool IsConnected()
    {
        return isConnected;
    }
}

