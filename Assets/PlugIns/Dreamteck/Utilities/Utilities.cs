using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dreamteck
{
    public static class Utilities
    {
        public static T SerializableClone<T>(this T obj)
        {
            return JsonUtility.FromJson<T>(JsonUtility.ToJson(obj));
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Random.Range(0, n + 1);
                list.Swap(k, n);
            }
        }

        public static void RemoveAtUnsorted<T>(this List<T> list, int i)
        {
            var last = list.Count - 1;
            list[i--] = list[last];
            list.RemoveAt(last);
        }

        public static T PopLast<T>(this IList<T> list)
        {
            var last = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return last;
        }

        public static void Swap<T>(this IList<T> list, int left, int right)
        {
            var value = list[left];
            list[left]  = list[right];
            list[right] = value;
        }

        public static void SafeInvoke(this Delegate del, params object[] parameters)
        {
            foreach (var handler in del.GetInvocationList())
                try
                {
                    handler.Method.Invoke(handler.Target, parameters);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
        }

        public static T PopRandom<T>(this List<T> list)
        {
            if (list.Count > 0)
            {
                var index   = Random.Range(0, list.Count);
                var element = list[index];
                list.RemoveAt(index);
                return element;
            }

            throw new ArgumentException("Attempting to remove an element from an empty list");
        }
    }
}