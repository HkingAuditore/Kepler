using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform _cameraBase;
    
    private Camera _camera;

    private bool _isFollowing = false;
    public bool IsFollowing
    {
        get => _isFollowing;
        set => _isFollowing = value;
    }


    public Camera GetMainCamera() => _camera;
    
    public CinemachineVirtualCamera virtualCamera;
    private CinemachineBrain _cameraBrain;
    
    public float correctiveSpeed;
    public float mainSpeed;
    public float scaleSize;
    public float orthoZoomSpeed;
    
    private float _orthoSize;
    private float OrthoSize
    {
        get => _orthoSize;
        set => _orthoSize = Mathf.Clamp(value,20,300);
    }

    
    private Transform _followingTarget;
    

    private void Awake()
    {
        _cameraBase = transform.parent;
        _camera = this.GetComponent<Camera>();
        _cameraBrain = this.GetComponent<CinemachineBrain>();
        OrthoSize = virtualCamera.m_Lens.OrthographicSize;
    }

    private void Update()
    {
        CameraMover();
        CameraScaler();
        if(IsFollowing)
            Follow();
    }

    private void CameraMover()
    {
        var xSpeed = 2 * ((Input.mousePosition.x - Screen.width / 2) / Screen.width);
        xSpeed = Mathf.Abs(xSpeed) > 0.7 ? xSpeed : 0;
        var ySpeed = 2 * ((Input.mousePosition.y - Screen.height / 2) / Screen.height);
        ySpeed = Mathf.Abs(ySpeed) > 0.7 ? ySpeed : 0;

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
        _followingTarget = target;
        
    }

    public void Follow()
    {
        _cameraBase.transform.position = new Vector3(_followingTarget.position.x,_cameraBase.transform.position.y,_followingTarget.position.z);
    }
}