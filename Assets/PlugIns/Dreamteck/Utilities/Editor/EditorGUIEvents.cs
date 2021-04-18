using UnityEngine;

namespace Dreamteck
{
    public class EditorGUIEvents
    {
        public delegate void CommandHandler(string command);

        public delegate void EmptyHandler();

        public delegate void KeyCodeHandler(KeyCode code);

        public delegate void MouseHandler(int button);

        public bool    alt;
        public bool    control;
        public bool    enterDown;
        public Vector2 lastClickPoint = Vector2.zero;
        public bool    mouseLeft;
        public bool    mouseLeftDown;
        public bool    mouseLeftUp;
        public bool    mouseRight;
        public bool    mouseRightDown;
        public bool    mouseRightUp;
        public Vector2 mousPos = Vector2.zero;
        public bool    shift;
        public bool    v;

        public Vector2 mouseClickDelta => Event.current.mousePosition - lastClickPoint;

        public event CommandHandler onCommand;
        public event KeyCodeHandler onkeyDown;
        public event KeyCodeHandler onKeyUp;
        public event MouseHandler   onMouseDown;
        public event MouseHandler   onMouseUp;

        public void Use()
        {
            mouseLeft      = false;
            mouseRight     = false;
            mouseLeftDown  = false;
            mouseRightDown = false;
            mouseLeftUp    = false;
            mouseRightUp   = false;
            control        = false;
            shift          = false;
            alt            = false;
            Event.current.Use();
        }

        public void Update()
        {
            ListenInput(Event.current);
        }

        public void Update(Event current)
        {
            ListenInput(current);
        }

        private void ListenInput(Event e)
        {
            //int controlID = GUIUtility.GetControlID(FocusType.Passive);
            mousPos       = e.mousePosition;
            mouseLeftDown = mouseLeftUp = mouseRightDown = mouseRightUp = false;
            control       = e.control;
            shift         = e.shift;
            alt           = e.alt;
            enterDown     = false;
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        mouseLeftDown  = true;
                        mouseLeft      = true;
                        lastClickPoint = e.mousePosition;
                    }

                    if (e.button    == 1) mouseRightDown = mouseRight = true;
                    if (onMouseDown != null) onMouseDown(e.button);
                    break;
                case EventType.MouseUp:
                    if (e.button == 0)
                    {
                        mouseLeftUp = true;
                        mouseLeft   = false;
                    }

                    if (e.button == 1)
                    {
                        mouseRightDown = true;
                        mouseRight     = false;
                    }

                    if (onMouseUp != null) onMouseUp(e.button);
                    break;

                case EventType.KeyDown:
                    if (onkeyDown != null) onkeyDown(e.keyCode);
                    if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) enterDown = true;
                    if (e.keyCode == KeyCode.V) v                                                  = true;
                    break;

                case EventType.KeyUp:
                    if (onKeyUp   != null) onKeyUp(e.keyCode);
                    if (e.keyCode == KeyCode.V) v = false;
                    break;
            }

            if (onCommand != null && e.commandName != "") onCommand(e.commandName);
        }
    }
}