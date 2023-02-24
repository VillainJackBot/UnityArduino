using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class PulseText : MonoBehaviour
{

    public float flashInterval = 0.5f;
    public float brightnessTarget = 0.5f; // Amount to change the brightness by each flash

    private TextMeshProUGUI _textMesh;
    private float initialBrightness;

    void OnEnable()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
        initialBrightness = _textMesh.color.a;
    }

    void Update() 
    {
        float osc = Mathf.Sin(Time.time * Mathf.PI * flashInterval) * 0.5f + 0.5f; // Calculate the new brightness change amount (sin wave
        float brightness = Mathf.Lerp(initialBrightness, brightnessTarget, osc); // Calculate the new brightness value
        _textMesh.color = new Color(_textMesh.color.r, _textMesh.color.g, _textMesh.color.b, brightness); // Update the text's color
    }

    void OnDisable()
    {
        _textMesh.color = new Color(_textMesh.color.r, _textMesh.color.g, _textMesh.color.b, initialBrightness); // Reset the color to its initial brightness
    }
}