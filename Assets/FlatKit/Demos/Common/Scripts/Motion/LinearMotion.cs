using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FlatKit {
    public class LinearMotion : MonoBehaviour {
        public enum TranslationMode {
            Off,
            XAxis,
            YAxis,
            ZAxis,
            Vector
        }

        public enum RotationMode {
            Off,
            XAxis,
            YAxis,
            ZAxis,
            Vector
        }

        public TranslationMode translationMode = TranslationMode.Off;
        public Vector3 translationVector = Vector3.forward;
        public float translationSpeed = 1.0f;
        public RotationMode rotationMode = RotationMode.Off;
        public Vector3 rotationAxis = Vector3.up;
        public float rotationSpeed = 50.0f;
        public bool useLocalCoordinate = true;
        public float translationAcceleration = 0f;
        public float rotationAcceleration = 0f;

        private Vector3 TranslationVector {
            get {
                switch (translationMode) {
                    case TranslationMode.XAxis: return Vector3.right;
                    case TranslationMode.YAxis: return Vector3.up;
                    case TranslationMode.ZAxis: return Vector3.forward;
                    case TranslationMode.Vector: return translationVector;
                    case TranslationMode.Off:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return Vector3.zero;
            }
        }

        Vector3 RotationVector {
            get {
                switch (rotationMode) {
                    case RotationMode.XAxis: return Vector3.right;
                    case RotationMode.YAxis: return Vector3.up;
                    case RotationMode.ZAxis: return Vector3.forward;
                    case RotationMode.Vector: return rotationAxis;
                    case RotationMode.Off:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return Vector3.zero;
            }
        }

        void Update() {
            if (translationMode != TranslationMode.Off) {
                Vector3 positionDelta = TranslationVector * translationSpeed * Time.deltaTime;

                if (useLocalCoordinate) {
                    transform.localPosition += positionDelta;
                }
                else {
                    transform.position += positionDelta;
                }
            }

            if (rotationMode == RotationMode.Off) return;

            Quaternion rotationDelta = Quaternion.AngleAxis(
                rotationSpeed * Time.deltaTime, RotationVector);
            if (useLocalCoordinate) {
                transform.localRotation = rotationDelta * transform.localRotation;
            }
            else {
                transform.rotation = rotationDelta * transform.rotation;
            }
        }

        private void FixedUpdate() {
            translationSpeed += translationAcceleration;
            rotationSpeed += rotationAcceleration;
        }
    }
}