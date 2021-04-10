using System;
using System.Collections;
using System.Collections.Generic;
using SpacePhysic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SatelliteControllerUI : MonoBehaviour
{
    public Satellite.SatelliteController satelliteController;
    public Slider                        slider;
    public Text                          speedText;
    public GravityTracing                orbit;

    private bool isEditing = false;



    private void FixedUpdate()
    {
        
        if (!isEditing)
        {
            this.slider.value = satelliteController.satellite.GetVelocity().magnitude;
            
        }
        speedText.text = (.1f * satelliteController.satellite.GetVelocity().magnitude).ToString("f2") + " km/s";
    }

    public void ChangeSpeed()
    {
        if(isEditing)
        {
            Debug.Log("changing!");
            satelliteController.SetCurDirVelocity(this.slider.value );

        }
    }
    public void Pause()  
    {
        orbit.Freeze(true);
        isEditing = true;
    }

    public void Continue()
    {
        orbit.Freeze(false);
        isEditing = false;
    }
    


}
