using System;
using UnityEngine;

namespace Dreamteck.Splines.Examples
{
    public class JunctionSwitch : MonoBehaviour
    {
        public Bridge[] bridges;

        private void OnValidate()
        {
            var node        = GetComponent<Node>();
            var connections = node.GetConnections();
            if (bridges == null) return;
            for (var i = 0; i < bridges.Length; i++)
            {
                if (bridges[i].a < 0) bridges[i].a                   = 0;
                if (bridges[i].b < 0) bridges[i].b                   = 0;
                if (bridges[i].a >= connections.Length) bridges[i].a = connections.Length - 1;
                if (bridges[i].b >= connections.Length) bridges[i].b = connections.Length - 1;
            }
        }

        [Serializable]
        public class Bridge
        {
            public enum Direction
            {
                Forward  = 1,
                Backward = -1,
                None     = 0
            }

            public bool      active = true;
            public int       a;
            public Direction aDirection = Direction.None;
            public int       b;
            public Direction bDirection = Direction.None;
        }
    }
}