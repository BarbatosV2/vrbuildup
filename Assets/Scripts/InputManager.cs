using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class InputManager : MonoBehaviour
{
    [Header("Actions")]
    public SteamVR_Action_Boolean touch = null;
    public SteamVR_Action_Boolean press = null;
    public SteamVR_Action_Vector2 touchPosition = null;

    [Header("Scene Objects")]
    public RadialMenu radialMenu = null;

    private void Awake()
    {
        touch.onChange += Touch;
        press.onStateDown += PressRelease;
        touchPosition.onAxis += Position;
    }

    private void OnDestroy()
    {
        touch.onChange -= Touch;
        press.onStateDown -= PressRelease;
        touchPosition.onAxis -= Position;
    }

    private void Position(SteamVR_Action_Vector2 formAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        radialMenu.SetTouchPosition(axis);
    }

    private void Touch(SteamVR_Action_Boolean formAction, SteamVR_Input_Sources fromSource, bool newState)
    {
        if (radialMenu.interactingHand.currentAttachedObject == null)
        {
            radialMenu.Show(newState);
            
        }
        else
        {
            radialMenu.Show(false);
        }
    }

    private void PressRelease(SteamVR_Action_Boolean formAction, SteamVR_Input_Sources fromSource)
    {
        
        if (radialMenu.gameObject.activeSelf)
        {
            radialMenu.ActivateHighlightedSection();
        }
        
    }



}