using System;
using UnityEngine;

namespace StaticClasses
{
    public static class GameObjectPlus
    {
        public static bool CheckReference(this GameObject reference)
        {
            try
            {
                var str = reference.name;
                Debug.Log(str);
            }
            catch (MissingReferenceException) // General Object like GameObject/Sprite etc
            {
                Debug.LogError("The provided reference is missing!");
                return false;
            }
            catch (MissingComponentException) // Specific for objects of type Component
            {
                Debug.LogError("The provided reference is missing!");
                return false;

            }
            catch (UnassignedReferenceException) // Specific for unassigned fields
            {
                Debug.LogWarning("The provided reference is null!");
                return false;

            }
            catch (NullReferenceException) // Any other null reference like for local variables
            {
                Debug.LogWarning("The provided reference is null!");
                return false;

            }

            return true;
        }
    }
}