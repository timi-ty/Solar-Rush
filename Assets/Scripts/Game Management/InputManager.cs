using UnityEngine;
using System.Collections.Generic;

public enum ControlSide { Left, Right, All }

public class InputManager : MonoBehaviour
{
    #region Worker Parameters
    private Vector2[] worldInputOrigin = new Vector2[2];
    private List<IInputHandler> inputHandlers = new List<IInputHandler>();
    #endregion

    protected virtual void Update()
    {
        GetInput();
    }

    public void RegisterHandler(IInputHandler inputHandler)
    {
        inputHandlers.Add(inputHandler);
    }

    private void GetInput()
    {
#if UNITY_EDITOR
        for(int i = 0; i < 2; i++)
        {
            if (Input.GetMouseButtonDown(i))
            {
                worldInputOrigin[i] = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                foreach (IInputHandler inputHandler in inputHandlers)
                {
                    inputHandler?.OnInputStart((ControlSide)i);
                }
            }
            else if (Input.GetMouseButtonUp(i))
            {
                foreach (IInputHandler inputHandler in inputHandlers)
                {
                    inputHandler?.OnInputFinished((ControlSide)i);
                }
            }
            else if (Input.GetMouseButton(i))
            {
                float input = (Camera.main.ScreenToWorldPoint(Input.mousePosition).y - worldInputOrigin[i].y) / ScreenBounds.height;
                
                foreach (IInputHandler inputHandler in inputHandlers)
                {
                    inputHandler?.OnRecieveInput(input, (ControlSide)i);
                }
            }
        }
#else
        for(int i = 0; i < Mathf.Min(Input.touchCount, 2); i++)
        {
            Touch touch = Input.GetTouch(i);

            ControlSide controlSide = worldInputOrigin[i].x > ScreenBounds.centre.x
                    ? ControlSide.Right : ControlSide.Left;

            if (touch.phase == TouchPhase.Began)
            {
                worldInputOrigin[i] = Camera.main.ScreenToWorldPoint(touch.position);

                ControlSide newControlSide = worldInputOrigin[i].x > ScreenBounds.centre.x
                    ? ControlSide.Right : ControlSide.Left;

                foreach (IInputHandler inputHandler in inputHandlers)
                {
                    inputHandler?.OnInputStart(newControlSide);
                }
            }
            else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
            {
                foreach (IInputHandler inputHandler in inputHandlers)
                {
                    inputHandler?.OnInputFinished(controlSide);
                }
            }
            else
            {
                float input = (Camera.main.ScreenToWorldPoint(touch.position).y - worldInputOrigin[i].y) / ScreenBounds.height;
                foreach (IInputHandler inputHandler in inputHandlers)
                {
                    inputHandler?.OnRecieveInput(input, controlSide);
                }
            }
        }
#endif
    }
}
