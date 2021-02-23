namespace Dreamteck.Splines.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CameraSmoothRotation : MonoBehaviour
    {
        //Simple script to smooth out the rotation of the camera
        //Since getting the rotation directly from the spline might not look good 
        //When looking from first person perspective
        public float damp = 0f;
        SplineFollower follower;
        Transform trs;

        void Start()
        {
            trs = transform;
            follower = GetComponent<SplineFollower>();
        }

        void Update()
        {
            if(damp <= 0f)
            {
                //if no damp is used, then make the follower apply the rotation automatically
                follower.motion.applyRotation = true;
                return;
            }
            //if damp > 0 then handle rotation manually here
            follower.motion.applyRotation = false;
            trs.rotation = Quaternion.Slerp(trs.rotation, follower.modifiedResult.rotation, Time.deltaTime / damp);
        }
    }
}
