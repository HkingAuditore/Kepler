using UnityEditor;
using UnityEngine;

namespace Dreamteck.Editor
{
    public class Toolbar
    {
        private readonly GUIContent[] allContent;
        public           bool         center        = true;
        public           float        elementHeight = 23f;
        public           float        elementWidth;
        public           bool         newLine = true;
        private readonly GUIContent[] shownContent;

        public Toolbar(GUIContent[] iconsNormal, GUIContent[] iconsSelected, float elementWidth = 0f)
        {
            this.elementWidth = elementWidth;
            if (iconsNormal.Length != iconsSelected.Length)
            {
                Debug.LogError("Invalid icon count for toolbar ");
                return;
            }

            allContent   = new GUIContent[iconsNormal.Length * 2];
            shownContent = new GUIContent[iconsNormal.Length];
            iconsNormal.CopyTo(allContent, 0);
            iconsSelected.CopyTo(allContent, iconsNormal.Length);
        }

        public void SetContent(int index, GUIContent content)
        {
            allContent[index]                       = content;
            allContent[shownContent.Length + index] = content;
        }

        public void SetContent(int index, GUIContent content, GUIContent contentSelected)
        {
            allContent[index]                       = content;
            allContent[shownContent.Length + index] = contentSelected;
        }

        public void Draw(ref int selected)
        {
            for (var i = 0; i < shownContent.Length; i++)
                shownContent[i] = selected == i ? allContent[shownContent.Length + i] : allContent[i];
            if (newLine) EditorGUILayout.BeginHorizontal();
            if (center) GUILayout.FlexibleSpace();
            if (elementWidth > 0f)
                selected = GUILayout.Toolbar(selected, shownContent,
                                             GUILayout.Width(elementWidth * shownContent.Length),
                                             GUILayout.Height(elementHeight));
            else selected = GUILayout.Toolbar(selected, shownContent, GUILayout.Height(elementHeight));
            if (center) GUILayout.FlexibleSpace();
            if (newLine) EditorGUILayout.EndHorizontal();
        }
    }
}