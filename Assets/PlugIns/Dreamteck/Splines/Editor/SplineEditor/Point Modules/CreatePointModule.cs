using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class CreatePointModule : PointModule
    {
        public enum AppendMode
        {
            Beginning,
            End
        }

        public enum NormalMode
        {
            Default,
            LookAtCamera,
            AlignWithCamera,
            Calculate,
            Left,
            Right,
            Up,
            Down,
            Forward,
            Back
        }

        public enum PlacementMode
        {
            YPlane,
            XPlane,
            ZPlane,
            CameraPlane,
            Surface,
            Insert
        }

        public    AppendMode    appendMode       = AppendMode.End;
        protected Vector3       createPoint      = Vector3.zero, createNormal = Vector3.up;
        public    Color         createPointColor = Color.white;
        public    float         createPointSize  = 1f;
        protected Camera        editorCamera;
        protected SplineSample  evalResult  = new SplineSample();
        protected int           lastCreated = -1;
        public    NormalMode    normalMode  = NormalMode.Default;
        public    float         offset;
        protected PlacementMode placementMode = PlacementMode.YPlane;
        public    LayerMask     surfaceLayerMask;
        protected Spline        visualizer;

        public CreatePointModule(SplineEditor editor) : base(editor)
        {
        }

        public override GUIContent GetIconOff()
        {
            return IconContent("+", "add", "Add Points");
        }

        public override GUIContent GetIconOn()
        {
            return IconContent("+", "add_on", "Add Points");
        }

        public override void LoadState()
        {
            base.LoadState();
            normalMode       = (NormalMode) LoadInt("normalMode");
            placementMode    = (PlacementMode) LoadInt("placementMode");
            appendMode       = (AppendMode) LoadInt("appendMode");
            offset           = LoadFloat("offset");
            surfaceLayerMask = LoadInt("surfaceLayerMask", ~0);
        }

        public override void SaveState()
        {
            base.SaveState();
            SaveInt("normalMode",    (int) normalMode);
            SaveInt("placementMode", (int) placementMode);
            SaveInt("appendMode",    (int) appendMode);
            SaveFloat("offset", offset);
            SaveInt("surfaceLayerMask", surfaceLayerMask);
        }

        public override void Deselect()
        {
            base.Deselect();
            GUIUtility.hotControl = -1;
            if (Event.current != null) Event.current.Use();
        }

        public override void DrawInspector()
        {
            placementMode = (PlacementMode) EditorGUILayout.EnumPopup("Placement Mode", placementMode);
            if (placementMode != PlacementMode.Insert)
            {
                normalMode = (NormalMode) EditorGUILayout.EnumPopup("Normal Mode", normalMode);
                appendMode = (AppendMode) EditorGUILayout.EnumPopup("Append To",   appendMode);
            }

            var offsetLabel                                             = "Grid Offset";
            if (placementMode == PlacementMode.CameraPlane) offsetLabel = "Far Plane";
            if (placementMode == PlacementMode.Surface) offsetLabel     = "Surface Offset";
            offset = EditorGUILayout.FloatField(offsetLabel, offset);
            if (placementMode == PlacementMode.Surface)
                surfaceLayerMask = DreamteckEditorGUI.LayermaskField("Surface Mask", surfaceLayerMask);
        }

        public override void DrawScene()
        {
            editorCamera = SceneView.currentDrawingSceneView.camera;
            var canCreate = false;
            if (placementMode == PlacementMode.CameraPlane)
            {
                GetCreatePointOnPlane(-editorCamera.transform.forward,
                                      editorCamera.transform.position + editorCamera.transform.forward * offset,
                                      out createPoint);
                Handles.color = new Color(1f, 0.78f, 0.12f);
                DrawGrid(createPoint, editorCamera.transform.forward, Vector2.one * 10, 2.5f);
                Handles.color = Color.white;
                canCreate     = true;
                createNormal  = -editorCamera.transform.forward;
            }

            if (placementMode == PlacementMode.Surface)
            {
                var        ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, surfaceLayerMask))
                {
                    canCreate     = true;
                    createPoint   = hit.point + hit.normal * offset;
                    Handles.color = Color.blue;
                    Handles.DrawLine(hit.point, createPoint);
                    SplineEditorHandles.DrawRectangle(createPoint,
                                                      Quaternion.LookRotation(-editorCamera.transform.forward,
                                                                              editorCamera.transform.up),
                                                      HandleUtility.GetHandleSize(createPoint) * 0.1f);
                    Handles.color = Color.white;
                    createNormal  = hit.normal;
                }
            }

            if (placementMode == PlacementMode.XPlane)
            {
                canCreate    = AxisGrid(Vector3.right, new Color(0.85f, 0.24f, 0.11f, 0.92f), out createPoint);
                createNormal = Vector3.right;
            }

            if (placementMode == PlacementMode.YPlane)
            {
                canCreate    = AxisGrid(Vector3.up, new Color(0.6f, 0.95f, 0.28f, 0.92f), out createPoint);
                createNormal = Vector3.up;
            }

            if (placementMode == PlacementMode.ZPlane)
            {
                canCreate    = AxisGrid(Vector3.forward, new Color(0.22f, 0.47f, 0.97f, 0.92f), out createPoint);
                createNormal = Vector3.back;
            }

            if (placementMode == PlacementMode.Insert)
            {
                canCreate = true;
                if (points.Length < 2)
                    placementMode = PlacementMode.YPlane;
                else
                    InsertMode(Event.current.mousePosition);
            }
            else if (eventModule.mouseLeftDown && canCreate && !eventModule.mouseRight && !eventModule.alt)
            {
                CreateSplinePoint(createPoint, createNormal);
            }

            if (lastCreated >= 0 && lastCreated < points.Length && editor.eventModule.mouseLeft)
            {
                var tangent                               = points[lastCreated].position - createPoint;
                if (appendMode == AppendMode.End) tangent = createPoint - points[lastCreated].position;
                points[lastCreated].SetTangent2Position(points[lastCreated].position + tangent);
            }
            else if (!editor.eventModule.mouseLeft)
            {
                lastCreated = -1;
            }


            if (!canCreate) DrawMouseCross();
            UpdateVisualizer();
            SplineDrawer.DrawSpline(visualizer, color);
            Repaint();
        }

        protected virtual void CreateSplinePoint(Vector3 position, Vector3 normal)
        {
            RecordUndo("Create Point");
            GUIUtility.hotControl = -1;
            AddPoint();
        }

        protected void AddPoint()
        {
            var newPoint = new SplinePoint(createPoint, createPoint);
            newPoint.size  = createPointSize;
            newPoint.color = createPointColor;
            var newPoints = new SplinePoint[points.Length];
            points.CopyTo(newPoints, 0);
            if (appendMode == AppendMode.End)
            {
                if (isClosed)
                {
                    ArrayUtility.Insert(ref newPoints, newPoints.Length - 1, newPoint);
                    lastCreated = newPoints.Length - 2;
                }
                else
                {
                    ArrayUtility.Add(ref newPoints, newPoint);
                    lastCreated = newPoints.Length - 1;
                }
            }
            else
            {
                if (isClosed)
                {
                    ArrayUtility.Insert(ref newPoints, 1, newPoint);
                    lastCreated = 1;
                }
                else
                {
                    ArrayUtility.Insert(ref newPoints, 0, newPoint);
                    lastCreated = 0;
                }
            }

            if (isClosed) newPoints[newPoints.Length - 1] = newPoints[0];
            points = newPoints;
            SetPointNormal(lastCreated, createNormal);
            SelectPoint(lastCreated);
        }

        protected void SetPointNormal(int index, Vector3 defaultNormal)
        {
            if (editor.is2D)
            {
                points[index].normal = Vector3.back;
                return;
            }

            if (normalMode == NormalMode.Default)
            {
                points[index].normal = defaultNormal;
            }
            else
            {
                var editorCamera = SceneView.lastActiveSceneView.camera;
                switch (normalMode)
                {
                    case NormalMode.AlignWithCamera:
                        points[index].normal = editorCamera.transform.forward;
                        break;
                    case NormalMode.LookAtCamera:
                        points[index].normal =
                            Vector3.Normalize(editorCamera.transform.position - points[index].position);
                        break;
                    case NormalMode.Calculate:
                        PointNormalModule.CalculatePointNormal(points, index, isClosed);
                        break;
                    case NormalMode.Left:
                        points[index].normal = Vector3.left;
                        break;
                    case NormalMode.Right:
                        points[index].normal = Vector3.right;
                        break;
                    case NormalMode.Up:
                        points[index].normal = Vector3.up;
                        break;
                    case NormalMode.Down:
                        points[index].normal = Vector3.down;
                        break;
                    case NormalMode.Forward:
                        points[index].normal = Vector3.forward;
                        break;
                    case NormalMode.Back:
                        points[index].normal = Vector3.back;
                        break;
                }
            }
        }

        protected virtual void InsertMode(Vector3 screenCoordinates)
        {
            var percent = ProjectScreenSpace(screenCoordinates);
            editor.evaluate(percent, evalResult);
            if (editor.eventModule.mouseRight)
            {
                SplineEditorHandles.DrawCircle(evalResult.position,
                                               Quaternion.LookRotation(editorCamera.transform.position -
                                                                       evalResult.position),
                                               HandleUtility.GetHandleSize(evalResult.position) * 0.2f);
                return;
            }

            if (SplineEditorHandles.CircleButton(evalResult.position,
                                                 Quaternion.LookRotation(editorCamera.transform.position -
                                                                         evalResult.position),
                                                 HandleUtility.GetHandleSize(evalResult.position) * 0.2f, 1.5f, color))
            {
                RecordUndo("Create Point");
                var newPoint = new SplinePoint(evalResult.position, evalResult.position);
                newPoint.size   = evalResult.size;
                newPoint.color  = evalResult.color;
                newPoint.normal = evalResult.up;
                var newPoints  = new SplinePoint[points.Length + 1];
                var floatIndex = (points.Length - 1) * percent;
                var pointIndex = Mathf.Clamp(DMath.FloorInt(floatIndex), 0, points.Length - 2);
                for (var i = 0; i < newPoints.Length; i++)
                    if (i      <= pointIndex) newPoints[i]     = points[i];
                    else if (i == pointIndex + 1) newPoints[i] = newPoint;
                    else newPoints[i]                          = points[i - 1];
                points = newPoints;
                SelectPoint(pointIndex);
            }
        }

        protected double ProjectScreenSpace(Vector2 screenPoint)
        {
            var closestDistance = (screenPoint - HandleUtility.WorldToGUIPoint(points[0].position)).sqrMagnitude;
            var closestPercent = 0.0;
            var moveStep = 1.0 / ((editor.points.Length - 1) * sampleRate);
            var add = moveStep;
            if (splineType == Spline.Type.Linear) add /= 2.0;
            var count = 0;
            for (var i = add; i < 1.0; i += add)
            {
                editor.evaluate(i, evalResult);
                var point = HandleUtility.WorldToGUIPoint(evalResult.position);
                var dist  = (point - screenPoint).sqrMagnitude;
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestPercent  = i;
                }

                count++;
            }

            return closestPercent;
        }

        private bool GetCreatePointOnPlane(Vector3 normal, Vector3 origin, out Vector3 result)
        {
            var   plane = new Plane(normal, origin);
            var   ray   = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float rayDistance;
            if (plane.Raycast(ray, out rayDistance))
            {
                result = ray.GetPoint(rayDistance);
                return true;
            }

            if (normal == Vector3.zero)
            {
                result = origin;
                return true;
            }

            result = ray.GetPoint(0f);
            return true;
        }


        private bool AxisGrid(Vector3 axis, Color color, out Vector3 origin)
        {
            var dot            = Vector3.Dot(editorCamera.transform.position.normalized, axis);
            if (dot < 0f) axis = -axis;
            var   plane        = new Plane(axis, Vector3.zero);
            var   ray          = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float rayDistance;
            if (plane.Raycast(ray, out rayDistance))
            {
                origin = ray.GetPoint(rayDistance) + axis * offset;
                Handles.color = color;
                var distance = 1f;
                ray = new Ray(editorCamera.transform.position, -axis);
                if (!editorCamera.orthographic && plane.Raycast(ray, out rayDistance))
                    distance = Vector3.Distance(editorCamera.transform.position + axis * offset, origin);
                else if (editorCamera.orthographic) distance = 2f * editorCamera.orthographicSize;
                DrawGrid(origin, axis, Vector2.one * distance * 0.3f, distance * 2.5f * 0.03f);
                Handles.DrawLine(origin, origin - axis * offset);
                Handles.color = Color.white;
                return true;
            }

            origin = Vector3.zero;
            return false;
        }

        private void DrawGrid(Vector3 center, Vector3 normal, Vector2 size, float scale)
        {
            var right = Vector3.Cross(Vector3.up, normal).normalized;
            if (Mathf.Abs(Vector3.Dot(Vector3.up, normal)) >= 0.9999f)
                right = Vector3.Cross(Vector3.forward, normal).normalized;
            var up         = Vector3.Cross(normal, right).normalized;
            var startPoint = center - right * size.x * 0.5f + up * size.y * 0.5f;
            var i          = 0f;
            var add        = scale;
            while (i <= size.x)
            {
                var point = startPoint        + right * i;
                Handles.DrawLine(point, point - up    * size.y);
                i += add;
            }

            i   = 0f;
            add = scale;
            while (i <= size.x)
            {
                var point = startPoint        - up    * i;
                Handles.DrawLine(point, point + right * size.x);
                i += add;
            }
        }

        private void DrawMouseCross()
        {
            var ray    = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var origin = ray.GetPoint(1f);
            var size   = 0.4f * HandleUtility.GetHandleSize(origin);
            var a      = origin + editorCamera.transform.up * size - editorCamera.transform.right * size;
            var b      = origin - editorCamera.transform.up * size + editorCamera.transform.right * size;
            Handles.color = Color.red;
            Handles.DrawLine(a, b);
            a = origin - editorCamera.transform.up * size - editorCamera.transform.right * size;
            b = origin + editorCamera.transform.up * size + editorCamera.transform.right * size;
            Handles.DrawLine(a, b);
            Handles.color = Color.white;
        }

        private void UpdateVisualizer()
        {
            if (visualizer == null) visualizer = new Spline(splineType);
            visualizer.type       = splineType;
            visualizer.sampleRate = sampleRate;
            if (placementMode == PlacementMode.Insert)
            {
                visualizer.points = points;
                if (isClosed) visualizer.Close();
                else if (visualizer.isClosed) visualizer.Break();
                return;
            }

            if (visualizer.points.Length != points.Length + 1) visualizer.points = new SplinePoint[points.Length + 1];
            var newPoint = new SplinePoint(createPoint, createPoint, createNormal, 1f, Color.white);
            if (appendMode == AppendMode.End)
            {
                if (isClosed)
                {
                    for (var i = 0; i < points.Length; i++) visualizer.points[i] = points[i];
                    visualizer.points[visualizer.points.Length - 2] = newPoint;
                    visualizer.points[visualizer.points.Length - 1] = points[points.Length - 1];
                }
                else
                {
                    for (var i = 0; i < points.Length; i++) visualizer.points[i] = points[i];
                    visualizer.points[visualizer.points.Length - 1] = newPoint;
                }
            }
            else
            {
                if (isClosed)
                {
                    for (var i = 1; i < points.Length; i++) visualizer.points[i] = points[i - 1];
                    visualizer.points[1] = newPoint;
                    visualizer.points[0] = points[0];
                }
                else
                {
                    for (var i = 1; i < visualizer.points.Length; i++) visualizer.points[i] = points[i - 1];
                    visualizer.points[0] = newPoint;
                }
            }

            if (isClosed && !visualizer.isClosed) visualizer.Close();
            else if (visualizer.isClosed) visualizer.Break();
            if (visualizer.isClosed) visualizer.points[0].SetPosition(createPoint);
        }
    }
}