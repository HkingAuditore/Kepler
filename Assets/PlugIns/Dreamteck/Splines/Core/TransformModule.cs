using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dreamteck.Splines
{
    [Serializable]
    public class TransformModule : ISerializationCallbackReceiver
    {
        public enum VelocityHandleMode
        {
            Zero,
            Preserve,
            Align,
            AlignRealistic
        }

        //These are used to save allocations
        private static Vector3    position = Vector3.zero;
        private static Quaternion rotation = Quaternion.identity;

        [SerializeField] [HideInInspector] private bool _hasOffset;

        [SerializeField] [HideInInspector] private bool _hasRotationOffset;

        [SerializeField] [HideInInspector] [FormerlySerializedAs("offset")]
        private Vector2 _offset;

        [SerializeField] [HideInInspector] [FormerlySerializedAs("rotationOffset")]
        private Vector3 _rotationOffset = Vector3.zero;

        [SerializeField] [HideInInspector] [FormerlySerializedAs("baseScale")]
        private Vector3 _baseScale = Vector3.one;

        public VelocityHandleMode velocityHandleMode = VelocityHandleMode.Zero;

        public bool             applyPositionX = true;
        public bool             applyPositionY = true;
        public bool             applyPositionZ = true;
        public Spline.Direction direction      = Spline.Direction.Forward;

        public bool applyRotationX = true;
        public bool applyRotationY = true;
        public bool applyRotationZ = true;

        public bool applyScaleX;
        public bool applyScaleY;
        public bool applyScaleZ;

        [HideInInspector] public SplineUser targetUser;

        private SplineSample _splineResult;

        public Vector2 offset
        {
            get => _offset;
            set
            {
                if (value != _offset)
                {
                    _offset    = value;
                    _hasOffset = _offset != Vector2.zero;
                    if (targetUser != null) targetUser.Rebuild();
                }
            }
        }

        public Vector3 rotationOffset
        {
            get => _rotationOffset;
            set
            {
                if (value != _rotationOffset)
                {
                    _rotationOffset    = value;
                    _hasRotationOffset = _rotationOffset != Vector3.zero;
                    if (targetUser != null) targetUser.Rebuild();
                }
            }
        }

        public bool hasOffset => _hasOffset;

        public bool hasRotationOffset => _hasRotationOffset;

        public Vector3 baseScale
        {
            get => _baseScale;
            set
            {
                if (value != _baseScale)
                {
                    _baseScale = value;
                    if (targetUser != null) targetUser.Rebuild();
                }
            }
        }

        public SplineSample splineResult
        {
            get
            {
                if (_splineResult == null) _splineResult = new SplineSample();
                return _splineResult;
            }
            set
            {
                if (_splineResult == null) _splineResult = new SplineSample(value);
                else _splineResult.CopyFrom(value);
            }
        }

        public bool applyPosition
        {
            get => applyPositionX || applyPositionY || applyPositionZ;
            set => applyPositionX = applyPositionY = applyPositionZ = value;
        }

        public bool applyRotation
        {
            get => applyRotationX || applyRotationY || applyRotationZ;
            set => applyRotationX = applyRotationY = applyRotationZ = value;
        }

        public bool applyScale
        {
            get => applyScaleX || applyScaleY || applyScaleZ;
            set => applyScaleX = applyScaleY = applyScaleZ = value;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            _hasRotationOffset = _rotationOffset != Vector3.zero;
            _hasOffset         = _offset         != Vector2.zero;
        }

        public void ApplyTransform(Transform input)
        {
            input.position   = GetPosition(input.position);
            input.rotation   = GetRotation(input.rotation);
            input.localScale = GetScale(input.localScale);
        }

        public void ApplyRigidbody(Rigidbody input)
        {
            input.transform.localScale = GetScale(input.transform.localScale);
            input.MovePosition(GetPosition(input.position));
            input.velocity = HandleVelocity(input.velocity);
            input.MoveRotation(GetRotation(input.rotation));
            var angularVelocity                   = input.angularVelocity;
            if (applyRotationX) angularVelocity.x = 0f;
            if (applyRotationY) angularVelocity.y = 0f;
            if (applyRotationZ) angularVelocity.z = 0f;
            input.angularVelocity = angularVelocity;
        }

        public void ApplyRigidbody2D(Rigidbody2D input)
        {
            input.transform.localScale = GetScale(input.transform.localScale);
            input.position             = GetPosition(input.position);
            input.velocity             = HandleVelocity(input.velocity);
            input.rotation             = -GetRotation(Quaternion.Euler(0f, 0f, input.rotation)).eulerAngles.z;
            if (applyRotationX) input.angularVelocity = 0f;
        }

        private Vector3 HandleVelocity(Vector3 velocity)
        {
            var idealVelocity = Vector3.zero;
            var direction     = Vector3.right;
            switch (velocityHandleMode)
            {
                case VelocityHandleMode.Preserve:
                    idealVelocity = velocity;
                    break;
                case VelocityHandleMode.Align:
                    direction = _splineResult.forward;
                    if (Vector3.Dot(velocity, direction) < 0f) direction *= -1f;
                    idealVelocity = direction * velocity.magnitude;
                    break;
                case VelocityHandleMode.AlignRealistic:
                    direction = _splineResult.forward;
                    if (Vector3.Dot(velocity, direction) < 0f) direction *= -1f;
                    idealVelocity = direction * velocity.magnitude * Vector3.Dot(velocity.normalized, direction);
                    break;
            }

            if (applyPositionX) velocity.x = idealVelocity.x;
            if (applyPositionY) velocity.y = idealVelocity.y;
            if (applyPositionZ) velocity.z = idealVelocity.z;
            return velocity;
        }

        private Vector3 GetPosition(Vector3 inputPosition)
        {
            position = _splineResult.position;
            var finalOffset = _offset;
            //if (customOffset != null) finalOffset += customOffset.Evaluate(_splineResult.percent);
            if (finalOffset != Vector2.zero)
                position += _splineResult.right * finalOffset.x * _splineResult.size +
                            _splineResult.up    * finalOffset.y * _splineResult.size;
            if (applyPositionX) inputPosition.x = position.x;
            if (applyPositionY) inputPosition.y = position.y;
            if (applyPositionZ) inputPosition.z = position.z;
            return inputPosition;
        }

        private Quaternion GetRotation(Quaternion inputRotation)
        {
            rotation =
                Quaternion.LookRotation(_splineResult.forward * (direction == Spline.Direction.Forward ? 1f : -1f),
                                        _splineResult.up);
            if (_rotationOffset != Vector3.zero) rotation = rotation * Quaternion.Euler(_rotationOffset);

            if (!applyRotationX || !applyRotationY || !applyRotationZ)
            {
                var targetEuler                    = rotation.eulerAngles;
                var sourceEuler                    = inputRotation.eulerAngles;
                if (!applyRotationX) targetEuler.x = sourceEuler.x;
                if (!applyRotationY) targetEuler.y = sourceEuler.y;
                if (!applyRotationZ) targetEuler.z = sourceEuler.z;
                inputRotation.eulerAngles = targetEuler;
            }
            else
            {
                inputRotation = rotation;
            }

            return inputRotation;
        }

        private Vector3 GetScale(Vector3 inputScale)
        {
            if (applyScaleX) inputScale.x = _baseScale.x * _splineResult.size;
            if (applyScaleY) inputScale.y = _baseScale.y * _splineResult.size;
            if (applyScaleZ) inputScale.z = _baseScale.z * _splineResult.size;
            return inputScale;
        }
    }
}