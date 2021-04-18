using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    [CustomEditor(typeof(SplineMorph))]
    public class SplineMorphEditor : UnityEditor.Editor
    {
        private string addName = "";

        private SplineMorph morph;
        private bool        rename;
        private int         selected = -1;

        private void OnEnable()
        {
            morph = (SplineMorph) target;
            GetAddName();
        }

        private void GetAddName()
        {
            addName = "Channel " + morph.GetChannelCount();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Undo.RecordObject(morph, "Edit Morph");
            morph.spline =
                (SplineComputer) EditorGUILayout.ObjectField("Spline", morph.spline, typeof(SplineComputer), true);
            morph.space = (SplineComputer.Space) EditorGUILayout.EnumPopup("Space", morph.space);
            morph.cycle = EditorGUILayout.Toggle("Runtime Cycle", morph.cycle);
            if (morph.cycle)
            {
                EditorGUI.indentLevel++;
                morph.cycleMode = (SplineMorph.CycleMode) EditorGUILayout.EnumPopup("Cycle Wrap", morph.cycleMode);
                morph.cycleUpdateMode =
                    (SplineMorph.UpdateMode) EditorGUILayout.EnumPopup("Update Mode", morph.cycleUpdateMode);
                morph.cycleDuration = EditorGUILayout.FloatField("Cycle Duration", morph.cycleDuration);
                EditorGUI.indentLevel--;
            }

            var channelCount = morph.GetChannelCount();
            if (channelCount > 0)
            {
                if (morph.spline == null)
                {
                    EditorGUILayout.HelpBox("No spline assigned.", MessageType.Error);
                    return;
                }

                if (morph.GetSnapshot(0).Length != morph.spline.pointCount)
                {
                    EditorGUILayout
                       .HelpBox("Recorded morphs require the spline to have " + morph.GetSnapshot(0).Length + ". The spline has " + morph.spline.pointCount,
                                MessageType.Error);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Clear morph states"))
                        if (EditorUtility.DisplayDialog("Clear morph states?", "Do you want to clear all morph states?",
                                                        "Yes", "No"))
                            morph.Clear();
                    var str                                                        = "Reduce";
                    if (morph.GetSnapshot(0).Length > morph.spline.pointCount) str = "Increase";
                    if (GUILayout.Button(str + " spline points"))
                        if (EditorUtility.DisplayDialog(str               + " spline points?",
                                                        "Do you want to " + str + " the spline points?", "Yes", "No"))
                            morph.spline.SetPoints(morph.GetSnapshot(0), SplineComputer.Space.Local);
                    EditorGUILayout.EndHorizontal();
                    return;
                }
            }

            for (var i = 0; i < channelCount; i++) DrawChannel(i);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+", GUILayout.Width(40)))
            {
                morph.AddChannel(addName);
                GetAddName();
            }

            addName = EditorGUILayout.TextField(addName);

            EditorGUILayout.EndHorizontal();
            if (GUI.changed) SceneView.RepaintAll();
        }

        private void DrawChannel(int index)
        {
            var channel = morph.GetChannel(index);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            if (selected == index && rename)
            {
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return) rename = false;
                channel.name = EditorGUILayout.TextField(channel.name);
            }
            else if (index > 0)
            {
                var weight     = morph.GetWeight(index);
                var lastWeight = weight;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("●", "Capture Snapshot"), GUILayout.Width(22f)))
                    morph.CaptureSnapshot(index);
                EditorGUILayout.LabelField(channel.name, GUILayout.Width(EditorGUIUtility.labelWidth));
                weight = EditorGUILayout.Slider(weight, 0f, 1f);
                EditorGUILayout.EndHorizontal();
                if (lastWeight != weight) morph.SetWeight(index, weight);
                var lastInterpolation = channel.interpolation;
                channel.interpolation =
                    (SplineMorph.Channel.Interpolation) EditorGUILayout.EnumPopup("Interpolation",
                                                                                  channel.interpolation);
                if (lastInterpolation != channel.interpolation) morph.UpdateMorph();

                channel.curve = EditorGUILayout.CurveField("Curve", channel.curve);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("●", "Capture Snapshot"), GUILayout.Width(22f)))
                    morph.CaptureSnapshot(index);
                GUILayout.Label(channel.name);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            var last = GUILayoutUtility.GetLastRect();
            if (last.Contains(Event.current.mousePosition))
                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 0)
                    {
                        rename   = false;
                        selected = -1;
                        Repaint();
                    }

                    if (Event.current.button == 1)
                    {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Rename"), false, delegate
                                                                      {
                                                                          rename   = true;
                                                                          selected = index;
                                                                      });
                        menu.AddItem(new GUIContent("Delete"), false, delegate
                                                                      {
                                                                          morph.SetWeight(index, 0f);
                                                                          morph.RemoveChannel(index);
                                                                          selected = -1;
                                                                          GetAddName();
                                                                      });
                        menu.ShowAsContext();
                    }
                }
        }
    }
}