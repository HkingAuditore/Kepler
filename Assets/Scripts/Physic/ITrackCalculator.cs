using System.Collections.Generic;
using UnityEngine;

namespace Physic
{
    public interface ITrackCalculator
    {
        List<Vector3> TrackPoints { get; }
        Vector3 CalculateTrack(int t,int totalNumber);
        void ShowTrack(LineRenderer lineRenderer, int totalNumber);
        
    }
}
