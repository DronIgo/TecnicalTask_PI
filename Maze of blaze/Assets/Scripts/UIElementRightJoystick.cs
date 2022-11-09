using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIElementRightJoystick : UIelement
{
    public override void UpdateUI()
    {
        if (GameManager.instance.controlType != GameManager.ControlType.STICKS)
        {
            this.gameObject.SetActive(false);
        } else
        {
            this.gameObject.SetActive(true);
        }
    }
}
