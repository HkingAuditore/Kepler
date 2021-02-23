namespace Dreamteck.Splines.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class JunctionSwitch : MonoBehaviour
    {
        [System.Serializable]
        public class Bridge
        {
            public enum Direction { Forward = 1, Backward = -1, None = 0 }
            public bool active = true;
            public int a;
            public Direction aDirection = Direction.None;
            public int b;
            public Direction bDirection = Direction.None;
        }

        public Bridge[] bridges;

        private void OnValidate()
        {
            Node node = GetComponent<Node>();
            Node.Connection[] connections = node.GetConnections();
            if (bridges == null) return;
            for (int i = 0; i < bridges.Length; i++)
            {
                if (bridges[i].a < 0) bridges[i].a = 0;
                if (bridges[i].b < 0) bridges[i].b = 0;
                if (bridges[i].a >= connections.Length) bridges[i].a = connections.Length - 1;
                if (bridges[i].b >= connections.Length) bridges[i].b = connections.Length - 1;
            }
        }
    }
}
