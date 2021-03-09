using UnityEngine;
using UnityEngine.UI;

public class VelocityEditorUI : MonoBehaviour
{
    public  LineRenderer velocityLine;
    public  AstralBody   editingTarget;
    public  Text         speedUI;
    public  float        showSize = .1f;
    public  VectorUI     velocityUI;
    private Camera       _camera;
    private Vector3      _mousePos;

    private Vector3 _velocity;

    private protected float Speed
    {
        get
        {
            var oriPos       = new Vector3(velocityLine.GetPosition(0).x, 0, velocityLine.GetPosition(0).z);
            var tarPos       = new Vector3(velocityLine.GetPosition(1).x, 0, velocityLine.GetPosition(1).z);
            var oriScreenPos = _camera.WorldToScreenPoint(oriPos);
            var tarScreenPos = _camera.WorldToScreenPoint(tarPos);

            return Vector3.Magnitude((tarScreenPos - oriScreenPos) * showSize);
        }
    }

    private void Start()
    {
        velocityLine.SetPosition(0, editingTarget.transform.position);
        _camera = GameManager.GetGameManager.GetMainCameraController().GetMainCamera();
    }

    private void Update()
    {
        // var fontSize = (int) (_camera.orthographicSize / 185 * 12);

        EditVelocity();

        if (Input.GetMouseButton(1))
            gameObject.SetActive(false);
        else if (Input.GetMouseButtonUp(0)) SetVelocity();
    }

    public void OnDisable()
    {
        Time.timeScale = 1;
    }

    private void EditVelocity()
    {
        velocityUI.gameObject.SetActive(false);
        Time.timeScale = 0;
        _mousePos      = _camera.ScreenToWorldPoint(Input.mousePosition);
        velocityLine.SetPosition(0, editingTarget.transform.position);
        velocityLine.SetPosition(1, new Vector3(_mousePos.x, editingTarget.transform.position.y, _mousePos.z));

        // Vector3 tmpScreenPos = Input.mousePosition;
        // Debug.Log(tmpScreenPos);
        // Debug.Log(this.gameObject.name + " : " + tmpScreenPos);
        transform.position = new Vector3(Mathf.Clamp(Input.mousePosition.x, 60, Screen.width  - 60),
                                         Mathf.Clamp(Input.mousePosition.y, 20, Screen.height - 20),
                                         0);

        speedUI.text = "速度：" + Speed.ToString("f2") + " m/s";
    }

    private void SetVelocity()
    {
        _velocity = _mousePos - editingTarget.transform.position;
        editingTarget.ChangeVelocity(new Vector3(_velocity.x, 0, _velocity.z) * showSize);
        gameObject.SetActive(false);
        Time.timeScale = 1;
        velocityUI.gameObject.SetActive(true);
    }
}