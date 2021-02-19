using SpacePhysic;
using UnityEngine;

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
    
        private void Awake()
        {
            _verticalLine = this.transform.Find("Vertical").GetComponent<RectTransform>();
            _horizontalLine = this.transform.Find("Horizontal").GetComponent<RectTransform>();
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
            _verticalLine.position = new Vector3(Input.mousePosition.x, _verticalLine.position.y, 0);
            _horizontalLine.position = new Vector3(_horizontalLine.position.x, Input.mousePosition.y, 0);
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
            }
        }

        public void SetPlacing() => _inPlacing = true;
    }
}
