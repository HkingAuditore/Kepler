using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck
{
    public static class SceneUtility
    {
        public static void GetChildrenRecursively(Transform current, ref List<Transform> transformList)
        {
            transformList.Add(current);
            foreach (Transform child in current) GetChildrenRecursively(child, ref transformList);
        }
    }
}