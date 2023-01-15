using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsHelper : MonoBehaviour
{
    public static float AmplitudeModifier = 20;
    public static float FrequencyModifier = 40;

    public static Action OnChangeCallback;

    public void UpdateAmplitudeModifier(float val)
    {
        AmplitudeModifier = val;
        OnChangeCallback?.Invoke();
    }

    public void UpdateFrequencyModifier(float val)
    {
        FrequencyModifier = val;
        OnChangeCallback?.Invoke();
    }

}
