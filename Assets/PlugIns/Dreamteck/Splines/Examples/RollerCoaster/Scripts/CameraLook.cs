using UnityEngine;

namespace Dreamteck.Splines.Examples
{
    public class CameraLook : MonoBehaviour
    {
        public float sensitivity = 3f;
        public float dampSpeed;
        public float lookRange = 45f;

        public Transform crosshairSphere;

        private float crosshairZ      = 5f;
        private float idealCrosshairZ = 3f;
        private float x;

        private float xMove;
        private float y;
        private float yMove;

        // Update is called once per frame
        private void Update()
        {
            xMove =  Mathf.MoveTowards(xMove, 0f, Time.deltaTime * dampSpeed);
            yMove =  Mathf.MoveTowards(yMove, 0f, Time.deltaTime * dampSpeed);
            xMove += Input.GetAxis("Mouse X") / 10f;
            yMove -= Input.GetAxis("Mouse Y") / 10f;
            xMove =  Mathf.Clamp(xMove, -1f, 1f);
            yMove =  Mathf.Clamp(yMove, -1f, 1f);
            var halfLookRange = lookRange / 2f;
            x += xMove * Time.deltaTime * sensitivity;
            y += yMove * Time.deltaTime * sensitivity;

            if (x > halfLookRange)
            {
                x = halfLookRange;
                if (xMove > 0f) xMove = 0f;
            }
            else if (x < -halfLookRange)
            {
                x = -halfLookRange;
                if (xMove < 0f) xMove = 0f;
            }

            if (y > halfLookRange)
            {
                y = halfLookRange;
                if (yMove > 0f) yMove = 0f;
            }
            else if (y < -halfLookRange)
            {
                y = -halfLookRange;
                if (yMove < 0f) yMove = 0f;
            }

            if (crosshairSphere != null && crosshairSphere.gameObject.activeSelf)
            {
                idealCrosshairZ += Input.GetAxis("Mouse ScrollWheel") * 4f;
                idealCrosshairZ =  Mathf.Clamp(idealCrosshairZ, 2f, 6f);
                crosshairZ      =  Mathf.MoveTowards(crosshairZ, idealCrosshairZ, Time.deltaTime * 8f);
                var localPos = crosshairSphere.localPosition;
                localPos.z                    = crosshairZ;
                crosshairSphere.localPosition = localPos;
            }

            transform.localRotation = Quaternion.AngleAxis(x, Vector3.up) * Quaternion.AngleAxis(y, Vector3.right);
        }
    }
}