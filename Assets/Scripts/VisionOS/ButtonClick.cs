using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public class ButtonClick : MonoBehaviour
{
    public UnityEvent click_callback;
    // add action to process press event
    public void Press()
    {
        //Debug.Log("Press Call");
        var button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.Invoke();
        }

        if (click_callback != null)
        {
            click_callback.Invoke();
        }
    }
}
