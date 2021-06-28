using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
public enum RumblePattern
{
    Constant,
    Pulse,
    Linear
}

public class Rumbler : MonoBehaviour
{
    private PlayerInput _playerInput;
    public RumblePattern activeRumbePattern;
    private float rumbleDurration;
    private float pulseDurration;
    private float lowA;
    private float lowStep;
    private float highA;
    private float highStep;
    private float rumbleStep;
    private bool isMotorActive = false;
    float left = 0, right = 0;

    public void RumbleLinear(float lowStart, float lowEnd, float highStart, float highEnd, float durration)
    {
        activeRumbePattern = RumblePattern.Linear;
        lowA = lowStart;
        highA = highStart;
        lowStep = (lowEnd - lowStart) / durration;
        highStep = (highEnd - highStart) / durration;
        rumbleDurration = Time.time + durration;
    }


    // Unity MonoBehaviors
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }


    private void OnDestroy()
    {
        StopAllCoroutines();
        StopRumble();
    }

    // Private helpers

    public void LinearRumbleIncrease()
    {
        left += +0.1f * Time.deltaTime;
        right += +0.1f * Time.deltaTime;
        Gamepad.current.SetMotorSpeeds(left, right);
    }

    public void StopRumble()
    {
        Gamepad.current.SetMotorSpeeds(0, 0);

    }
}