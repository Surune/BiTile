using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager
{
    public Action<Define.MouseEvent> MouseAction =null;

    static public bool _pressed = false;
    public void OnUpdate()
    {
        if (EventSystem.current.IsPointerOverGameObject() == false)
            return;
        
        if(MouseAction != null)
        {
            if(Input.GetMouseButton(0))
            {
                MouseAction.Invoke(Define.MouseEvent.Press);
                _pressed = true;
            }

            else
            {
                if(_pressed)
                    MouseAction.Invoke(Define.MouseEvent.Click);
                _pressed = false;
            }
        }


    }

    public void Clear()
    {
        MouseAction = null;
    }
}
