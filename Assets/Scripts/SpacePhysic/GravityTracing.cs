using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityTracing : MonoBehaviour
{
    private List<AstralBody> _astralBodies = new List<AstralBody>();
    private List<LineRenderer> _orbitRenderers = new List<LineRenderer>();
    
    private Dictionary<AstralBody,Queue<Vector3>> _orbitPoints = new Dictionary<AstralBody, Queue<Vector3>>();
    

    private void Start()
    {
        foreach (AstralBody astralBody in this.GetComponentsInChildren<AstralBody>())
        {
            _astralBodies.Add(astralBody);
            _orbitPoints[astralBody].Enqueue(astralBody.transform.position);
            _orbitRenderers.Add(astralBody.gameObject.GetComponent<LineRenderer>());
        }
    }

    #region 引力步进

    private void CalculateOrbit(int sample)
    {
        Dictionary<AstralBody,Vector3> astralBodyVelocities = new Dictionary<AstralBody, Vector3>();
        
        //起始速度
        foreach (AstralBody astralBody in _astralBodies)
        {
            astralBodyVelocities[astralBody] = astralBody.GetComponent<Rigidbody>().velocity;
        }
        
        for (int i = 0; i < sample; i++)
        {
            
        }
    }
    

    #endregion
    
    
    
}
