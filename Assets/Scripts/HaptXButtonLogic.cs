using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HaptXButtonLogic : MonoBehaviour
{
    private TextMeshPro _text;

    [Serializable]
    public class BoolEvent : UnityEvent<Boolean> { }

    public BoolEvent ButtonPressed;

    public UnityEvent ToggleEnabled;
    public UnityEvent ToggleDisabled;

    public Color TextColorOn = Color.white;
    public Color TextColorOff = Color.black;

    void Start()
    {
        _text = GetComponentInChildren<TextMeshPro>();
        if (buttonJoint != null)
        {
            HxDof dof = buttonJoint.GetOperatingDof();
            if (dof != null)
            {
                HxStateFunction stateFunction;
                if (dof.TryGetStateFunctionByName(buttonStateFunction, out stateFunction))
                {
                    stateFunction.OnStateChange += OnHandleStateChange;
                }
            }
        }
    }

    public void EnableToggle()
    {
        BtnOn = true;
        UpdateToggle();
        ToggleEnabled?.Invoke();
    }

    private void UpdateToggle()
    {
        ButtonPressed?.Invoke(BtnOn);
        _text.color = BtnOn ? TextColorOn : TextColorOff;
        if (haptXEffect != null)
        {
            if (BtnOn)
            {
                haptXEffect.Play();
            }
            else
            {
                haptXEffect.Stop();
            }
        }
    }

    public void DisableToggle()
    {
        BtnOn = false;
        UpdateToggle();
        ToggleDisabled?.Invoke();
    }

    void OnHandleStateChange(int newState)
    {
        if (newState == buttonToggleState)
        {
            if (BtnOn)
            {
                DisableToggle();
            }
            else
            {
                EnableToggle();
            }
        }
    }

    [Tooltip("The joint constraining the engine start/stop button.")]
    public Hx1DTranslator buttonJoint = null;

    [Tooltip("The name of the state function to listen to.")]
    public string buttonStateFunction = "Function0";

    [Tooltip("The button state that toggles the engine.")]
    public int buttonToggleState = 0;

    [Tooltip("The Haptic Effect to play when the engine is on.")]
    public HxHapticEffect haptXEffect = null;

    // Whether the engine is on.
    public bool BtnOn = false;
}
