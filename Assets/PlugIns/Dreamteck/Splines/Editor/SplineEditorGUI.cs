using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines.Editor
{
    public static class SplineEditorGUI
    {
        public static readonly GUIStyle leftButtonStyle, midButtonStyle, rightButtonStyle, boxStyle;

        private static Color previousContentColor, previousBackgroundColor, highLightBGColor, highlightContentColor;


        public static readonly  GUIStyle defaultButton;
        public static readonly  GUIStyle defaultEditorButton         = null;
        public static readonly  GUIStyle defaultEditorButtonSelected = null;
        public static readonly  GUIStyle dropdownItem;
        public static readonly  GUIStyle bigButton;
        public static readonly  GUIStyle bigButtonSelected;
        public static readonly  GUIStyle labelText;
        private static readonly GUIStyle _whiteBox;
        private static readonly GUIStyle _defaultField;
        private static readonly GUIStyle _smallField;
        public static readonly  Color    inactiveColor      = new Color(0.7f, 0.7f, 0.7f, 0.3f);
        public static readonly  Color    textColor          = new Color(0.2f, 0.2f, 0.2f, 1f);
        public static readonly  Color    activeColor        = new Color(1f,   1f,   1f,   1f);
        public static readonly  Color    blackColor         = new Color(0,    0,    0,    0.7f);
        public static readonly  Color    buttonContentColor = Color.black;
        private static          bool[]   controlStates      = new bool[0];
        private static          string[] floatFieldContents = new string[0];
        private static          int      controlIndex;

        public static  float     scale = 1f;
        private static Texture2D _white;

        static SplineEditorGUI()
        {
            midButtonStyle         = new GUIStyle(GUI.skin.button);
            midButtonStyle.margin  = new RectOffset(0, 0, midButtonStyle.margin.top,  midButtonStyle.margin.bottom);
            midButtonStyle.padding = new RectOffset(3, 3, midButtonStyle.padding.top, midButtonStyle.padding.bottom);

            leftButtonStyle                = new GUIStyle(midButtonStyle);
            leftButtonStyle.contentOffset  = new Vector2(-leftButtonStyle.border.left * 0.5f, 0f);
            rightButtonStyle               = new GUIStyle(midButtonStyle);
            rightButtonStyle.contentOffset = new Vector2(rightButtonStyle.border.right * 0.5f, 0f);

            boxStyle                   = new GUIStyle(GUI.skin.GetStyle("box"));
            boxStyle.normal.background = DreamteckEditorGUI.blankImage;
            boxStyle.margin            = new RectOffset(0, 0, 0, 2);

            defaultButton                   = new GUIStyle(GUI.skin.GetStyle("button"));
            _whiteBox                       = new GUIStyle(GUI.skin.GetStyle("box"));
            _whiteBox.normal.background     = white;
            _defaultField                   = new GUIStyle(GUI.skin.GetStyle("textfield"));
            _defaultField.normal.background = white;
            _defaultField.normal.textColor  = Color.white;
            defaultField.alignment          = TextAnchor.MiddleLeft;
            _smallField                     = new GUIStyle(GUI.skin.GetStyle("textfield"));
            _smallField.normal.background   = white;
            _smallField.normal.textColor    = Color.white;
            _smallField.alignment           = TextAnchor.MiddleLeft;
            _smallField.clipping            = TextClipping.Clip;
            labelText                       = new GUIStyle(GUI.skin.GetStyle("label"));
            labelText.fontStyle             = FontStyle.Bold;
            labelText.alignment             = TextAnchor.MiddleRight;
            labelText.normal.textColor      = Color.white;
            dropdownItem                    = new GUIStyle(GUI.skin.GetStyle("button"));
            dropdownItem.normal.background  = white;
            dropdownItem.normal.textColor   = Color.white;
            dropdownItem.alignment          = TextAnchor.MiddleLeft;
            bigButton                       = new GUIStyle(GUI.skin.GetStyle("button"));
            bigButton.fontStyle             = FontStyle.Bold;
            bigButton.normal.textColor      = buttonContentColor;
            bigButtonSelected               = new GUIStyle(GUI.skin.GetStyle("button"));
            bigButtonSelected.fontStyle     = FontStyle.Bold;
            buttonContentColor              = defaultButton.normal.textColor;
            //If the button text color is too dark, generate a brightened version
            var avg = (buttonContentColor.r + buttonContentColor.g + buttonContentColor.b) / 3f;
            if (avg <= 0.2f) buttonContentColor = new Color(0.2f, 0.2f, 0.2f);
            Rescale();
        }

        public static GUIStyle whiteBox
        {
            get
            {
                if (_whiteBox.normal.background == null) _whiteBox.normal.background = white;
                return _whiteBox;
            }
        }

        public static GUIStyle defaultField
        {
            get
            {
                if (_defaultField.normal.background == null) _defaultField.normal.background = white;
                return _defaultField;
            }
        }

        public static GUIStyle smallField
        {
            get
            {
                if (_smallField.normal.background == null) _smallField.normal.background = white;
                return _smallField;
            }
        }

        public static Texture2D white
        {
            get
            {
                if (_white == null)
                {
                    _white = new Texture2D(1, 1);
                    _white.SetPixel(0, 0, Color.white);
                    _white.Apply();
                }

                return _white;
            }
        }

        public static void Update()
        {
            controlStates      = new bool[0];
            floatFieldContents = new string[0];
        }

        public static void Reset()
        {
            controlIndex = 0;
        }

        public static void SetHighlightColors(Color background, Color content)
        {
            highLightBGColor      = background;
            highlightContentColor = content;
        }

        public static bool LeftButton(GUIContent content, bool selected)
        {
            var clicked = false;
            var rect    = ButtonBegin(selected, leftButtonStyle);
            if (GUI.Button(new Rect(0, 0, rect.width + leftButtonStyle.border.right, rect.height), content,
                           leftButtonStyle)) clicked = true;
            ButtonEnd();
            return clicked;
        }

        public static bool MidButton(GUIContent content, bool selected)
        {
            var clicked = false;
            var rect    = ButtonBegin(selected, midButtonStyle);
            if (
                GUI.Button(new Rect(-midButtonStyle.border.left, 0, rect.width + midButtonStyle.border.left + midButtonStyle.border.right, rect.height),
                           content, midButtonStyle)) clicked = true;
            ButtonEnd();
            return clicked;
        }

        public static bool RightButton(GUIContent content, bool selected)
        {
            var clicked = false;
            var rect    = ButtonBegin(selected, rightButtonStyle);
            if (
                GUI.Button(new Rect(-rightButtonStyle.border.left, 0, rect.width + rightButtonStyle.border.left, rect.height),
                           content, rightButtonStyle)) clicked = true;
            ButtonEnd();
            return clicked;
        }

        private static Rect ButtonBegin(bool selected, GUIStyle style)
        {
            previousContentColor    = GUI.contentColor;
            previousBackgroundColor = GUI.backgroundColor;
            GUI.contentColor        = style.normal.textColor;
            if (selected)
            {
                GUI.backgroundColor = highLightBGColor;
                GUI.contentColor    = highlightContentColor;
            }

            var rect = GUILayoutUtility.GetRect(30f, 22f);
            GUI.BeginGroup(rect);
            return rect;
        }

        public static void ButtonEnd()
        {
            GUI.EndGroup();
            GUI.contentColor    = previousContentColor;
            GUI.backgroundColor = previousBackgroundColor;
        }

        public static int ButtonRibbon(GUIContent[] contents, float buttonWidth, int highLighted = -1)
        {
            var selected = -1;
            var width    = contents.Length * buttonWidth;
            EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
            for (var i = 0; i < contents.Length; i++)
                if (i == 0)
                {
                    if (LeftButton(contents[i], highLighted == i)) selected = i;
                }
                else if (i == contents.Length - 1)
                {
                    if (RightButton(contents[i], highLighted == i)) selected = i;
                }
                else
                {
                    if (MidButton(contents[i], highLighted == i)) selected = i;
                }

            EditorGUILayout.EndHorizontal();
            return selected;
        }


        public static void BeginContainerBox(ref bool open, string name)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
            GUI.color = new Color(1f, 1f, 1f, 0.7f);
            open      = Foldout(open, name, true);
            GUI.color = Color.white;
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        public static void EndContainerBox()
        {
            EditorGUILayout.EndVertical();
        }

        public static bool Foldout(bool foldout, GUIContent content, bool toggleOnLabelClick)
        {
            var position = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f, EditorStyles.foldout);
            return EditorGUI.Foldout(position, foldout, content, toggleOnLabelClick, EditorStyles.foldout);
        }

        public static bool Foldout(bool foldout, string content, bool toggleOnLabelClick)
        {
            return Foldout(foldout, new GUIContent(content), toggleOnLabelClick);
        }

        private static void Rescale()
        {
            defaultField.padding = new RectOffset(Mathf.RoundToInt(5 * scale), Mathf.RoundToInt(5 * scale),
                                                  Mathf.RoundToInt(5 * scale), Mathf.RoundToInt(5 * scale));
            smallField.padding = new RectOffset(Mathf.RoundToInt(2 * scale), Mathf.RoundToInt(2 * scale),
                                                Mathf.RoundToInt(2 * scale), Mathf.RoundToInt(2 * scale));
            dropdownItem.padding = new RectOffset(Mathf.RoundToInt(10 * scale), 0, 0, 0);
            bigButton.padding = new RectOffset(Mathf.RoundToInt(3 * scale), Mathf.RoundToInt(3 * scale),
                                               Mathf.RoundToInt(3 * scale), Mathf.RoundToInt(3 * scale));
            bigButtonSelected.normal.textColor = new Color(0.95f, 0.95f, 0.95f);
            bigButton.padding = new RectOffset(Mathf.RoundToInt(4 * scale), Mathf.RoundToInt(4 * scale),
                                               Mathf.RoundToInt(4 * scale), Mathf.RoundToInt(4 * scale));
            bigButton.fontSize         = Mathf.RoundToInt(30 * scale);
            bigButtonSelected.fontSize = Mathf.RoundToInt(30 * scale);
            defaultButton.fontSize     = Mathf.RoundToInt(14 * scale);
            dropdownItem.fontSize      = Mathf.RoundToInt(12 * scale);
            labelText.fontSize         = Mathf.RoundToInt(12 * scale);
            defaultField.fontSize      = Mathf.RoundToInt(14 * scale);
            smallField.fontSize        = Mathf.RoundToInt(11 * scale);
        }

        public static void SetScale(float s)
        {
            if (s != scale)
            {
                scale = s;
                Rescale();
            }

            scale = s;
        }

        public static bool EditorLayoutSelectableButton(GUIContent content, bool active = true, bool selected = false,
                                                        params GUILayoutOption[] options)
        {
            var prevColor           = GUI.color;
            var prevContentColor    = GUI.contentColor;
            var prevBackgroundColor = GUI.backgroundColor;
            var selectedStyle       = GUI.skin.button;
            if (!active)
            {
                GUI.color = inactiveColor;
            }
            else
            {
                GUI.color = activeColor;
                if (selected)
                {
                    GUI.backgroundColor            = highLightBGColor;
                    GUI.contentColor               = highlightContentColor;
                    selectedStyle                  = new GUIStyle(selectedStyle);
                    selectedStyle.normal.textColor = Color.white;
                    selectedStyle.hover.textColor  = Color.white;
                    selectedStyle.active.textColor = Color.white;
                }
                else
                {
                    GUI.contentColor = buttonContentColor;
                }
            }

            var clicked = GUILayout.Button(content, selectedStyle, options);
            GUI.color           = prevColor;
            GUI.contentColor    = prevContentColor;
            GUI.backgroundColor = prevBackgroundColor;
            return clicked && active;
        }

        private static string CleanStringForFloat(string input)
        {
            if (Regex.Match(input, @"^-?[0-9]*(?:\.[0-9]*)?$").Success)
                return input;
            return "0";
        }

        private static void HandleControlsCount()
        {
            if (controlIndex >= controlStates.Length)
            {
                var newStates = new bool[controlStates.Length + 1];
                controlStates.CopyTo(newStates, 0);
                controlStates = newStates;

                var newContents = new string[controlStates.Length + 1];
                floatFieldContents.CopyTo(newContents, 0);
                floatFieldContents = newContents;
            }
        }

#if DREAMTECK_SPLINES
        public static double ScreenPointToSplinePercent(SplineComputer computer, Vector2 screenPoint)
        {
            var points = computer.GetPoints();
            var closestDistance = (screenPoint - HandleUtility.WorldToGUIPoint(points[0].position)).sqrMagnitude;
            var closestPercent = 0.0;
            var add = computer.moveStep;
            if (computer.type == Spline.Type.Linear) add /= 2f;
            var count = 0;
            for (var i = add; i < 1.0; i += add)
            {
                var result = computer.Evaluate(i);
                var point  = HandleUtility.WorldToGUIPoint(result.position);
                var dist   = (point - screenPoint).sqrMagnitude;
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestPercent  = i;
                }

                count++;
            }

            return closestPercent;
        }
#endif
    }
}