using System;
using System.Collections.Generic;
using Dreamteck.Splines;
using SpacePhysic;
using StaticClasses.MathPlus;
using UnityEngine;

namespace CustomUI
{
    public class ConstandOrbit : MonoBehaviour
    {
        public SplineComputer splineComputer;
        public AstralBody     astralBody;
        public GravityTracing orbit;
        
        private Vector3 ConvertV2ToV3(Vector2 vector2)
        {
            return new Vector3(vector2.x, 0, vector2.y);
        }

        private void FixedUpdate()
        {
            DrawMathOrbit(orbit.GetConicSection(astralBody), 20);
        }

        public void DrawMathOrbit(ConicSection conicSection, int sam)
        {
            if (conicSection != null)
            {
                var points = new List<SplinePoint>();
                var step   = 360f / sam;
                for (var i = 0; i < sam; i++)
                    points.Add(new SplinePoint(ConvertV2ToV3(conicSection.GetPolarPos(i * step))));
                points.Add(new SplinePoint(ConvertV2ToV3(conicSection.GetPolarPos(360))));

                splineComputer.SetPoints(points.ToArray());
                splineComputer.Close();
            }
            else
            {
                splineComputer.SetPoints(new[]
                                         {
                                             new SplinePoint(new Vector3(0, 0, 0)),
                                             new SplinePoint(new Vector3(0, 0, 0)),
                                             new SplinePoint(new Vector3(0, 0, 0)),
                                             new SplinePoint(new Vector3(0, 0, 0))
                                         });
                splineComputer.Close();
            }

            // mathOrbitDrawer.positionCount = sam + 1;
            // mathOrbitDrawer.SetPositions(points.ToArray());
        }
    }
}