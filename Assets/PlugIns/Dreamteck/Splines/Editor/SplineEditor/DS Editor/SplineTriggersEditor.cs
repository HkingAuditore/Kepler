using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class SplineTriggersEditor : SplineEditorBase
    {
        private          SplineTrigger.Type addTriggerType = SplineTrigger.Type.Double;
        private          bool               renameTrigger,    renameGroup;
        private          int                selected = -1,    selectedGroup = -1;
        private          int                setDistanceGroup, setDistanceTrigger;
        private readonly SplineComputer     spline;

        public SplineTriggersEditor(SplineComputer spline)
        {
            this.spline = spline;
        }

        protected override void Load()
        {
            base.Load();
            addTriggerType = (SplineTrigger.Type) LoadInt("addTriggerType");
        }

        protected override void Save()
        {
            base.Save();
            SaveInt("addTriggerType", (int) addTriggerType);
        }

        public override void DrawInspector()
        {
            base.DrawInspector();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical();
            for (var i = 0; i < spline.triggerGroups.Length; i++) DrawGroupGUI(i);
            EditorGUILayout.Space();
            if (GUILayout.Button("New Group"))
            {
                RecordUndo("Add Trigger Group");
                var group = new TriggerGroup();
                group.name = "Trigger Group " + (spline.triggerGroups.Length + 1);
                UnityEditor.ArrayUtility.Add(ref spline.triggerGroups, group);
            }

            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
        }

        public override void DrawScene(SceneView current)
        {
            base.DrawScene(current);
            for (var i = 0; i < spline.triggerGroups.Length; i++)
            {
                if (!spline.triggerGroups[i].open) continue;
                DrawGroupScene(i);
            }
        }

        private void DrawGroupScene(int index)
        {
            var group = spline.triggerGroups[index];
            for (var i = 0; i < group.triggers.Length; i++)
            {
                var gizmo = SplineComputerEditorHandles.SplineSliderGizmo.DualArrow;
                switch (group.triggers[i].type)
                {
                    case SplineTrigger.Type.Backward:
                        gizmo = SplineComputerEditorHandles.SplineSliderGizmo.BackwardTriangle;
                        break;
                    case SplineTrigger.Type.Forward:
                        gizmo = SplineComputerEditorHandles.SplineSliderGizmo.ForwardTriangle;
                        break;
                    case SplineTrigger.Type.Double:
                        gizmo = SplineComputerEditorHandles.SplineSliderGizmo.DualArrow;
                        break;
                }

                var last = group.triggers[i].position;
                if (SplineComputerEditorHandles.Slider(spline, ref group.triggers[i].position, group.triggers[i].color,
                                                       group.triggers[i].name, gizmo) ||
                    last != group.triggers[i].position)
                {
                    Select(index, i);
                    Repaint();
                }
            }
        }

        private void OnSetDistance(float distance)
        {
            var serializedObject = new SerializedObject(spline);
            var groups           = serializedObject.FindProperty("triggerGroups");
            var groupProperty    = groups.GetArrayElementAtIndex(setDistanceGroup);

            var triggersProperty = groupProperty.FindPropertyRelative("triggers");
            var triggerProperty  = triggersProperty.GetArrayElementAtIndex(setDistanceTrigger);

            var position = triggerProperty.FindPropertyRelative("position");

            var travel = spline.Travel(0.0, distance);
            position.floatValue = (float) travel;
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGroupGUI(int index)
        {
            var group            = spline.triggerGroups[index];
            var serializedObject = new SerializedObject(spline);
            var groups           = serializedObject.FindProperty("triggerGroups");
            var groupProperty    = groups.GetArrayElementAtIndex(index);
            EditorGUI.indentLevel += 2;
            if (selectedGroup == index && renameGroup)
            {
                if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return ||
                                                                Event.current.keyCode == KeyCode.KeypadEnter))
                {
                    renameGroup = false;
                    Repaint();
                }

                group.name = EditorGUILayout.TextField(group.name);
            }
            else
            {
                @group.open = EditorGUILayout.Foldout(@group.open, index + " - " + @group.name);
            }

            var lastRect = GUILayoutUtility.GetLastRect();
            if (lastRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown &&
                Event.current.button                                                 == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Rename"), false, delegate
                                                              {
                                                                  RecordUndo("Rename Trigger Group");
                                                                  selectedGroup = index;
                                                                  renameGroup   = true;
                                                                  renameTrigger = false;
                                                                  Repaint();
                                                              });
                menu.AddItem(new GUIContent("Delete"), false, delegate
                                                              {
                                                                  RecordUndo("Delete Trigger Group");
                                                                  UnityEditor.ArrayUtility
                                                                             .RemoveAt(ref spline.triggerGroups, index);
                                                                  Repaint();
                                                              });
                menu.ShowAsContext();
            }

            EditorGUI.indentLevel -= 2;
            if (!group.open) return;

            for (var i = 0; i < group.triggers.Length; i++) DrawTriggerGUI(i, index, groupProperty);
            if (GUI.changed) serializedObject.ApplyModifiedProperties();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Trigger"))
            {
                RecordUndo("Add Trigger");
                var newTrigger = new SplineTrigger(addTriggerType);
                newTrigger.name = "Trigger " + (group.triggers.Length + 1);
                UnityEditor.ArrayUtility.Add(ref group.triggers, newTrigger);
            }

            addTriggerType = (SplineTrigger.Type) EditorGUILayout.EnumPopup(addTriggerType);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        private void Select(int group, int trigger)
        {
            selected      = trigger;
            selectedGroup = group;
            renameTrigger = false;
            renameGroup   = false;
            Repaint();
        }

        private void DrawTriggerGUI(int index, int groupIndex, SerializedProperty groupProperty)
        {
            var isSelected       = selected == index && selectedGroup == groupIndex;
            var group            = spline.triggerGroups[groupIndex];
            var trigger          = group.triggers[index];
            var triggersProperty = groupProperty.FindPropertyRelative("triggers");
            var triggerProperty  = triggersProperty.GetArrayElementAtIndex(index);
            var eventProperty    = triggerProperty.FindPropertyRelative("onCross");
            var positionProperty = triggerProperty.FindPropertyRelative("position");
            var colorProperty    = triggerProperty.FindPropertyRelative("color");
            var nameProperty     = triggerProperty.FindPropertyRelative("name");
            var enabledProperty  = triggerProperty.FindPropertyRelative("enabled");
            var workOnceProperty = triggerProperty.FindPropertyRelative("workOnce");
            var typeProperty     = triggerProperty.FindPropertyRelative("type");

            var col = colorProperty.colorValue;
            if (isSelected) col.a = 1f;
            else col.a            = 0.6f;
            GUI.backgroundColor = col;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUI.backgroundColor = Color.white;
            if (trigger == null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("NULL");
                if (GUILayout.Button("x")) UnityEditor.ArrayUtility.RemoveAt(ref group.triggers, index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }


            if (isSelected && renameTrigger)
            {
                if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return ||
                                                                Event.current.keyCode == KeyCode.KeypadEnter))
                {
                    renameTrigger = false;
                    Repaint();
                }

                nameProperty.stringValue = EditorGUILayout.TextField(nameProperty.stringValue);
            }
            else
            {
                EditorGUILayout.LabelField(nameProperty.stringValue);
            }

            if (isSelected)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(enabledProperty);
                EditorGUILayout.PropertyField(colorProperty);

                EditorGUILayout.BeginHorizontal();
                positionProperty.floatValue = EditorGUILayout.Slider("Position", positionProperty.floatValue, 0f, 1f);
                if (GUILayout.Button("Set Distance", GUILayout.Width(85)))
                {
                    var w = EditorWindow.GetWindow<DistanceWindow>(true);
                    w.Init(OnSetDistance, spline.CalculateLength());
                    setDistanceGroup   = groupIndex;
                    setDistanceTrigger = index;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(typeProperty);
                EditorGUILayout.PropertyField(workOnceProperty);

                EditorGUILayout.PropertyField(eventProperty);
            }

            EditorGUILayout.EndVertical();

            var lastRect = GUILayoutUtility.GetLastRect();
            if (lastRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    Select(groupIndex, index);
                }
                else if (Event.current.button == 1)
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Deselect"), false, delegate { Select(-1, -1); });
                    menu.AddItem(new GUIContent("Rename"), false, delegate
                                                                  {
                                                                      Select(groupIndex, index);
                                                                      renameTrigger = true;
                                                                      renameGroup   = false;
                                                                  });
                    if (index > 0)
                        menu.AddItem(new GUIContent("Move Up"), false, delegate
                                                                       {
                                                                           RecordUndo("Move Trigger Up");
                                                                           var temp = @group.triggers[index - 1];
                                                                           @group.triggers[index - 1] = trigger;
                                                                           @group.triggers[index]     = temp;
                                                                           selected--;
                                                                           renameTrigger = false;
                                                                       });
                    else
                        menu.AddDisabledItem(new GUIContent("Move Up"));
                    if (index < group.triggers.Length - 1)
                        menu.AddItem(new GUIContent("Move Down"), false, delegate
                                                                         {
                                                                             RecordUndo("Move Trigger Down");
                                                                             var temp = @group.triggers[index + 1];
                                                                             @group.triggers[index + 1] = trigger;
                                                                             @group.triggers[index]     = temp;
                                                                             selected--;
                                                                             renameTrigger = false;
                                                                         });
                    else
                        menu.AddDisabledItem(new GUIContent("Move Down"));

                    menu.AddItem(new GUIContent("Duplicate"), false, delegate
                                                                     {
                                                                         RecordUndo("Duplicate Trigger");
                                                                         var newTrigger =
                                                                             new SplineTrigger(SplineTrigger.Type
                                                                                                            .Double);
                                                                         newTrigger.color   = colorProperty.colorValue;
                                                                         newTrigger.enabled = enabledProperty.boolValue;
                                                                         newTrigger.position =
                                                                             positionProperty.floatValue;
                                                                         newTrigger.type =
                                                                             (SplineTrigger.Type) typeProperty.intValue;
                                                                         newTrigger.name =
                                                                             "Trigger " + (group.triggers.Length + 1);
                                                                         UnityEditor.ArrayUtility
                                                                                    .Add(ref group.triggers,
                                                                                         newTrigger);
                                                                         Select(groupIndex, group.triggers.Length - 1);
                                                                     });
                    menu.AddItem(new GUIContent("Delete"), false, delegate
                                                                  {
                                                                      RecordUndo("Delete Trigger");
                                                                      UnityEditor.ArrayUtility
                                                                                 .RemoveAt(ref group.triggers, index);
                                                                      Select(-1, -1);
                                                                  });
                    menu.ShowAsContext();
                }
            }
        }
    }
}