namespace Dreamteck.Splines.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class BallCamera : MonoBehaviour
    {
        public Rigidbody rb;
        public SplineProjector projector;
        public float positionSpeed = 10f;
        public Vector3 offset = Vector3.zero;
        public float rotationSpeed = 0.5f;
        public Vector3 rotationOffset = Vector3.zero;

        Transform trs;

        private void Awake()
        {
            trs = transform;
            trs.position = rb.position + projector.result.rotation * offset;
            trs.rotation = projector.result.rotation * Quaternion.Euler(rotationOffset);
        }

        void FixedUpdate()
        {
            Vector3 idealPosition = rb.position + trs.rotation * offset;
            Quaternion idealRotation = projector.result.rotation * Quaternion.Euler(rotationOffset);
            trs.position = Vector3.Lerp(trs.position, idealPosition, Time.deltaTime * positionSpeed);
            trs.rotation = Quaternion.Slerp(trs.rotation, idealRotation, Time.deltaTime * rotationSpeed);
        }
    }
}