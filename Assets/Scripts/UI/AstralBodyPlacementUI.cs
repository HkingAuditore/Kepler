using SpacePhysic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AstralBodyPlacementUI : MonoBehaviour
    {
        public AstralBody placePrefab;
        public GravityTracing orbits;
        public AstralBodyAddUI root;
        
        

        public Vector3 Target { get; set; }

        private bool _inPlacing = true;
        private RectTransform _verticalLine;
        private RectTransform _horizontalLine;
        private Text _rangeText;
        private Camera _camera;
        private LineRenderer _lineRenderer;
        private Transform _orbitCore;
    
        private void Awake()
        {
            _verticalLine = this.transform.Find("Vertical").GetComponent<RectTransform>();
            _horizontalLine = this.transform.Find("Horizontal").GetComponent<RectTransform>();
            _rangeText = this.transform.Find("RangeText").GetComponent<Text>();
            _lineRenderer = this.GetComponent<LineRenderer>();
            _camera = GameManager.GetGameManager.GetMainCameraController().GetMainCamera();
            _orbitCore = orbits.transform.Find("Core");
        }

        private void Update()
        {
            if (_inPlacing)
            {
                Placing();
            }
        }

        private void Placing()
        {
            Time.timeScale = 0;
            _verticalLine.position = new Vector3(Input.mousePosition.x, _verticalLine.position.y, 0);
            _horizontalLine.position = new Vector3(_horizontalLine.position.x, Input.mousePosition.y, 0);
            _lineRenderer.SetPosition(0,_camera.ScreenToWorldPoint(Input.mousePosition));
            _lineRenderer.SetPosition(1,_orbitCore.position);
            _rangeText.transform.position = _camera.WorldToScreenPoint((_lineRenderer.GetPosition(0) +
                                                                        _lineRenderer.GetPosition(1)) * 0.5f);
            _rangeText.text = Vector3.Distance(_lineRenderer.GetPosition(0), _lineRenderer.GetPosition(1)).ToString("f2")+ " m";
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Debug.Log("Mouse X: " + mousePosInWorld.x);
                Debug.Log("Mouse Y: " + mousePosInWorld.y);
                AstralBody newAstralBody = GameObject.Instantiate(placePrefab, new Vector3(mousePosInWorld.x, 0,mousePosInWorld.z),
                    Quaternion.LookRotation(new Vector3(0,0,0)), orbits.transform);
                orbits.AddTracingTarget(newAstralBody);
                _inPlacing = false;
                root.Switch2Normal();
                Time.timeScale = 1;
            }else if (Input.GetMouseButton(1))
            {
                _inPlacing = false;
                root.Switch2Normal();
                Time.timeScale = 1;
            }
        }

        public void SetPlacing() => _inPlacing = true;
    }
}
