using UnityEngine;

namespace Dreamteck.Splines.Examples
{
    public class CameraSmoothRotation : MonoBehaviour
    {
        //Simple script to smooth out the rotation of the camera
        //Since getting the rotation directly from the spline might not look good 
        //When looking from first person perspective
        public  float          damp;
        private SplineFollower follower;
        private Transform      trs;

        private void Start()
        {
            trs      = transform;
            follower = GetComponent<SplineFollower>();
        }

        private void Update()
        {
            if (damp <= 0f)
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