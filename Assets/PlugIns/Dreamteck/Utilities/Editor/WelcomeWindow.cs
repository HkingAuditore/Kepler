using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dreamteck
{
    public class WelcomeWindow : EditorWindow
    {
        public delegate void EmptyHandler();

        protected static GUIStyle      wrapText;
        protected static GUIStyle      buttonTitleText;
        protected static GUIStyle      warningText;
        protected static GUIStyle      titleText;
        private static   bool          init = true;
        protected        Texture2D     header;
        protected        string        headerTitle = "";
        protected        WindowPanel[] panels      = new WindowPanel[0];

        protected void OnEnable()
        {
            init = true;
        }

        protected void OnGUI()
        {
            if (init) Load();
            if (header == null) GetHeader();
            GUI.DrawTexture(new Rect(0, 0,  maxSize.x,         82), header, ScaleMode.StretchToFill);
            GUI.Label(new Rect(90,      15, Screen.width - 95, 50), headerTitle, titleText);
            for (var i = 0; i < panels.Length; i++) panels[i].Draw();
            Repaint();
        }

        public virtual void Load()
        {
            minSize                      = maxSize = new Vector2(450, 500);
            buttonTitleText              = new GUIStyle(GUI.skin.GetStyle("label"));
            buttonTitleText.fontStyle    = FontStyle.Bold;
            titleText                    = new GUIStyle(GUI.skin.GetStyle("label"));
            titleText.fontSize           = 25;
            titleText.fontStyle          = FontStyle.Bold;
            titleText.alignment          = TextAnchor.MiddleLeft;
            titleText.normal.textColor   = Color.white;
            warningText                  = new GUIStyle(GUI.skin.GetStyle("label"));
            warningText.fontSize         = 18;
            warningText.fontStyle        = FontStyle.Bold;
            warningText.normal.textColor = Color.red;
            warningText.alignment        = TextAnchor.MiddleCenter;
            wrapText                     = new GUIStyle(GUI.skin.GetStyle("label"));
            wrapText.wordWrap            = true;
            init                         = false;
        }

        protected virtual void SetTitle(string titleBar, string header)
        {
            titleContent = new GUIContent(titleBar);
            headerTitle  = header;
        }

        protected virtual void GetHeader()
        {
            header = null;
        }

        public class WindowPanel
        {
            public enum SlideDiretion
            {
                Left,
                Right,
                Up,
                Down
            }

            public  WindowPanel   back;
            public  SlideDiretion closeDirection = SlideDiretion.Right;
            public  List<Element> elements       = new List<Element>();
            private bool          goingBack;
            private bool          open;
            public  SlideDiretion openDirection = SlideDiretion.Left;
            private Vector2       origin        = Vector2.zero;
            public  float         slideDuration = 1f;
            public  float         slideStart;

            public WindowPanel(string title, bool o, float slideDur = 1f)
            {
                slideDuration = slideDur;
                SetState(o, false);
            }

            public WindowPanel(string title, bool o, WindowPanel backPanel, float slideDur = 1f)
            {
                slideDuration = slideDur;
                SetState(o, false);
                back = backPanel;
            }

            public bool isActive => open || Time.realtimeSinceStartup - slideStart <= slideDuration;

            public void Back()
            {
                Close(true, true);
                back.Open(true, true);
            }

            public void Close(bool useTransition, bool goBack = false)
            {
                SetState(false, useTransition, goBack);
            }

            public void Open(bool useTransition, bool goBack = false)
            {
                goingBack = false;
                SetState(true, useTransition, goBack);
            }

            private Vector2 GetSize()
            {
                return new Vector2(Screen.width, Screen.height - 82);
            }

            private void HandleOrigin()
            {
                var percent        = Mathf.Clamp01((Time.realtimeSinceStartup - slideStart) / slideDuration);
                var size           = GetSize();
                var dir            = openDirection;
                if (goingBack) dir = closeDirection;
                if (open)
                    switch (dir)
                    {
                        case SlideDiretion.Left:
                            origin.x = Mathf.SmoothStep(size.x, 0f, percent);
                            origin.y = 0f;
                            break;

                        case SlideDiretion.Right:
                            origin.x = Mathf.SmoothStep(-size.x, 0f, percent);
                            origin.y = 0f;
                            break;

                        case SlideDiretion.Up:
                            origin.x = 0f;
                            origin.y = Mathf.SmoothStep(size.y, 0f, percent);
                            break;

                        case SlideDiretion.Down:
                            origin.x = 0f;
                            origin.y = Mathf.SmoothStep(-size.y, 0f, percent);
                            break;
                    }
                else
                    switch (dir)
                    {
                        case SlideDiretion.Left:
                            origin.x = Mathf.SmoothStep(0f, -size.x, percent);
                            origin.y = 0f;
                            break;

                        case SlideDiretion.Right:
                            origin.x = Mathf.SmoothStep(0f, size.x, percent);
                            origin.y = 0f;
                            break;

                        case SlideDiretion.Up:
                            origin.x = 0f;
                            origin.y = Mathf.SmoothStep(0f, -size.y, percent);
                            break;

                        case SlideDiretion.Down:
                            origin.x = 0f;
                            origin.y = Mathf.SmoothStep(0f, -size.y, percent);
                            break;
                    }
            }

            private void SetState(bool state, bool useTransition, bool goBack = false)
            {
                if (open == state) return;
                open = state;
                if (useTransition) slideStart = Time.realtimeSinceStartup;
                else slideStart               = Time.realtimeSinceStartup + slideDuration;
                goingBack = goBack;
            }

            public void Draw()
            {
                if (!isActive) return;
                HandleOrigin();
                var size = GetSize();
                GUILayout.BeginArea(new Rect(origin.x + 25, origin.y + 85, size.x - 25, size.y));
                //Back button
                if (back != null)
                    if (GUILayout.Button("◄", GUILayout.Width(45), GUILayout.Height(25)))
                        Back();

                for (var i = 0; i < elements.Count; i++) elements[i].Draw();
                GUILayout.EndArea();
            }


            public class Element
            {
                public    ActionLink action;
                protected Vector2    size = Vector2.zero;

                public Element(float x, float y, ActionLink a = null)
                {
                    size   = new Vector2(x, y);
                    action = a;
                }

                internal virtual void Draw()
                {
                }
            }

            public class Space : Element
            {
                public Space(float x, float y) : base(x, y)
                {
                }

                internal override void Draw()
                {
                    GUILayoutUtility.GetRect(size.x, size.y);
                }
            }

            public class Button : Element
            {
                private readonly string text = "";

                public Button(float x, float y, string t, ActionLink a) : base(x, y, a)
                {
                    text = t;
                }

                internal override void Draw()
                {
                    base.Draw();
                    if (GUILayout.Button(text, GUILayout.Width(size.x), GUILayout.Height(size.y)))
                        if (action != null)
                            action.Do();
                }
            }

            public class Thumbnail : Element
            {
                public           string    description = "";
                private readonly Texture2D thumbnail;
                private readonly string    thumbnailName = "";
                private readonly string    thumbnailPath = "";
                public           string    title         = "";

                public Thumbnail(string path, string fileName, string t, string d, ActionLink a, float x = 400,
                                 float  y = 60) : base(x, y, a)
                {
                    title         = t;
                    description   = d;
                    thumbnailPath = path;
                    thumbnailName = fileName;

                    thumbnail = ResourceUtility.EditorLoadTexture(thumbnailPath, thumbnailName);
                }

                internal override void Draw()
                {
                    var rect = GUILayoutUtility.GetRect(size.x, size.y);
                    var buttonColor = Color.clear;
                    if (rect.Contains(Event.current.mousePosition)) buttonColor = Color.white;
                    GUI.BeginGroup(rect);
                    GUI.color = buttonColor;
                    if (GUI.Button(new Rect(0, 0, size.x, size.y), "")) action.Do();
                    GUI.color = Color.white;
                    if (thumbnail != null)
                    {
                        var offset = new Vector2(0, (size.y - 50)    / 2);
                        GUI.DrawTexture(new Rect(offset, Vector2.one * 50), thumbnail, ScaleMode.StretchToFill);
                    }

                    GUI.Label(new Rect(60, 5,  370 - 65, 16), title,       buttonTitleText);
                    GUI.Label(new Rect(60, 20, 370 - 65, 40), description, wrapText);
                    GUI.EndGroup();
                    GUILayout.Space(10);
                }
            }

            public class ScrollText : Element
            {
                private          Vector2 scroll = Vector2.zero;
                private readonly string  text   = "";

                public ScrollText(float x, float y, string t) : base(x, y)
                {
                    text = t;
                }

                internal override void Draw()
                {
                    base.Draw();
                    scroll = GUILayout.BeginScrollView(scroll, GUILayout.Width(size.x), GUILayout.MaxHeight(size.y));
                    EditorGUILayout.LabelField(text, wrapText, GUILayout.Width(size.x - 30));
                    GUILayout.EndScrollView();
                }
            }

            public class Label : Element
            {
                private readonly Color    color;
                private readonly GUIStyle style;
                private readonly string   text = "";

                public Label(string t, GUIStyle s, Color col) : base(400, 30)
                {
                    color = col;
                    text  = t;
                    style = s;
                }

                public Label(string t, GUIStyle s, Color col, float x, float y) : base(x, y)
                {
                    color = col;
                    text  = t;
                    style = s;
                }

                internal override void Draw()
                {
                    base.Draw();
                    var prev = GUI.color;
                    GUI.color = color;
                    if (style == null)
                        EditorGUILayout.LabelField(text,  GUILayout.Width(size.x), GUILayout.Height(size.y));
                    else EditorGUILayout.LabelField(text, style, GUILayout.Width(size.x), GUILayout.Height(size.y));
                    GUI.color = prev;
                }
            }
        }

        public class ActionLink
        {
            private readonly WindowPanel  currentPanel;
            private readonly EmptyHandler customHandler;
            private readonly WindowPanel  targetPanel;
            private readonly string       URL = "";

            public ActionLink(string u)
            {
                URL = u;
            }

            public ActionLink(EmptyHandler handler)
            {
                customHandler = handler;
            }

            public ActionLink(WindowPanel target, WindowPanel current)
            {
                currentPanel = current;
                targetPanel  = target;
            }

            public void Do()
            {
                if (customHandler != null)
                {
                    customHandler();
                }
                else if (URL != "")
                {
                    Application.OpenURL(URL);
                }
                else if (targetPanel != null && currentPanel != null)
                {
                    currentPanel.Close(true);
                    targetPanel.Open(true);
                }
            }
        }
    }
}