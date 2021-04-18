using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck
{
    public static class FindDerivedClasses
    {
        public static List<Type> GetAllDerivedClasses(this Type aBaseClass, string[] aExcludeAssemblies)
        {
            var result = new List<Type>();
            foreach (var A in AppDomain.CurrentDomain.GetAssemblies())
            {
                var exclude = false;
                foreach (var S in aExcludeAssemblies)
                    if (A.GetName().FullName.StartsWith(S))
                    {
                        exclude = true;
                        break;
                    }

                if (exclude) continue;
                try
                {
                    if (aBaseClass.IsInterface)
                        foreach (var C in A.GetExportedTypes())
                        {
                            foreach (var I in C.GetInterfaces())
                                if (aBaseClass == I)
                                {
                                    result.Add(C);
                                    break;
                                }
                        }
                    else
                        foreach (var C in A.GetExportedTypes())
                            if (C.IsSubclassOf(aBaseClass))
                                result.Add(C);
                }
                catch
                {
                    Debug.LogWarning("Dreamteck was unable to scan " + A.FullName + " for derived classes");
                }
            }

            return result;
        }

        public static List<Type> GetAllDerivedClasses(this Type aBaseClass)
        {
            return GetAllDerivedClasses(aBaseClass, new string[0]);
        }
    }
}