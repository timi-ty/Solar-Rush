using UnityEngine;
using System.Collections;

public interface IInputHandler
{
    ControlSide controlSide { get; }

    void OnInputStart(ControlSide controlSide);

    void OnRecieveInput(float input, ControlSide controlSide);

    void OnInputFinished(ControlSide controlSide);
}
