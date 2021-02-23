using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;

    public float correctiveSpeed;
    public float mainSpeed;
    public float scaleSize;
    public float orthoZoomSpeed;

    private Camera _camera;
    private Transform _cameraBase;
    private CinemachineBrain _cameraBrain;


    private Transform _followingTarget;
    private CinemachineFramingTransposer _framingTransposer;


    private float _orthoSize;

    public bool IsFollowing { get; set; } = false;

    private float OrthoSize
    {
        get => _orthoSize;
        set => _orthoSize = Mathf.Clamp(value, 20, 300);
    }

    private void Awake()
    {
        _cameraBase = transform.parent;
        _camera = GetComponent<Camera>();
        _cameraBrain = GetComponent<CinemachineBrain>();
        OrthoSize = virtualCamera.m_Lens.OrthographicSize;
        _framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    private void Update()
    {
        if (!Input.GetKey(KeyCode.LeftAlt))
            CameraMover();
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

        _cameraBase.position += new Vector3(xSpeed, 0, ySpeed) * (mainSpeed * Time.deltaTime) * (IsFollowing ? 0 : 1) +
                                new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y")) *
                                (correctiveSpeed * Time.deltaTime);
    }

    private void CameraScaler()
    {
        OrthoSize += Input.GetAxis("Mouse ScrollWheel") * scaleSize;
        virtualCamera.m_Lens.OrthographicSize =
            Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, OrthoSize, orthoZoomSpeed);
    }


    public void FocusOn(Transform target)
    {
        virtualCamera.m_LookAt = target;
        _framingTransposer.m_ScreenX = 0.33f;
        _followingTarget = target;
    }

    public void ExitFocus()
    {
        _framingTransposer.m_ScreenX = 0.5f;
    }

    public void Follow()
    {
        _cameraBase.transform.position = new Vector3(_followingTarget.position.x, _cameraBase.transform.position.y,
                                                     _followingTarget.position.z);
    }
}