using System;
using UnityEngine;
using UnityEngine.Events;

public class HaptXButtonLogic : MonoBehaviour
{
    [Serializable]
    public class BoolEvent : UnityEvent<Boolean> { }

    public BoolEvent ButtonPressed;

    void Start()
    {
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

    void OnHandleStateChange(int newState)
    {
        if (newState == buttonToggleState && buttonMesh != null && buttonMesh.material != null)
        {
            _btnOn = !_btnOn;
            ButtonPressed?.Invoke(_btnOn);
            buttonMesh.material.SetFloat(Shader.PropertyToID(_EmissivePropertyName),
                _btnOn ? btnOn : btnOff);
            if (haptXEffect != null)
            {
                if (_btnOn)
                {
                    haptXEffect.Play();
                }
                else
                {
                    haptXEffect.Stop();
                }
            }
        }
    }

    [Tooltip("The joint constraining the engine start/stop button.")]
    public Hx1DTranslator buttonJoint = null;

    [Tooltip("The name of the state function to listen to.")]
    public string buttonStateFunction = "Function0";

    [Tooltip("The button state that toggles the engine.")]
    public int buttonToggleState = 0;

    [Tooltip("The button mesh that toggles with start/stop.")]
    public MeshRenderer buttonMesh = null;

    [Tooltip("The emissive value to use on the button light when the engine is off.")]
    [Range(0.0f, 1.0f)]
    public float btnOff = 0.0f;

    [Tooltip("The emissive value to use on the button light when the engine is on.")]
    [Range(0.0f, 1.0f)]
    public float btnOn = 1.0f;

    [Tooltip("The Haptic Effect to play when the engine is on.")]
    public HxHapticEffect haptXEffect = null;

    // Whether the engine is on.
    private bool _btnOn = false;

    // The name of the property that manages emission intensity in the button material.
    private static readonly string _EmissivePropertyName = "_EmissionIntensity";
}
