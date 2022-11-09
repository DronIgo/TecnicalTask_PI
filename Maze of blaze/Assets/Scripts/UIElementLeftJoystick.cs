using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIElementLeftJoystick : UIelement
{
    public override void UpdateUI()
    {
        if ((GameManager.instance.controlType != GameManager.ControlType.STICKS) || (GameManager.instance.cameraType != GameManager.CameraType.FIRST_PERSON))
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
        }
    }
}
