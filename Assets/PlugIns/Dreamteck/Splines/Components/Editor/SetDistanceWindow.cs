using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public class DistanceWindow : EditorWindow
    {
        public delegate void DistanceReceiver(float distance);

        private float            distance;
        private float            length;
        private DistanceReceiver rcv;

        private void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter ||
                                                            Event.current.keyCode == KeyCode.Return))
            {
                rcv(distance);
                Close();
            }

            distance = EditorGUILayout.FloatField("Distance", distance);
            if (distance      < 0f) distance     = 0f;
            else if (distance > length) distance = length;
            if (distance > 0f) EditorGUILayout.LabelField("Press Enter to set.", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.HelpBox("Enter the distance and press Enter. Current spline length: " + length,
                                    MessageType.Info);
        }

        public void Init(DistanceReceiver receiver, float totalLength)
        {
            rcv          = receiver;
            length       = totalLength;
            titleContent = new GUIContent("Set Distance");
            minSize      = maxSize = new Vector2(240, 90);
        }
    }
}