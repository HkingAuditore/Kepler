﻿using Satellite;
using SpacePhysic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI.Satellite
{
    public class SatelliteControllerUI : MonoBehaviour
    {
        public  GravityTracing      orbit;
        public  SatelliteController satelliteController;
        public  Slider              slider;
        public  Text                speedText;
        private bool                isEditing;


        private void FixedUpdate()
        {
            if (!isEditing) slider.value = satelliteController.satellite.GetVelocity().magnitude;
            speedText.text = (.1f * satelliteController.satellite.GetVelocity().magnitude).ToString("f2") + " km/s";
        }

        public void ChangeSpeed()
        {
            if (isEditing)
            {
                Debug.Log("changing!");
                satelliteController.SetCurDirVelocity(slider.value);
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
}