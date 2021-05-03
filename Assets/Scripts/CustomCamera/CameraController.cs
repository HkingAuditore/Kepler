using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomCamera
{
    /// <summary>
    /// 相机控制
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        /// <summary>
        ///     矫正速度
        /// </summary>
        public float correctiveSpeed;

        /// <summary>
        ///     聚焦位移
        /// </summary>
        public float focusOffset = 0.33f;

        /// <summary>
        ///     移动速度
        /// </summary>
        public float mainSpeed;

        /// <summary>
        ///     缩放速度
        /// </summary>
        public float orthoZoomSpeed;

        /// <summary>
        ///     最大缩放尺寸
        /// </summary>
        public float scaleMax = 300;

        /// <summary>
        ///     最小缩放尺寸
        /// </summary>
        public float scaleMin = 20;

        /// <summary>
        ///     标准缩放尺寸
        /// </summary>
        public float scaleSize;

        public CinemachineVirtualCamera virtualCamera;
        public CinemachineFreeLook      freeLookCamera;

        private Camera                       _camera;
        private Transform                    _cameraBase;
        private CinemachineBrain             _cameraBrain;
        private Vector2                      _dragEndPos;
        private Vector2                      _dragOriPos;
        private Transform                    _followingTarget;
        private CinemachineFramingTransposer _framingTransposer;
        private bool                         _isInDrag;
        private float                        _oriOrthoSize;
        private float                        _virtualOrthoSize;
        private float                        _freeLookOrthoSize;
        private bool                         _isFollowing = false;

        public bool IsFollowing
        {
            get => _isFollowing;
            set
            {
                _isFollowing = value;
                if (!_isFollowing)
                {
                    freeLookCamera?.gameObject.SetActive(false);
                }
            }
        }

        private float VirtualOrthoSize
        {
            get => _virtualOrthoSize;
            set => _virtualOrthoSize = Mathf.Clamp(value, scaleMin, scaleMax);
        }
        private float FreeLookOrthoSize
        {
            get => _freeLookOrthoSize;
            set => _freeLookOrthoSize = Mathf.Clamp(value, scaleMin, scaleMax);
        }

        private void Awake()
        {
            _cameraBase        = transform.parent;
            _camera            = GetComponent<Camera>();
            _cameraBrain       = GetComponent<CinemachineBrain>();
            VirtualOrthoSize   = virtualCamera.m_Lens.OrthographicSize;
            if(freeLookCamera!=null)
                FreeLookOrthoSize  = freeLookCamera.m_Lens.OrthographicSize;
            _oriOrthoSize      = VirtualOrthoSize;
            _framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Space) && (freeLookCamera == null || !freeLookCamera.gameObject.activeSelf))
                // CameraMover();
                CameraDrag();
            // if (Input.GetKey(KeyCode.LeftAlt))
            // {
            //     CameraRotator();
            //     if (Input.GetMouseButton(1))
            //     {
            //         _camera.transform.localEulerAngles = new Vector3(0, 0, 0);
            //     }
            // }
            if(freeLookCamera !=null)
            {
                if (Input.GetKeyDown(KeyCode.Z) && this.IsFollowing)
                {
                    freeLookCamera.gameObject.SetActive(!freeLookCamera.gameObject.activeSelf);
                }
                if (freeLookCamera.gameObject.activeSelf)
                    CameraRotator();
            }
            
            if (EventSystem.current.IsPointerOverGameObject() == false && !Input.GetKey(KeyCode.LeftAlt))
                CameraScaler();
            if (IsFollowing)
                Follow();
        }


        public Camera GetMainCamera()
        {
            return _camera;
        }

        private void CameraMover()
        {
            var xSpeed = 2 * ((Input.mousePosition.x - Screen.width / 2) / Screen.width);
            xSpeed = Mathf.Abs(xSpeed) > 0.95 ? xSpeed : 0;
            var ySpeed = 2 * ((Input.mousePosition.y - Screen.height / 2) / Screen.height);
            ySpeed = Mathf.Abs(ySpeed) > 0.95 ? ySpeed : 0;

            // Debug.Log("Mouse [x:" + xSpeed + ", y:" + ySpeed + "]");

            _cameraBase.position += new Vector3(xSpeed, 0, ySpeed) *
                                    (mainSpeed * Mathf.Pow(VirtualOrthoSize / _oriOrthoSize, 0.9f) * Time.deltaTime *
                                     (IsFollowing ? 0 : 1)) +
                                    new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y")) *
                                    (correctiveSpeed * Mathf.Pow(VirtualOrthoSize / _oriOrthoSize, 1.3f) * Time.deltaTime);
        }

        private void CameraDrag()
        {
            if (_isInDrag)
            {
                _dragEndPos = Input.mousePosition;
                var offset = _dragEndPos - _dragOriPos;
                _cameraBase.position -= new Vector3(offset.x, 0, offset.y) * ((IsFollowing ? 0 : 1) * 0.5f);
                _dragOriPos          =  Input.mousePosition;
            }

            if (!_isInDrag && Input.GetMouseButtonDown(0))
            {
                _dragOriPos = Input.mousePosition;
                _isInDrag   = true;
            }

            if (_isInDrag && Input.GetMouseButtonUp(0)) _isInDrag = false;
        }

        public void CameraRotator()
        {
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                freeLookCamera.m_YAxis.m_MaxSpeed = 2f;
                freeLookCamera.m_XAxis.m_MaxSpeed = 300f;
            }
            else
            {
                freeLookCamera.m_YAxis.m_MaxSpeed = 0f;
                freeLookCamera.m_XAxis.m_MaxSpeed = 0f;
            }
        }

        private void CameraScaler()
        {
            VirtualOrthoSize  -= Input.GetAxis("Mouse ScrollWheel") * scaleSize;
            virtualCamera.m_Lens.OrthographicSize =
                Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, VirtualOrthoSize, orthoZoomSpeed);
            
            if(freeLookCamera!=null && freeLookCamera.gameObject.activeSelf)
            {
                FreeLookOrthoSize -= Input.GetAxis("Mouse ScrollWheel") * scaleSize;
                freeLookCamera.m_Lens.OrthographicSize =
                    Mathf.Lerp(freeLookCamera.m_Lens.OrthographicSize, FreeLookOrthoSize, orthoZoomSpeed);
            }
            
        }

        /// <summary>
        ///     聚焦
        /// </summary>
        /// <param name="target">聚焦目标</param>
        public void FocusOn(Transform target)
        {
            virtualCamera.m_LookAt       = target;
            // virtualCamera.m_Follow       = target;
            _framingTransposer.m_ScreenX = focusOffset;
            _followingTarget             = target;
        }

        /// <summary>
        ///     退出聚焦
        /// </summary>
        public void ExitFocus()
        {
            // virtualCamera.m_LookAt       = _cameraBase;
            // virtualCamera.m_Follow       = _cameraBase;
            _framingTransposer.m_ScreenX = 0.5f;
            virtualCamera.m_LookAt       = null;
        }

        private void Follow()
        {
            _cameraBase.transform.position = new Vector3(_followingTarget.position.x, _cameraBase.transform.position.y,
                                                         _followingTarget.position.z);
        }
    }
}