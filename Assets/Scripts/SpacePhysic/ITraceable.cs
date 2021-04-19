using System.Collections.Generic;
using UnityEngine;

namespace SpacePhysic
{
    public interface ITraceable
    {
        Transform        GetTransform();
        Vector3          GetPosition();
        GameObject       GetGameObject();
        bool             GetEnableTracing();
        float            GetMass();
        Rigidbody        GetRigidbody();
        Vector3          GetVelocity();
        List<AstralBody> GetAffectedPlanets();

        AstralBody GetAstralBody();
    }
}