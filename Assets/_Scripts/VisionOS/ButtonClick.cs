using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public class ButtonClick : MonoBehaviour
{

    // add action to process press event
    public void Press()
    {
        var button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.Invoke();
        }
    }
}
