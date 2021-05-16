using System;
using SpacePhysic;
using UnityEngine;

namespace CustomUI.VR
{
    [Serializable]
    public class StarInfoMap
    {
        public GameObject star;
        public String     starName;
        public String     starMass;
        public String     starRadius;
        public String     starGravity;
        public String     starDayTime;
        public String     starYearTime;
    }
}